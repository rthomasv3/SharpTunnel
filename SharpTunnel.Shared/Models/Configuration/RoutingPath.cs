using SharpTunnel.Shared.Enums;
using System;

namespace SharpTunnel.Shared.Models.Configuration;

[Serializable]
public class RoutingPath
{
    public string Subdomain { get; init; }
    public string Domain { get; init; }
    public string Path { get; init; }
    public ServiceType Type { get; init; }
    public string Url { get; init; }
}
