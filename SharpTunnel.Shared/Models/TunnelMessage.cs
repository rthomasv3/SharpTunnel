using MessagePack;
using SharpTunnel.Shared.Enums;
using SharpTunnel.Shared.Models.Configuration;

namespace SharpTunnel.Shared.Models;

[MessagePackObject]
public class TunnelMessage
{
    [Key(0)]
    public string TraceIdentifier { get; init; }
    [Key(1)]
    public TunnelMessageType MessageType { get; init; }
    [Key(2)]
    public string Name { get; init; }
    [Key(3)]
    public ConfigurationMessage Configuration{ get; init; }
    [Key(4)]
    public WebRequest Request { get; init; }
    [Key(5)]
    public WebResponse Response { get; init; }
}
