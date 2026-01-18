using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gargantua.Providers.Siemens;

public sealed record SiemensProviderOptions(
    string IpAddress,
    string CpuTypeName,
    short Rack,
    short Slot,
    string PlcIdentifier);

public static class SiemensServiceCollectionExtensions
{
    public static IServiceCollection AddSiemensProvider(this IServiceCollection services, SiemensProviderOptions options)
    {
        services.AddSingleton(options);

        services.AddSingleton<Internal.SiemensTcpDriver>(serviceProvider =>
        {
            ILogger<Internal.SiemensTcpDriver> logger = serviceProvider.GetRequiredService<ILogger<Internal.SiemensTcpDriver>>();
            return new Internal.SiemensTcpDriver(
                ipAddress: options.IpAddress,
                cpuType: Internal.SiemensCpuTypeMapper.MapFromName(options.CpuTypeName),
                logger: logger,
                rack: options.Rack,
                slot: options.Slot);
        });

        services.AddSingleton<Internal.SiemensPlcConnectionService>(serviceProvider =>
        {
            Internal.SiemensTcpDriver driver = serviceProvider.GetRequiredService<Internal.SiemensTcpDriver>();
            ILogger<Internal.SiemensPlcConnectionService> logger = serviceProvider.GetRequiredService<ILogger<Internal.SiemensPlcConnectionService>>();
            return new Internal.SiemensPlcConnectionService(driver, logger, options.PlcIdentifier);
        });

        services.AddSingleton<Internal.ISiemensPlcStateInfo>(serviceProvider =>
            serviceProvider.GetRequiredService<Internal.SiemensPlcConnectionService>());

        services.AddHostedService(serviceProvider =>
            serviceProvider.GetRequiredService<Internal.SiemensPlcConnectionService>());

        services.AddSingleton<Gargantua.Providers.Abstractions.IPlcProvider, SiemensPlcProvider>();

        return services;
    }
}
