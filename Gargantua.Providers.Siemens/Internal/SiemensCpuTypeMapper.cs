using System;
using S7.Net;

namespace Gargantua.Providers.Siemens.Internal;

public static class SiemensCpuTypeMapper
{
    internal static CpuType MapFromName(string cpuTypeName)
    {
        return cpuTypeName switch
        {
            "S7-1200" => CpuType.S71200,
            "S7-1500" => CpuType.S71500,
            "S7-300" => CpuType.S7300,
            "S7-400" => CpuType.S7400,
            _ => throw new ArgumentOutOfRangeException(nameof(cpuTypeName), cpuTypeName, "Unknown Siemens CPU type name.")
        };
    }
}
