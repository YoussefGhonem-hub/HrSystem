using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTaskById;
using HrSystem.Application.Features.Lifecycle.OnboardingTasks.Queries.GetOnboardingTasksList;
using HrSystem.Domain.Entities.Lifecycle;
using Mapster;

namespace HrSystem.Application.Common.Mappings.Lifecycle;

public class OnboardingTaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OnboardingTask, OnboardingTaskDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<OnboardingTask, OnboardingTaskListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
    }
}
