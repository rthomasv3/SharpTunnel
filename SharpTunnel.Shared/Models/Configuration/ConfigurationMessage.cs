using MessagePack;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models.Configuration;

[MessagePackObject]
public class ConfigurationMessage
{
    [Key(0)]
    public List<RoutingPath> Paths { get; set; }
}
