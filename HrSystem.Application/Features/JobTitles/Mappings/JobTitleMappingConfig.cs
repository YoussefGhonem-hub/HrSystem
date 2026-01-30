using HrSystem.Application.Features.JobTitles.Queries.GetJobTitleById;
using HrSystem.Application.Features.JobTitles.Queries.GetJobTitlesList;
using HrSystem.Domain.Entities.Employee;
using Mapster;

namespace HrSystem.Application.Features.JobTitles.Mappings;

public class JobTitleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JobTitle, JobTitleDto>()
            .Map(dest => dest.EmployeeCount, src => src.Employees != null ? src.Employees.Count : 0);

        config.NewConfig<JobTitle, JobTitleListDto>()
            .Map(dest => dest.EmployeeCount, src => src.Employees != null ? src.Employees.Count : 0);
    }
}
