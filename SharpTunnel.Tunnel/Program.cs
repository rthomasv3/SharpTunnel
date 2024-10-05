using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpTunnel.Shared.Models;

namespace SharpTunnel.Tunnel;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Services.AddHostedService<Worker>();
        builder.Services.AddHttpClient();

        Config config = new();
        builder.Configuration.Bind(config);
        builder.Services.AddSingleton(config);

        var host = builder.Build();
        host.Run();
    }
}