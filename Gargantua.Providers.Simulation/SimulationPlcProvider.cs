using System.Collections.Concurrent;
using Gargantua.Core.Models;
using Gargantua.Providers.Abstractions;

namespace Gargantua.Providers.Simulation;

public sealed class SimulationPlcProvider : IPlcProvider
{
    private readonly ConcurrentDictionary<string, object?> valueByAddressDictionary =
        new ConcurrentDictionary<string, object?>();

    private bool isConnected;

    public SimulationPlcProvider(string plcIdentifier)
    {
        PlcIdentifier = plcIdentifier;
    }

    public string Vendor => "Simulation";

    public string PlcIdentifier { get; }

    public bool IsConnected => isConnected;

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        isConnected = true;
        return Task.CompletedTask;
    }

    public Task DisconnectAsync()
    {
        isConnected = false;
        return Task.CompletedTask;
    }

    public async Task<PlcReadResult> ReadAsync(
        PlcReadRequest plcReadRequest,
        CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var plcReadItemResults = new List<PlcReadItemResult>();

        foreach (PlcAddress plcAddress in plcReadRequest.Addresses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string addressKey = plcAddress.VendorAddress;

            bool addressExists = valueByAddressDictionary.TryGetValue(addressKey, out object? value);

            PlcValueQuality quality = IsConnected
                ? (addressExists ? PlcValueQuality.Good : PlcValueQuality.Bad)
                : PlcValueQuality.NotConnected;

            plcReadItemResults.Add(
                new PlcReadItemResult
                {
                    Address = plcAddress,
                    Value = value,
                    Quality = quality,
                    TimestampUtc = DateTimeOffset.UtcNow
                });
        }

        return new PlcReadResult
        {
            PlcIdentifier = plcReadRequest.PlcIdentifier,
            Items = plcReadItemResults
        };
    }

    public async Task<PlcWriteResult> WriteAsync(
        PlcWriteRequest plcWriteRequest,
        CancellationToken cancellationToken)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }

        var plcWriteItemResults = new List<PlcWriteItemResult>();

        foreach (PlcWriteItem plcWriteItem in plcWriteRequest.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string addressKey = plcWriteItem.Address.VendorAddress;

            valueByAddressDictionary[addressKey] = plcWriteItem.Value;

            plcWriteItemResults.Add(
                new PlcWriteItemResult
                {
                    Address = plcWriteItem.Address,
                    Success = true
                });
        }

        return new PlcWriteResult
        {
            PlcIdentifier = plcWriteRequest.PlcIdentifier,
            Items = plcWriteItemResults
        };
    }

    public ValueTask DisposeAsync()
    {
        isConnected = false;
        valueByAddressDictionary.Clear();
        return ValueTask.CompletedTask;
    }
}
