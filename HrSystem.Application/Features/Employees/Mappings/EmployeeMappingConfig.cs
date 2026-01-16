using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployee;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Employees.Queries.GetEmployeesList;
using HrSystem.Domain.Entities.Employee;
using Mapster;

namespace HrSystem.Application.Features.Employees.Mappings;

public class EmployeeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Employee, EmployeeDto>()
            .Map(dest => dest.DepartmentNameEn, src => src.Department.NameEn)
            .Map(dest => dest.DepartmentNameAr, src => src.Department.NameAr)
            .Map(dest => dest.JobTitleEn, src => src.JobTitle.TitleEn)
            .Map(dest => dest.JobTitleAr, src => src.JobTitle.TitleAr)
            .Map(dest => dest.DirectManagerName, src => src.DirectManager != null ? src.DirectManager.FullNameEn : null)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.FullNameAr, src => src.FullNameAr)
            .Map(dest => dest.FullNameEn, src => src.FullNameEn);

        config.NewConfig<Employee, EmployeeListDto>()
            .Map(dest => dest.FullNameEn, src => src.FullNameEn)
            .Map(dest => dest.FullNameAr, src => src.FullNameAr)
            .Map(dest => dest.DepartmentNameEn, src => src.Department.NameEn)
            .Map(dest => dest.JobTitleEn, src => src.JobTitle.TitleEn)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null);

        config.NewConfig<CreateEmployeeDto, Employee>()
            .Map(dest => dest.Status, src => Domain.Enums.EmployeeStatus.Active)
            .Map(dest => dest.ProbationEndDate, src => src.HiringDate.AddMonths(src.ProbationPeriodMonths));

        config.NewConfig<UpdateEmployeeDto, Employee>()
            .IgnoreNonMapped(true);
    }
}
