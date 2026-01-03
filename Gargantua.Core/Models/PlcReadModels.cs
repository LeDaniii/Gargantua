using System;

namespace Gargantua.Core.Models;

public sealed class PlcReadRequest
{
    public required string PlcIdentifier { get; init; }

    public required IReadOnlyCollection<PlcAddress> Addresses { get; init; }
}

public sealed class PlcReadItemResult
{
    public required PlcAddress Address { get; init; }

    public required object? Value { get; init; }

    public required PlcValueQuality Quality { get; init; }

    public required DateTimeOffset TimestampUtc { get; init; }
}

public sealed class PlcReadResult
{
    public required string PlcIdentifier { get; init; }

    public required IReadOnlyCollection<PlcReadItemResult> Items { get; init; }
}