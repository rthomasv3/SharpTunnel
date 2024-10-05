using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpTunnel.Shared.Enums;
using SharpTunnel.Shared.Models;

namespace SharpTunnel.Tunnel;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Config _config;
    private readonly HubConnection _hubConnection;

    public Worker(ILogger<Worker> logger, IHttpClientFactory httpClientFactory, Config config)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _config = config;

        _hubConnection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7128/tunnelhub", options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(_config.SignalRBearerToken);
            })
            .AddMessagePackProtocol()
            .WithAutomaticReconnect()
            .Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // start up web socket connection to tunnel api...
        // wait for messages and use a service class to process them and send them on
        // config could come from the socket as well, it could be returned on first connect and then updated any time

        _hubConnection.On<TunnelMessage>("ReceiveMessage", async message =>
        {
            Console.WriteLine($"Got Message {message.TraceIdentifier} - {message.Name}");

            try
            {
                if (message.MessageType == TunnelMessageType.WebRequest)
                {
                    HttpClient client = _httpClientFactory.CreateClient();
                    HttpRequestMessage forwardRequest = BuildRequest(message.Request);
                    HttpResponseMessage localResponse = await client.SendAsync(forwardRequest);

                    TunnelMessage response = await BuildResponse(localResponse, message.TraceIdentifier);
                    await _hubConnection.InvokeAsync("GetResponse", response);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        });

        await _hubConnection.StartAsync(stoppingToken);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                Console.WriteLine("Ping");
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (TaskCanceledException) { }

        await _hubConnection.StopAsync();

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

    private HttpRequestMessage BuildRequest(WebRequest message)
    {
        HttpRequestMessage request = new()
        {
            Method = HttpMethod.Parse(message.Method),
            RequestUri = new Uri(MapRoute(message) + message.Path + message.Query)
        };

        Console.WriteLine($"Building request for: {request.RequestUri.ToString()}");

        if (message.Body?.Length > 0 == true)
        {
            request.Content = new ByteArrayContent(message.Body);
        }
        else if (message.Form != null && message.Form.Count > 0)
        {
            request.Content = new FormUrlEncodedContent(message.Form);
        }

        // Need to think about HTTP/2 connections vs HTTP/1.1 only headers
        // https://github.com/dotnet/aspnetcore/pull/33502/commits
        foreach (KeyValuePair<string, string> header in message.Headers)
        {
            Console.WriteLine($"Header Key: {header.Key}, Value: {header.Value}");

            if ((header.Key != "Content-Type" && header.Key != "Content-Length") || request.Content == null)
            {
                // Add had validation error, maybe something to do with Auth scheme missing, need to fix...
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else
            {
                if (header.Key == "Content-Length")
                {
                    MemoryStream ms = request.Content.ReadAsStream() as MemoryStream;
                    int length = ms.ToArray().Length;
                    request.Content.Headers.Add("Content-Length", length.ToString());
                }
                else
                {
                    request.Content.Headers.TryAddWithoutValidation(header.Key.ToString(), header.Value);
                }
            }
        }

        return request;
    }

    private string MapRoute(WebRequest message)
    {
        return "http://10.0.0.63:13378";
    }

    private async Task<TunnelMessage> BuildResponse(HttpResponseMessage message, string traceId)
    {
        byte[] body = await message.Content.ReadAsByteArrayAsync();
        Dictionary<string, IEnumerable<string>> headers = new();
        Dictionary<string, string> cookies = new();
        string contentType = null;

        foreach (KeyValuePair<string, IEnumerable<string>> header in message.Headers)
        {
            headers[header.Key] = header.Value;

            //https://stackoverflow.com/questions/12373738/how-do-i-set-a-cookie-on-httpclients-httprequestmessage
            // Set-Cookie header might be enough...
            //if (header.Key == "Set-Cookie")
            //{
            //    foreach (string cookie in header.Value)
            //    {
            //        foreach (string value in cookie.Split(';'))
            //        {
            //            if (value.Contains("="))
            //            {
            //                string[] values = value.Split("=");
            //                cookies.Add(values[0], values[1]);
            //            }
            //            else
            //            {
            //                cookies.Add(value, null);
            //            }
            //        }
            //    }
            //}

            if (header.Key == "Content-Type")
            {
                contentType = String.Join(";", header.Value);
            }
        }

        if (message.Content != null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> contentHeader in message.Content.Headers)
            {
                headers[contentHeader.Key] = contentHeader.Value;

                if (contentHeader.Key == "Content-Type")
                {
                    contentType = String.Join(";", contentHeader.Value);
                }
            }
        }

        WebResponse response = new()
        {
            Body = body,
            ContentLength = body.Length,
            ContentType = contentType,
            Cookies = cookies,
            Headers = headers,
            StatusCode = (int)message.StatusCode,
            Path = message.RequestMessage.RequestUri.LocalPath,
            Query = message.RequestMessage.RequestUri.Query,
        };

        return new TunnelMessage()
        {
            MessageType = TunnelMessageType.WebResponse,
            TraceIdentifier = traceId,
            Response = response,
        };
    }
}
