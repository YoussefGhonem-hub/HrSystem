using HrSystem.Application.Features.Branches.Commands.CreateBranch;
using HrSystem.Application.Features.Branches.Commands.UpdateBranch;
using HrSystem.Application.Features.Branches.Queries.GetBranchById;
using HrSystem.Application.Features.Branches.Queries.GetBranchesList;
using HrSystem.Domain.Entities.Organization;
using Mapster;

namespace HrSystem.Application.Features.Branches.Mappings;

public class BranchMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Branch, BranchDto>()
            .Map(dest => dest.BranchManagerName, src => src.BranchManager != null ? src.BranchManager.FullNameEn : null)
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count)
            .Map(dest => dest.DepartmentCount, src => src.Departments.Count);

        config.NewConfig<Branch, BranchListDto>()
            .Map(dest => dest.EmployeeCount, src => src.Employees.Count);

        config.NewConfig<CreateBranchDto, Branch>()
            .Map(dest => dest.IsActive, src => true);

        config.NewConfig<UpdateBranchDto, Branch>()
            .IgnoreNonMapped(true);
    }
}
