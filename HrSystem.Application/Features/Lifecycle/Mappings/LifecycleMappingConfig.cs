using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetById;
using HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTasksList;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;
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

        config.NewConfig<OffboardingTask, OffboardingTaskDto>()
    .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<OffboardingTask, OffboardingTaskListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
        config.NewConfig<OnboardingTask, OnboardingTaskDto>()
    .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<OnboardingTask, OnboardingTaskListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);

        config.NewConfig<PolicyAcknowledgment, PolicyAcknowledgmentDto>()
    .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<PolicyAcknowledgment, PolicyAcknowledgmentListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
    }
}
