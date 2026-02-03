using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Personal;

public record GetPersonalRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetPersonalRequestByIdQueryHandler : IRequestHandler<GetPersonalRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetPersonalRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetPersonalRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.PersonalDetail)
                .ThenInclude(p => p!.PersonalType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Personal", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Personal request not found.");

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = employeeRequest.RequestTypeId,
            RequestTypeName = employeeRequest.RequestTypeRef?.Code ?? "",
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = employeeRequest.Employee?.FullNameEn,
            BranchId = employeeRequest.BranchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            StartDate = employeeRequest.StartDate,
            EndDate = employeeRequest.EndDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            ManagerComments = employeeRequest.ManagerComments,
            RejectionReason = employeeRequest.RejectionReason,
            ApprovedBy = employeeRequest.ApprovedBy,
            ApprovedDate = employeeRequest.ApprovedDate,
            ProcessedBy = employeeRequest.ProcessedBy,
            ProcessedDate = employeeRequest.ProcessedDate,
            PersonalDetail = employeeRequest.PersonalDetail != null ? new PersonalDetailDto
            {
                PersonalTypeId = employeeRequest.PersonalDetail.PersonalTypeId,
                PersonalTypeName = employeeRequest.PersonalDetail.PersonalType?.NameEn,
                Reason = employeeRequest.PersonalDetail.Reason ?? string.Empty,
                IsUrgent = employeeRequest.PersonalDetail.IsUrgent,
                RequiresConfidentiality = employeeRequest.PersonalDetail.RequiresConfidentiality,
                PreferredContactMethod = employeeRequest.PersonalDetail.PreferredContactMethod,
                AdditionalContactInfo = employeeRequest.PersonalDetail.AdditionalContactInfo
            } : null
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Personal request retrieved successfully");
    }
}
