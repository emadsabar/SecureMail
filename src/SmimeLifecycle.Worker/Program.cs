using Hangfire;
using Serilog;
using SmimeLifecycle.Application;
using SmimeLifecycle.Infrastructure;
using SmimeLifecycle.Infrastructure.Jobs;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var host = builder.Build();

RecurringJob.AddOrUpdate<LifecycleJobs>("poll-issuance", job => job.PollIssuanceAsync(), "*/1 * * * *");
RecurringJob.AddOrUpdate<LifecycleJobs>("publish-ad", job => job.PublishAsync(), "*/2 * * * *");

await host.RunAsync();
