using HrSystem.Application.Features.Departments.Queries.GetDepartmentById;
using HrSystem.Application.Features.Departments.Queries.GetDepartmentsList;
using HrSystem.Domain.Entities.Employee;
using Mapster;

namespace HrSystem.Application.Features.Departments.Mappings;

public class DepartmentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Department, DepartmentDto>()
            .Map(dest => dest.ManagerName, src => src.Manager != null ? src.Manager.FullNameEn : null)
            .Map(dest => dest.ParentDepartmentName, src => src.ParentDepartment != null ? src.ParentDepartment.NameEn : null)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count);

        config.NewConfig<Department, DepartmentListDto>()
            .Map(dest => dest.ManagerName, src => src.Manager != null ? src.Manager.FullNameEn : null)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count);
    }
}
