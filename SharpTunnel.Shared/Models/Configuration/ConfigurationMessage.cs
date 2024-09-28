using System;
using System.Collections.Generic;

namespace SharpTunnel.Shared.Models.Configuration;

[Serializable]
public class ConfigurationMessage
{
    public List<RoutingPath> Paths { get; set; }
}
