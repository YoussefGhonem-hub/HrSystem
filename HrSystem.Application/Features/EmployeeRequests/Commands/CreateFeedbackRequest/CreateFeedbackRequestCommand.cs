using ErrorOr;
using HrSystem.Application.Features.EmployeeRequests.Dtos;
using HrSystem.Domain.Entities.Requests;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.EmployeeRequests.Commands.CreateFeedbackRequest;

public record CreateFeedbackRequestCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    Guid FeedbackTypeId,
    string FeedbackContent,
    bool IsAnonymous,
    int? Rating,
    string? TargetDepartment,
    string? TargetPerson,
    string? SuggestedImprovement,
    bool ResponseRequired,
    string? AttachmentUrl,
    Guid? BranchId
) : IRequest<ErrorOr<GenericResponse<EmployeeRequestDto>>>;

public class CreateFeedbackRequestCommandHandler
    : IRequestHandler<CreateFeedbackRequestCommand, ErrorOr<GenericResponse<EmployeeRequestDto>>>
{
    private static readonly EmployeeRequestStatus[] OpenStatuses =
    {
        EmployeeRequestStatus.Draft,
        EmployeeRequestStatus.Pending
    };

    private readonly ApplicationDbContext _context;

    public CreateFeedbackRequestCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse<EmployeeRequestDto>>> Handle(
        CreateFeedbackRequestCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Error.NotFound(description: "Employee not found.");

        var requestType = await _context.RequestTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Code == "Feedback", cancellationToken);

        if (requestType == null)
            return Error.NotFound(description: "Feedback request type not configured.");

        var branchId = request.BranchId ?? employee.BranchId;
        if (!branchId.HasValue)
            return Error.Validation(description: "BranchId is required.");

        var branchSetting = await _context.BranchRequestSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                s => s.BranchId == branchId && s.RequestTypeId == requestType.Id,
                cancellationToken);

        if (branchSetting == null || !branchSetting.AllowEmployeesToSubmit)
            return Error.Forbidden(description: "Feedback requests are not allowed for this branch.");

        if (branchSetting.RequireAttachment && string.IsNullOrWhiteSpace(request.AttachmentUrl))
            return Error.Validation(description: "An attachment is required.");

        if (branchSetting.MaxOpenRequests.HasValue)
        {
            var openCount = await _context.EmployeeRequests
                .CountAsync(r => r.EmployeeId == request.EmployeeId
                                 && r.RequestTypeId == requestType.Id
                                 && OpenStatuses.Contains(r.Status), cancellationToken);

            if (openCount >= branchSetting.MaxOpenRequests.Value)
                return Error.Validation(description: "Maximum open feedback requests reached.");
        }

        var feedbackType = await _context.FeedbackTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.FeedbackTypeId && t.IsActive, cancellationToken);

        if (feedbackType == null)
            return Error.Validation(description: "Invalid feedback type.");

        var employeeRequest = new EmployeeRequest
        {
            RequestTypeId = requestType.Id,
            Status = EmployeeRequestStatus.Pending,
            EmployeeId = request.EmployeeId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            AttachmentUrl = request.AttachmentUrl,
            BranchId = branchId,
            TenantId = employee.TenantId,
            RequestedDate = DateTime.UtcNow
        };

        var detail = new FeedbackRequestDetail
        {
            FeedbackTypeId = request.FeedbackTypeId,
            FeedbackContent = request.FeedbackContent.Trim(),
            IsAnonymous = request.IsAnonymous,
            Rating = request.Rating,
            TargetDepartment = request.TargetDepartment,
            TargetPerson = request.TargetPerson,
            SuggestedImprovement = request.SuggestedImprovement,
            ResponseRequired = request.ResponseRequired
        };

        employeeRequest.FeedbackDetail = detail;

        await _context.EmployeeRequests.AddAsync(employeeRequest, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeRequestDto
        {
            Id = employeeRequest.Id,
            RequestTypeId = requestType.Id,
            RequestTypeName = requestType.Code,
            Status = employeeRequest.Status,
            EmployeeId = employeeRequest.EmployeeId,
            EmployeeName = request.IsAnonymous ? null : $"{employee.FirstNameEn} {employee.LastNameEn}",
            BranchId = branchId,
            Title = employeeRequest.Title,
            Description = employeeRequest.Description,
            RequestedDate = employeeRequest.RequestedDate,
            AttachmentUrl = employeeRequest.AttachmentUrl,
            FeedbackDetail = new FeedbackDetailDto
            {
                FeedbackTypeId = detail.FeedbackTypeId,
                FeedbackTypeName = feedbackType.NameEn,
                FeedbackContent = detail.FeedbackContent,
                IsAnonymous = detail.IsAnonymous,
                Rating = detail.Rating,
                TargetDepartment = detail.TargetDepartment,
                TargetPerson = detail.TargetPerson,
                SuggestedImprovement = detail.SuggestedImprovement,
                ResponseRequired = detail.ResponseRequired
            }
        };

        return GenericResponse<EmployeeRequestDto>.SuccessResult(dto, "Feedback request submitted successfully.");
    }
}
