using System;

namespace Gargantua.Core.Models;

public sealed class PlcAddress
{
    public required string PlcIdentifier { get; init; }

    public required string VendorAddress { get; init; }

    public required PlcDataType DataType { get; init; }
}
