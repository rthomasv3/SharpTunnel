using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SharpTunnel.Web.Services;

namespace SharpTunnel;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<TunnelService>();

        builder.Services.AddControllers();

        var app = builder.Build();

        app.UseHttpsRedirection();
        app.UseWebSockets();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}
