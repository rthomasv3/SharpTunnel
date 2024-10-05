using MessagePack;
using SharpTunnel.Shared.Enums;

namespace SharpTunnel.Shared.Models.Configuration;

[MessagePackObject]
public class RoutingPath
{
    [Key(0)]
    public string Subdomain { get; init; }
    [Key(1)]
    public string Domain { get; init; }
    [Key(2)]
    public string Path { get; init; }
    [Key(3)]
    public ServiceType Type { get; init; }
    [Key(4)]
    public string Url { get; init; }
}
