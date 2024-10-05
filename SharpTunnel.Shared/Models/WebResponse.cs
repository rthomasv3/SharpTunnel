using MessagePack;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models;

[MessagePackObject]
public class WebResponse
{
    [Key(0)]
    public byte[] Body { get; init; }
    [Key(1)]
    public long? ContentLength { get; init; }
    [Key(2)]
    public string ContentType { get; init; }
    [Key(3)]
    public Dictionary<string, string> Cookies { get; init; }
    [Key(4)]
    public Dictionary<string, IEnumerable<string>> Headers { get; init; }
    [Key(5)]
    public int StatusCode { get; init; }
    [Key(6)]
    public string Path { get; init; }
    [Key(7)]
    public string Query { get; init; }
}
