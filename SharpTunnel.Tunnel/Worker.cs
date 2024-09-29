using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpTunnel.Shared.Enums;
using SharpTunnel.Shared.Models;

namespace SharpTunnel.Tunnel;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // start up web socket connection to tunnel api...
        // wait for messages and use a service class to process them and send them on
        // config could come from the socket as well, it could be returned on first connect and then updated any time

        // await Task.Delay(5000);


        HubConnection hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7128/tunnelhub")
            .WithAutomaticReconnect()
            .Build();

        hubConnection.On<TunnelMessage>("ReceiveMessage", async message =>
        {
            Console.WriteLine($"Got Message {message.TraceIdentifier} - {message.Name}");

            //https://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
            //HttpClient client = _httpClientFactory.CreateClient();
            //HttpRequestMessage forwardRequest = new HttpRequestMessage(HttpMethod.Get, "")
            //{
                
            //};
            //foreach (KeyValuePair<string, string> header in message.Request.Headers)
            //{
            //    forwardRequest.Headers.Add(header.Key, header.Value);
            //}
            //HttpResponseMessage localResponse = await client.SendAsync(forwardRequest);

            TunnelMessage response = new()
            {
                TraceIdentifier = message.TraceIdentifier,
                Name = "This is a test of SignalR responding",
                MessageType = TunnelMessageType.WebResponse,
                Response = new()
                {
                    Url = message.Request.Path
                }
            };
            await hubConnection.InvokeAsync("SendMessage", response);
        });

        await hubConnection.StartAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            Console.WriteLine("Ping");
            await Task.Delay(5000, stoppingToken);
        }

        await hubConnection.StopAsync();

        //TunnelMessage message = new()
        //{
        //    TraceIdentifier = Guid.NewGuid().ToString(),
        //    Name = "This is a test of SignalR"
        //};
        //await hubConnection.InvokeAsync("SendMessage", message);


        //int port = 5178; // 5178 or 7128

        //ClientWebSocket clientWebSocket = new();
        //try
        //{
        //    await clientWebSocket.ConnectAsync(new Uri($"ws://localhost:{port}/api/tunnel/start"), stoppingToken);

        //    byte[] messageData = System.Text.Encoding.UTF8.GetBytes("Hello, world!");
        //    await clientWebSocket.SendAsync(
        //            new ArraySegment<byte>(messageData, 0, messageData.Length),
        //            WebSocketMessageType.Binary,
        //            true,
        //            CancellationToken.None);
        //}
        //catch (Exception ex)
        //{
        //    Console.Error.WriteLine(ex.ToString());
        //}

        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    var buffer = new byte[1024 * 4];
        //    var receiveResult = await clientWebSocket.ReceiveAsync(
        //        new ArraySegment<byte>(buffer), stoppingToken);

        //    while (!receiveResult.CloseStatus.HasValue && !stoppingToken.IsCancellationRequested)
        //    {
        //        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer));
        //        await Task.Delay(1000, stoppingToken);

        //        await clientWebSocket.SendAsync(
        //            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
        //            receiveResult.MessageType,
        //            receiveResult.EndOfMessage,
        //            CancellationToken.None);

        //        receiveResult = await clientWebSocket.ReceiveAsync(
        //            new ArraySegment<byte>(buffer), CancellationToken.None);
        //    }

        //    await clientWebSocket.CloseAsync(
        //        receiveResult.CloseStatus.Value,
        //        receiveResult.CloseStatusDescription,
        //        CancellationToken.None);
        //}
    }
}
