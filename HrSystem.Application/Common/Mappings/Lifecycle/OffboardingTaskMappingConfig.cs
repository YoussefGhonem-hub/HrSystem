using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTaskById;
using HrSystem.Application.Features.Lifecycle.OffboardingTasks.Queries.GetOffboardingTasksList;
using HrSystem.Domain.Entities.Lifecycle;
using Mapster;

namespace HrSystem.Application.Common.Mappings.Lifecycle;

public class OffboardingTaskMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OffboardingTask, OffboardingTaskDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<OffboardingTask, OffboardingTaskListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
    }
}
