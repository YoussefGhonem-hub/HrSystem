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
            .Map(dest => dest.FullNameEn, src => src.FullNameEn)
            .Map(dest => dest.GenderNameEn, src => src.Gender != null ? src.Gender.NameEn : null)
            .Map(dest => dest.GenderNameAr, src => src.Gender != null ? src.Gender.NameAr : null)
            .Map(dest => dest.MaritalStatusNameEn, src => src.MaritalStatus != null ? src.MaritalStatus.NameEn : null)
            .Map(dest => dest.MaritalStatusNameAr, src => src.MaritalStatus != null ? src.MaritalStatus.NameAr : null)
            .Map(dest => dest.ContractTypeNameEn, src => src.ContractType != null ? src.ContractType.NameEn : null)
            .Map(dest => dest.ContractTypeNameAr, src => src.ContractType != null ? src.ContractType.NameAr : null)
            .Map(dest => dest.StatusNameEn, src => src.Status != null ? src.Status.NameEn : null)
            .Map(dest => dest.StatusNameAr, src => src.Status != null ? src.Status.NameAr : null);

        config.NewConfig<Employee, EmployeeListDto>()
            .Map(dest => dest.FullNameEn, src => src.FullNameEn)
            .Map(dest => dest.FullNameAr, src => src.FullNameAr)
            .Map(dest => dest.DepartmentNameEn, src => src.Department != null ? src.Department.NameEn : string.Empty)
            .Map(dest => dest.JobTitleEn, src => src.JobTitle != null ? src.JobTitle.TitleEn : string.Empty)
            .Map(dest => dest.BranchName, src => src.Branch != null ? src.Branch.NameEn : null)
            .Map(dest => dest.StatusName, src => src.Status != null ? src.Status.NameEn : string.Empty);
    }
}
