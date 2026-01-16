using HrSystem.Application.Features.JobTitles.DTOs;
using HrSystem.Domain.Entities.Employee;
using Mapster;

namespace HrSystem.Application.Features.JobTitles.Mappings;

public class JobTitleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<JobTitle, JobTitleDto>()
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count);

        config.NewConfig<JobTitle, JobTitleListDto>()
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count);

        config.NewConfig<CreateJobTitleDto, JobTitle>();
        config.NewConfig<UpdateJobTitleDto, JobTitle>()
            .IgnoreNonMapped(true);
    }
}
