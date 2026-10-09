using Serilog;
using SmimeLifecycle.WindowsAgent;

Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddSerilog();
builder.Services.Configure<AgentOptions>(builder.Configuration.GetSection(AgentOptions.SectionName));
builder.Services.AddHttpClient<LifecycleApiClient>();
builder.Services.AddSingleton<LocalKeyAndCertificateService>();
builder.Services.AddHostedService<EnrollmentWorker>();

var host = builder.Build();
await host.RunAsync();
