using System.Security.Cryptography;
using System.Text;
using ErrorOr;
using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Attendance.Commands.EnrollEmployeeBiometric;

public class EnrollEmployeeBiometricCommandHandler : IRequestHandler<EnrollEmployeeBiometricCommand, ErrorOr<GenericResponse<EmployeeBiometricDto>>>
{
    private readonly ApplicationDbContext _context;

    public EnrollEmployeeBiometricCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeBiometricDto>>> Handle(
        EnrollEmployeeBiometricCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var isHr = CurrentUser.Roles?.Contains(RoleNames.HRManager) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.HRSpecialist) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.OrganizationAdmin) == true ||
                   CurrentUser.Roles?.Contains(RoleNames.SuperAdmin) == true;

        if (!isHr)
        {
            var currentEmployeeId = CurrentUser.EmployeeId;
            if (!currentEmployeeId.HasValue || currentEmployeeId.Value != request.EmployeeId)
            {
                return Error.Unauthorized("Biometric.Unauthorized", "Not allowed to enroll biometric");
            }
        }

        var templateHash = ComputeTemplateHash(request.TemplateBase64);

        var existing = await _context.EmployeeBiometrics
            .FirstOrDefaultAsync(b => b.EmployeeId == request.EmployeeId && b.BiometricType == request.BiometricType, cancellationToken);

        if (existing != null)
        {
            existing.TemplateHash = templateHash;
            existing.TemplateData = request.TemplateBase64;
            existing.Provider = request.Provider;
            existing.DeviceId = request.DeviceId;
            existing.IsActive = request.IsActive;
            existing.EnrolledAt = DateTimeOffset.UtcNow;
            existing.MarkAsModified(CurrentUser.Id ?? Guid.Empty);
        }
        else
        {
            var biometric = new EmployeeBiometric
            {
                EmployeeId = request.EmployeeId,
                BiometricType = request.BiometricType,
                TemplateHash = templateHash,
                TemplateData = request.TemplateBase64,
                Provider = request.Provider,
                DeviceId = request.DeviceId,
                IsActive = request.IsActive,
                EnrolledAt = DateTimeOffset.UtcNow,
                TenantId = employee.TenantId
            };

            biometric.MarkAsCreated(CurrentUser.Id ?? Guid.Empty);

            _context.EmployeeBiometrics.Add(biometric);
        }

        await _context.SaveChangesAsync(cancellationToken);

        var saved = await _context.EmployeeBiometrics
            .AsNoTracking()
            .FirstAsync(b => b.EmployeeId == request.EmployeeId && b.BiometricType == request.BiometricType, cancellationToken);

        var dto = new EmployeeBiometricDto
        {
            Id = saved.Id,
            EmployeeId = saved.EmployeeId,
            BiometricType = saved.BiometricType,
            Provider = saved.Provider,
            DeviceId = saved.DeviceId,
            IsActive = saved.IsActive,
            EnrolledAt = saved.EnrolledAt
        };

        return new GenericResponse<EmployeeBiometricDto>
        {
            Success = true,
            Message = "Biometric enrolled successfully",
            Data = dto
        };
    }

    private static string ComputeTemplateHash(string templateBase64)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(templateBase64.Trim());
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
