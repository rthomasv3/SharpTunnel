using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharpTunnel.Web.Hubs;
using SharpTunnel.Web.Services;

namespace SharpTunnel;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<TunnelService>();

        builder.Services.AddControllers();

        builder.Services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.MaximumReceiveMessageSize = long.MaxValue; // default is 32768
        });

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();

        app.MapHub<TunnelHub>("/tunnelhub");

        app.Run();
    }
}
