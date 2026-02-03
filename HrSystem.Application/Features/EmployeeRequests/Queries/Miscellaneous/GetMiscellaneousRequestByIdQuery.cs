using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Miscellaneous;

public record GetMiscellaneousRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetMiscellaneousRequestByIdQueryHandler : IRequestHandler<GetMiscellaneousRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetMiscellaneousRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetMiscellaneousRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.MiscellaneousDetail)
                .ThenInclude(m => m!.MiscellaneousType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Miscellaneous", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Miscellaneous request not found.");

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
            MiscellaneousDetail = employeeRequest.MiscellaneousDetail != null ? new MiscellaneousDetailDto
            {
                MiscellaneousTypeId = employeeRequest.MiscellaneousDetail.MiscellaneousTypeId,
                MiscellaneousTypeName = employeeRequest.MiscellaneousDetail.MiscellaneousType?.NameEn,
                AdditionalNotes = employeeRequest.MiscellaneousDetail.AdditionalNotes,
                ReferenceNumber = employeeRequest.MiscellaneousDetail.ReferenceNumber,
                Priority = employeeRequest.MiscellaneousDetail.Priority,
                ExpectedCompletionDate = employeeRequest.MiscellaneousDetail.ExpectedCompletionDate
            } : null
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Miscellaneous request retrieved successfully");
    }
}
