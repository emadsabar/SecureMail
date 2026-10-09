using Hangfire;
using Microsoft.Identity.Web;
using Serilog;
using SmimeLifecycle.Application;
using SmimeLifecycle.Infrastructure;
using SmimeLifecycle.Infrastructure.Jobs;
using SmimeLifecycle.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration).WriteTo.Console());

    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "SmimeLifecycle API",
            Version = "v1",
            Description = "S/MIME certificate lifecycle management. Not a mail gateway, relay, DLP, or archive."
        });
    });
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("admin", policy =>
            policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:5173"])
                .AllowAnyHeader()
                .AllowAnyMethod());
    });

    var authMode = builder.Configuration.GetValue<string>("Authentication:Mode") ?? "Development";
    if (string.Equals(authMode, "EntraId", StringComparison.OrdinalIgnoreCase))
    {
        builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
        builder.Services.AddAuthorization();
    }
    else
    {
        builder.Services.AddAuthorization(options => options.AddPolicy("default", p => p.RequireAssertion(_ => true)));
    }

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();
    }

    app.UseMiddleware<SmimeLifecycle.Api.Middleware.ExceptionMappingMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseCors("admin");
    app.UseSwagger();
    app.UseSwaggerUI();
    if (string.Equals(authMode, "EntraId", StringComparison.OrdinalIgnoreCase))
    {
        app.UseAuthentication();
    }

    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/health", () => Results.Ok(new { status = "ok", product = "SmimeLifecycle" }));
    if (!app.Configuration.GetValue("Hangfire:Disabled", false))
    {
        app.UseHangfireDashboard("/hangfire");
        RecurringJob.AddOrUpdate<LifecycleJobs>(
            "poll-issuance",
            job => job.PollIssuanceAsync(),
            "*/1 * * * *");
        RecurringJob.AddOrUpdate<LifecycleJobs>(
            "publish-ad",
            job => job.PublishAsync(),
            "*/2 * * * *");
    }

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}

public partial class Program;
