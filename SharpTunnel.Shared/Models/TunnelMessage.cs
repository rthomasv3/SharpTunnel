using System;
using SharpTunnel.Shared.Enums;

namespace SharpTunnel.Shared.Models;

[Serializable]
public class TunnelMessage
{
    public TunnelMessageType MessageType { get; init; }
}
