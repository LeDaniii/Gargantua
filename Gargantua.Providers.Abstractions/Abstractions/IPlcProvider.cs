using System;
using Gargantua.Core.Models;

namespace Gargantua.Providers.Abstractions;

public interface IPlcProvider : IAsyncDisposable
{
    string Vendor { get; }

    string PlcIdentifier { get; }

    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken cancellationToken);

    Task DisconnectAsync();

    Task<PlcReadResult> ReadAsync(
        PlcReadRequest plcReadRequest,
        CancellationToken cancellationToken);

    Task<PlcWriteResult> WriteAsync(
        PlcWriteRequest plcWriteRequest,
        CancellationToken cancellationToken);
}

