using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharpTunnel.Tunnel;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // start up web socket connection to tunnel api...
        // wait for messages and use a service class to process them and send them on
        // config could come from the socket as well, it could be returned on first connect and then updated any time

        // await Task.Delay(5000);

        int port = 5178; // 5178 or 7128

        ClientWebSocket clientWebSocket = new();
        try
        {
            await clientWebSocket.ConnectAsync(new Uri($"ws://localhost:{port}/api/tunnel/start"), stoppingToken);
            byte[] messageData = System.Text.Encoding.UTF8.GetBytes("Hello, world!");
            await clientWebSocket.SendAsync(
                    new ArraySegment<byte>(messageData, 0, messageData.Length),
                    WebSocketMessageType.Binary,
                    true,
                    CancellationToken.None);
        }
        catch (Exception ex)
        {

        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var buffer = new byte[1024 * 4];
            var receiveResult = await clientWebSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), stoppingToken);

            while (!receiveResult.CloseStatus.HasValue && !stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer));
                await Task.Delay(1000, stoppingToken);

                await clientWebSocket.SendAsync(
                    new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                    receiveResult.MessageType,
                    receiveResult.EndOfMessage,
                    CancellationToken.None);

                receiveResult = await clientWebSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await clientWebSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
    }
}
