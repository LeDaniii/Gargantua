using System;

namespace Gargantua.Core.Models.PlcBlocks;

public class BlockDefinition
{

}

public enum PlcPrimitiveType
{
    Bool,
    Int16,
    UInt16,
    Int32,
    Real32,
    S7String
}

public sealed record PlcBlockFieldDefinition(
    string FieldName,
    PlcPrimitiveType PrimitiveType,
    string Address,
    int? StringLength = null,
    string? Comment = null);

public sealed record PlcBlockDefinition(
    string PlcIdentifier,
    string BlockName,
    int DataBlockNumber,
    IReadOnlyList<PlcBlockFieldDefinition> Fields);