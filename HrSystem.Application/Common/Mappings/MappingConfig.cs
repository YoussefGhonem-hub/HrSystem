using HrSystem.Application.Features.Performance.Mappings;
using Mapster;

namespace HrSystem.Application.Common.Mappings;

public static class MappingConfig
{
    public static void Register(TypeAdapterConfig config)
    {
        // Register Performance mappings
        new PerformanceMappingConfig().Register(config);
    }
}
