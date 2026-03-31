using ErrorOr;
using HrSystem.Application.Features.Payroll.Commands.CreateBankExportProfile;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Payroll.Queries.GetBankExportProfiles;

public record GetBankExportProfilesQuery : IRequest<ErrorOr<GenericResponse<List<BankExportProfileDto>>>>;

public class GetBankExportProfilesQueryHandler
    : IRequestHandler<GetBankExportProfilesQuery, ErrorOr<GenericResponse<List<BankExportProfileDto>>>>
{
    private readonly ApplicationDbContext _context;

    public GetBankExportProfilesQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<List<BankExportProfileDto>>>> Handle(
        GetBankExportProfilesQuery request,
        CancellationToken cancellationToken)
    {
        var profiles = await _context.BankExportProfiles
            .Where(b => !b.IsDeleted)
            .OrderByDescending(b => b.IsDefault)
            .ThenBy(b => b.ProfileName)
            .Select(b => new BankExportProfileDto
            {
                Id = b.Id,
                ProfileName = b.ProfileName,
                BankName = b.BankName,
                CompanyAccountNumber = b.CompanyAccountNumber,
                CompanyAccountName = b.CompanyAccountName,
                Currency = b.Currency,
                BicCode = b.BicCode,
                Narrative = b.Narrative,
                IsDefault = b.IsDefault,
                CreatedDate = b.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return GenericResponse<List<BankExportProfileDto>>.SuccessResult(profiles, "Bank export profiles retrieved.");
    }
}
