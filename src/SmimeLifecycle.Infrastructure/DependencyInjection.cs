using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmimeLifecycle.Application.Abstractions;
using SmimeLifecycle.Infrastructure.CertificateAuthority;
using SmimeLifecycle.Infrastructure.Certificates;
using SmimeLifecycle.Infrastructure.Directory;
using SmimeLifecycle.Infrastructure.Identity;
using SmimeLifecycle.Infrastructure.Jobs;
using SmimeLifecycle.Infrastructure.Persistence;

namespace SmimeLifecycle.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
                               ?? "Host=localhost;Port=5432;Database=smimelifecycle;Username=smime;Password=smime";

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAuditService, AuditService>();
        services.AddSingleton<IPublicCertificateParser, PublicCertificateParser>();
        services.AddScoped<LifecycleJobs>();

        services.Configure<SectigoOptions>(configuration.GetSection(SectigoOptions.SectionName));
        services.Configure<ActiveDirectoryOptions>(configuration.GetSection(ActiveDirectoryOptions.SectionName));
        services.Configure<IdentityDirectoryOptions>(configuration.GetSection(IdentityDirectoryOptions.SectionName));

        var caProvider = configuration.GetValue<string>("CertificateAuthority:Provider") ?? "Development";
        if (string.Equals(caProvider, "Sectigo", StringComparison.OrdinalIgnoreCase))
        {
            services.AddHttpClient<ICertificateAuthority, SectigoCertificateAuthority>();
        }
        else
        {
            services.AddSingleton<ICertificateAuthority, DevelopmentCertificateAuthority>();
        }

        var directoryProvider = configuration.GetValue<string>("ActiveDirectory:Provider") ?? "Null";
        if (string.Equals(directoryProvider, "Ldaps", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IActiveDirectoryPublisher, LdapsActiveDirectoryPublisher>();
        }
        else
        {
            services.AddSingleton<IActiveDirectoryPublisher, NullActiveDirectoryPublisher>();
        }

        var identityProvider = configuration.GetValue<string>("IdentityDirectory:Provider") ?? "Configured";
        if (string.Equals(identityProvider, "ActiveDirectory", StringComparison.OrdinalIgnoreCase))
        {
            services.AddSingleton<IIdentityDirectory, ActiveDirectoryIdentityDirectory>();
        }
        else
        {
            services.AddSingleton<IIdentityDirectory, ConfiguredIdentityDirectory>();
        }

        if (!configuration.GetValue("Hangfire:Disabled", false))
        {
            services.AddHangfire(cfg => cfg
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));
            services.AddHangfireServer();
        }

        return services;
    }
}
