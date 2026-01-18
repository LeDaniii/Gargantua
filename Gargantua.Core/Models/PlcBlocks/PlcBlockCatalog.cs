using System;

namespace Gargantua.Core.Models.PlcBlocks;

public interface IPlcBlockCatalog
{
    PlcBlockDefinition GetBlock(string plcIdentifier, string blockName);
}

public class PlcBlockCatalog
{

}
