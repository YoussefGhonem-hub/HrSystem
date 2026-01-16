using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentById;
using HrSystem.Application.Features.Lifecycle.PolicyAcknowledgments.Queries.GetPolicyAcknowledgmentsList;
using HrSystem.Domain.Entities.Lifecycle;
using Mapster;

namespace HrSystem.Application.Common.Mappings.Lifecycle;

public class PolicyAcknowledgmentMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PolicyAcknowledgment, PolicyAcknowledgmentDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<PolicyAcknowledgment, PolicyAcknowledgmentListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
    }
}
