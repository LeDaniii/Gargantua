using System;

namespace Gargantua.Core.Models;

public sealed class PlcWriteItem
{
    public required PlcAddress Address { get; init; }

    public required object? Value { get; init; }
}

public sealed class PlcWriteRequest
{
    public required string PlcIdentifier { get; init; }

    public required IReadOnlyCollection<PlcWriteItem> Items { get; init; }
}

public sealed class PlcWriteItemResult
{
    public required PlcAddress Address { get; init; }

    public required bool Success { get; init; }

    public string? ErrorMessage { get; init; }
}

public sealed class PlcWriteResult
{
    public required string PlcIdentifier { get; init; }

    public required IReadOnlyCollection<PlcWriteItemResult> Items { get; init; }
}