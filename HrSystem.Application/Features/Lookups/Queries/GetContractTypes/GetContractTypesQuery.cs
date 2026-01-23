using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lookups.Queries.GetContractTypes;

/// <summary>
/// Query to get all active contract types for dropdown
/// </summary>
public record GetContractTypesQuery : IRequest<ErrorOr<GenericResponse<List<ContractTypeDto>>>>;

public class GetContractTypesQueryHandler : IRequestHandler<GetContractTypesQuery, ErrorOr<GenericResponse<List<ContractTypeDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetContractTypesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<ContractTypeDto>>>> Handle(
        GetContractTypesQuery request,
        CancellationToken cancellationToken)
    {
        var types = await _context.ContractTypes
            .Where(t => t.IsActive)
            .OrderBy(t => t.DisplayOrder)
            .Select(t => new ContractTypeDto
            {
                Id = t.Id,
                NameEn = t.NameEn,
                NameAr = t.NameAr,
                Description = t.Description,
                DisplayOrder = t.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return new GenericResponse<List<ContractTypeDto>>
        {
            Success = true,
            Message = "Contract types retrieved successfully",
            Data = types
        };
    }
}

public class ContractTypeDto
{
    public Guid Id { get; set; }
    public string NameEn { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
}
