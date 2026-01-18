using System;

namespace Gargantua.Providers.Siemens.Internal;

public enum PlcConnectionState
{
    Idle,
    Connecting,
    Connected,
    Disconnected,
    Error
}
