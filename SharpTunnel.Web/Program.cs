using MessagePack;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpTunnel.Shared.Models;
using SharpTunnel.Web.Hubs;
using SharpTunnel.Web.MIddleware;
using SharpTunnel.Web.Services;

namespace SharpTunnel;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<TunnelService>();

        Config config = new();
        builder.Configuration.Bind(config);
        builder.Services.AddSingleton(config);

        builder.Services.AddControllers();

        builder.Services
            .AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.MaximumReceiveMessageSize = long.MaxValue; // default is 32768
            })
            //.AddMessagePackProtocol(options =>
            //{
            //    options.SerializerOptions = MessagePackSerializerOptions.Standard
            //        .WithSecurity(MessagePackSecurity.UntrustedData);
            //})
            .AddMessagePackProtocol();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        app.UseMiddleware<SignalRAuthMiddleware>();

        app.MapHub<TunnelHub>("/tunnelhub");

        app.Run();
    }
}
