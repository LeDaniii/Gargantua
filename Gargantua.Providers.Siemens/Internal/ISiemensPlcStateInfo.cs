using System;

namespace Gargantua.Providers.Siemens.Internal;

public interface ISiemensPlcStateInfo
{
    string PlcIdentifier { get; }
    PlcConnectionState CurrentState { get; }

    event Action<PlcConnectionState>? StateChanged;

    void ResetError();
}
