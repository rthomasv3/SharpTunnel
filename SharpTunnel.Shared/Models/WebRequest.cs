using System;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models;

[Serializable]
public class WebRequest
{
    public string Method { get; init; }
    public string Path { get; init; }
    public string Scheme { get; init; }
    public string Host { get; init; }
    public byte[] Body { get; init; }
    public string Query { get; init; }
    public Dictionary<string, string> Headers { get; init; }
    public bool IsWebSocketRequest { get; init; }
}
