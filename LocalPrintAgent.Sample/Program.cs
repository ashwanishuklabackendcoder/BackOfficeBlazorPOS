using LocalPrintAgent.Sample;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AgentOptions>(builder.Configuration);
builder.Services.AddHostedService<PrintAgentWorker>();

builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "LocalPrintAgent";
});

var host = builder.Build();
await host.RunAsync();
