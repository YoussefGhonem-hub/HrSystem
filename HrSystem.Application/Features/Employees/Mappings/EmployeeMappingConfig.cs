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
            .Map(dest => dest.DepartmentNameEn, src => src.Department != null ? src.Department.NameEn : string.Empty)
            .Map(dest => dest.DepartmentNameAr, src => src.Department != null ? src.Department.NameAr : string.Empty)
            .Map(dest => dest.JobTitleEn, src => src.JobTitle != null ? src.JobTitle.TitleEn : string.Empty)
            .Map(dest => dest.JobTitleAr, src => src.JobTitle != null ? src.JobTitle.TitleAr : string.Empty)
            .Map(dest => dest.DirectManagerName, src => src.DirectManager != null ? src.DirectManager.FullNameEn : null)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.FullNameAr, src => src.FullNameAr)
            .Map(dest => dest.FullNameEn, src => src.FullNameEn);

        config.NewConfig<Employee, EmployeeListDto>()
            .Map(dest => dest.FullNameEn, src => src.FullNameEn)
            .Map(dest => dest.FullNameAr, src => src.FullNameAr)
            .Map(dest => dest.DepartmentNameEn, src => src.Department != null ? src.Department.NameEn : string.Empty)
            .Map(dest => dest.JobTitleEn, src => src.JobTitle != null ? src.JobTitle.TitleEn : string.Empty)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.StatusName, src => src.Status != null ? src.Status.NameEn : string.Empty);
    }
}
