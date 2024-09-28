using SharpTunnel.Shared.Enums;
using SharpTunnel.Shared.Models.Configuration;
using System;

namespace SharpTunnel.Shared.Models;

[Serializable]
public class TunnelMessage
{
    public string TraceIdentifier { get; init; }
    public TunnelMessageType MessageType { get; init; }
    public string Name { get; init; }
    public ConfigurationMessage Configuration{ get; init; }
    public WebRequest Request { get; init; }
    public WebResponse Response { get; init; }
}
