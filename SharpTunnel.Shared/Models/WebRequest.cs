using MessagePack;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models;

[MessagePackObject]
public class WebRequest
{
    [Key(0)]
    public string Method { get; init; }
    [Key(1)]
    public string Path { get; init; }
    [Key(2)]
    public string Scheme { get; init; }
    [Key(3)]
    public string Host { get; init; }
    [Key(4)]
    public byte[] Body { get; init; }
    [Key(5)]
    public string Query { get; init; }
    [Key(6)]
    public Dictionary<string, string> Headers { get; init; }
    [Key(7)]
    public Dictionary<string, string> Cookies { get; init; }
    [Key(8)]
    public bool IsWebSocketRequest { get; init; }
    [Key(9)]
    public long? ContentLength { get; init; }
    [Key(10)]
    public string ContentType { get; init; }
    [Key(11)]
    public Dictionary<string, string> Form { get; init; }
}
