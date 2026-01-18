using System;
using System.Threading;
using System.Threading.Tasks;
using Gargantua.Core.Models;
using Gargantua.Providers.Abstractions;
using Gargantua.Providers.Siemens.Internal;

namespace Gargantua.Providers.Siemens;

public sealed class SiemensPlcProvider : IPlcProvider
{
    private readonly SiemensTcpDriver _siemensTcpDriver;
    private readonly ISiemensPlcStateInfo _siemensPlcStateInfo;

    public SiemensPlcProvider(
        SiemensTcpDriver siemensTcpDriver,
        ISiemensPlcStateInfo siemensPlcStateInfo)
    {
        _siemensTcpDriver = siemensTcpDriver;
        _siemensPlcStateInfo = siemensPlcStateInfo;
    }

    public string Vendor => "Siemens";

    public string PlcIdentifier => _siemensPlcStateInfo.PlcIdentifier;

    public bool IsConnected => _siemensPlcStateInfo.CurrentState == PlcConnectionState.Connected;

    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        bool connected = await _siemensTcpDriver.ConnectAsync(cancellationToken);
        if (!connected)
        {
            throw new InvalidOperationException("Siemens PLC connection failed.");
        }
    }

    public Task DisconnectAsync()
    {
        return _siemensTcpDriver.DisconnectAsync();
    }

    public Task<PlcReadResult> ReadAsync(PlcReadRequest plcReadRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<PlcWriteResult> WriteAsync(PlcWriteRequest plcWriteRequest, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        return new ValueTask(_siemensTcpDriver.DisconnectAsync());
    }
}
