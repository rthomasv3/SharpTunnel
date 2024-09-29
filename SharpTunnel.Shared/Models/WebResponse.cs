using System;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models;

[Serializable]
public class WebResponse
{
    public byte[] Body { get; init; }
    public long? ContentLength { get; init; }
    public string ContentType { get; init; }
    public IEnumerable<string> Cookies { get; init; }
    public Dictionary<string, IEnumerable<string>> Headers { get; init; }
    public int StatusCode { get; init; }
    public string Path { get; init; }
    public string Query { get; init; }
}
