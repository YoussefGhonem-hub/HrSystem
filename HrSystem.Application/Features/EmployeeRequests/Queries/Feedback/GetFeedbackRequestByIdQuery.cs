using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Queries.Feedback;

public record GetFeedbackRequestByIdQuery(Guid Id) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class GetFeedbackRequestByIdQueryHandler : IRequestHandler<GetFeedbackRequestByIdQuery, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private readonly ApplicationDbContext _context;

    public GetFeedbackRequestByIdQueryHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        GetFeedbackRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employeeRequest = await _context.EmployeeRequests
            .Include(r => r.RequestTypeRef)
            .Include(r => r.Employee)
            .Include(r => r.FeedbackDetail)
                .ThenInclude(f => f!.FeedbackType)
            .FirstOrDefaultAsync(r => r.Id == request.Id && r.RequestTypeRef != null && r.RequestTypeRef.Code == "Feedback", cancellationToken);

        if (employeeRequest == null)
            return Error.NotFound(description: "Feedback request not found.");

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = employeeRequest.RequestTypeId,
            RequestTypeName = employeeRequest.RequestTypeRef?.Code ?? "",
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = employeeRequest.FeedbackDetail?.IsAnonymous == true ? "Anonymous" : employeeRequest.Employee?.FullNameEn,
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
            FeedbackDetail = employeeRequest.FeedbackDetail != null ? new FeedbackDetailDto
            {
                FeedbackTypeId = employeeRequest.FeedbackDetail.FeedbackTypeId,
                FeedbackTypeName = employeeRequest.FeedbackDetail.FeedbackType?.NameEn,
                FeedbackContent = employeeRequest.FeedbackDetail.FeedbackContent,
                IsAnonymous = employeeRequest.FeedbackDetail.IsAnonymous,
                Rating = employeeRequest.FeedbackDetail.Rating,
                TargetDepartment = employeeRequest.FeedbackDetail.TargetDepartment,
                TargetPerson = employeeRequest.FeedbackDetail.TargetPerson,
                SuggestedImprovement = employeeRequest.FeedbackDetail.SuggestedImprovement,
                ResponseRequired = employeeRequest.FeedbackDetail.ResponseRequired,
                ResponseContent = employeeRequest.FeedbackDetail.ResponseContent,
                ResponseDate = employeeRequest.FeedbackDetail.ResponseDate
            } : null
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Feedback request retrieved successfully");
    }
}
