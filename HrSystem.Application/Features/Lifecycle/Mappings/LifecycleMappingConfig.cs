using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;
using HrSystem.Domain.Entities.Lifecycle;
using Mapster;

namespace HrSystem.Application.Features.Lifecycle.Mappings;

public class LifecycleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<EmployeeAsset, EmployeeAssetDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<EmployeeAsset, EmployeeAssetListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);

    }
}
