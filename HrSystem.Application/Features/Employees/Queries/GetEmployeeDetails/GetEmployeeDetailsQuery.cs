using ErrorOr;
using HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Leave.Queries.GetMyLeaveBalances;
using HrSystem.Domain.Enums;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;

public record GetEmployeeDetailsQuery(Guid EmployeeId, int AttendanceRecentCount = 10, int? LeaveBalanceYear = null)
    : IRequest<ErrorOr<GenericResponse<EmployeeDetailsDto>>>;

public class GetEmployeeDetailsQueryHandler : IRequestHandler<GetEmployeeDetailsQuery, ErrorOr<GenericResponse<EmployeeDetailsDto>>>
{
    private readonly ApplicationDbContext _context;
    private readonly IStorageService _storageService;

    public GetEmployeeDetailsQueryHandler(ApplicationDbContext context, IStorageService storageService)
    {
        _context = context;
        _storageService = storageService;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDetailsDto>>> Handle(GetEmployeeDetailsQuery request, CancellationToken cancellationToken)
    {
        var employeeDto = await GetEmployeeDtoAsync(request.EmployeeId, cancellationToken);
        if (employeeDto is null)
        {
            return Error.NotFound(description: "Employee not found");
        }

        var payroll = await GetEmployeePayrollSummaryAsync(request.EmployeeId, cancellationToken);
        var attendance = await GetAttendanceSectionAsync(request.EmployeeId, 30, cancellationToken);

        var year = request.LeaveBalanceYear ?? DateTime.UtcNow.Year;
        var leaveBalances = await GetLeaveBalancesAsync(request.EmployeeId, year, cancellationToken);

        var documents = await GetEmployeeDocumentsAsync(request.EmployeeId, cancellationToken);
        var assets = await GetEmployeeAssetsAsync(request.EmployeeId, cancellationToken);

        var dto = new EmployeeDetailsDto
        {
            Employee = employeeDto,
            Payroll = payroll,
            Attendance = attendance,
            LeaveBalances = leaveBalances,
            Documents = documents,
            Assets = assets
        };

        return new GenericResponse<EmployeeDetailsDto>
        {
            Success = true,
            Message = "Employee details retrieved successfully",
            Data = dto
        };
    }

    private async Task<EmployeeDto?> GetEmployeeDtoAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.DirectManager)
            .Include(e => e.Branch)
            .Include(e => e.ContractType)
            .Include(e => e.Status)
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);

        if (employee == null)
        {
            return null;
        }

        return new EmployeeDto
        {
            Id = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            FirstNameAr = employee.FirstNameAr,
            LastNameAr = employee.LastNameAr,
            FirstNameEn = employee.FirstNameEn,
            LastNameEn = employee.LastNameEn,
            FullNameAr = employee.FullNameAr,
            FullNameEn = employee.FullNameEn,
            NationalId = employee.NationalId,
            PassportNumber = employee.PassportNumber,
            DateOfBirth = employee.DateOfBirth,
            GenderId = employee.GenderId,
            GenderNameEn = employee.Gender?.NameEn,
            GenderNameAr = employee.Gender?.NameAr,
            MaritalStatusId = employee.MaritalStatusId,
            MaritalStatusNameEn = employee.MaritalStatus?.NameEn,
            MaritalStatusNameAr = employee.MaritalStatus?.NameAr,
            Email = employee.Email,
            PhoneNumber = employee.PhoneNumber,
            MobileNumber = employee.MobileNumber,
            AddressAr = employee.AddressAr,
            AddressEn = employee.AddressEn,
            City = employee.City,
            Country = employee.Country,
            Nationality = employee.Country,
            DepartmentId = employee.DepartmentId,
            DepartmentNameEn = employee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = employee.Department?.NameAr ?? string.Empty,
            JobTitleId = employee.JobTitleId,
            JobTitleEn = employee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = employee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = employee.DirectManagerId,
            DirectManagerName = employee.DirectManager?.FullNameEn,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.NameEn,
            ContractTypeId = employee.ContractTypeId,
            ContractTypeNameEn = employee.ContractType?.NameEn,
            ContractTypeNameAr = employee.ContractType?.NameAr,
            StatusId = employee.StatusId,
            StatusNameEn = employee.Status?.NameEn,
            StatusNameAr = employee.Status?.NameAr,
            HiringDate = employee.HiringDate,
            ProbationEndDate = employee.ProbationEndDate,
            ProbationPeriodMonths = employee.ProbationPeriodMonths,
            TerminationDate = employee.TerminationDate,
            TerminationReason = employee.TerminationReason,
            ProfilePictureUrl = employee.ProfilePictureUrl,
            CreatedDate = employee.CreatedDate.DateTime
        };
    }

    private async Task<EmployeePayrollSummaryDto> GetEmployeePayrollSummaryAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        // Latest payslip for summary
        var latestPayslip = await _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .FirstOrDefaultAsync(cancellationToken);

        // History (latest 12)
        var history = await _context.Payslips
            .Include(p => p.PayrollCycle)
            .Where(p => p.EmployeeId == employeeId)
            .OrderByDescending(p => p.PayrollCycle.Year)
            .ThenByDescending(p => p.PayrollCycle.Month)
            .ThenByDescending(p => p.GeneratedDate)
            .Take(12)
            .Select(p => new EmployeePayslipHistoryItemDto
            {
                PayslipId = p.Id,
                CycleName = p.PayrollCycle.CycleName,
                Year = p.PayrollCycle.Year,
                Month = p.PayrollCycle.Month,
                GrossSalary = p.GrossSalary,
                TotalDeductions = p.TotalDeductions,
                NetSalary = p.NetSalary,
                Status = p.IsPaid ? "Paid" : "Processing"
            })
            .ToListAsync(cancellationToken);

        var payDayDescription = latestPayslip?.PayrollCycle?.PaymentDate is DateTime pd
            ? $"{GetOrdinal(pd.Day)} of every month"
            : null;

        var employeeScope = await _context.Employees
            .Where(e => e.Id == employeeId)
            .Select(e => new
            {
                e.TenantId,
                BranchCurrency = e.Branch != null ? e.Branch.Currency : null
            })
            .FirstOrDefaultAsync(cancellationToken);

        var currency = employeeScope?.BranchCurrency;
        var tenantId = employeeScope?.TenantId;

        if (string.IsNullOrWhiteSpace(currency) && tenantId.HasValue)
        {
            currency = await _context.Organizations
                .Where(o => o.Id == tenantId.Value)
                .Select(o => o.Currency)
                .FirstOrDefaultAsync(cancellationToken);
        }

        currency ??= string.Empty;

        return new EmployeePayrollSummaryDto
        {
            PayslipId = latestPayslip?.Id,
            Year = latestPayslip?.PayrollCycle?.Year,
            Month = latestPayslip?.PayrollCycle?.Month,
            GrossSalary = latestPayslip?.GrossSalary ?? 0m,
            TotalDeductions = latestPayslip?.TotalDeductions ?? 0m,
            NetSalary = latestPayslip?.NetSalary ?? 0m,
            GeneratedDate = latestPayslip?.GeneratedDate,
            IsPaid = latestPayslip?.IsPaid ?? false,
            PaidDate = latestPayslip?.PaidDate,
            PaymentMethod = null, // Not available in schema yet
            PayDayDescription = payDayDescription,
            Currency = currency,
            History = history
        };
    }

    private static string GetOrdinal(int day)
    {
        if (day <= 0) return day.ToString();
        var suffix = day % 100 is 11 or 12 or 13 ? "th" : (day % 10) switch
        {
            1 => "st",
            2 => "nd",
            3 => "rd",
            _ => "th"
        };
        return $"{day}{suffix}";
    }

    private async Task<EmployeeAttendanceSectionDto> GetAttendanceSectionAsync(Guid employeeId, int count, CancellationToken cancellationToken)
    {
        var baseQuery = _context.Attendances
            .Include(a => a.Status)
            .Where(a => a.EmployeeId == employeeId);

        var history = await baseQuery
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CheckInTime)
            .Take(30)
            .Select(a => new EmployeeAttendanceHistoryItemDto
            {
                Id = a.Id,
                Date = a.Date,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                StatusId = a.StatusId,
                StatusNameEn = a.Status.NameEn,
                StatusNameAr = a.Status.NameAr,
                WorkedHours = a.WorkedHours
            })
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var monthData = await baseQuery
            .Where(a => a.Date.Year == now.Year && a.Date.Month == now.Month)
            .Select(a => new { a.IsLate, a.OvertimeHours, StatusNameEn = a.Status.NameEn })
            .ToListAsync(cancellationToken);

        var presentDays = monthData.Count(x => x.StatusNameEn == "Present");
        var absentDays = monthData.Count(x => x.StatusNameEn == "Absent");
        var lateDays = monthData.Count(x => x.IsLate);
        var overtimeHours = monthData.Where(x => x.OvertimeHours.HasValue).Sum(x => x.OvertimeHours!.Value.TotalHours);

        return new EmployeeAttendanceSectionDto
        {
            PresentDays = presentDays,
            LateDays = lateDays,
            AbsentDays = absentDays,
            OvertimeHours = Math.Round(overtimeHours, 2),
            History = history
        };
    }

    private async Task<List<LeaveBalanceDto>> GetLeaveBalancesAsync(Guid employeeId, int year, CancellationToken cancellationToken)
    {
        var balances = await _context.LeaveBalances
            .Include(lb => lb.LeavePolicy)
                .ThenInclude(lp => lp.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .OrderBy(lb => lb.LeavePolicy.LeaveType.DisplayOrder)
            .ThenBy(lb => lb.LeavePolicy.NameEn)
            .Select(lb => new LeaveBalanceDto
            {
                LeavePolicyId = lb.LeavePolicyId,
                LeavePolicyNameEn = lb.LeavePolicy.NameEn,
                LeavePolicyNameAr = lb.LeavePolicy.NameAr,
                LeaveTypeId = lb.LeavePolicy.LeaveTypeId,
                LeaveTypeNameEn = lb.LeavePolicy.LeaveType.NameEn,
                LeaveTypeNameAr = lb.LeavePolicy.LeaveType.NameAr,
                Year = lb.Year,
                TotalDays = lb.TotalDays,
                UsedDays = lb.UsedDays,
                RemainingDays = lb.RemainingDays,
                CarriedForwardDays = lb.CarriedForwardDays
            })
            .ToListAsync(cancellationToken);

        if (balances.Count == 0)
        {
            return balances;
        }

        var policyIds = balances
            .Select(b => b.LeavePolicyId)
            .Distinct()
            .ToList();

        if (policyIds.Count == 0)
        {
            return balances;
        }

        var leaveHistory = await _context.LeaveRequests
            .Include(lr => lr.LeaveStatus)
            .Where(lr => lr.EmployeeId == employeeId
                         && policyIds.Contains(lr.LeavePolicyId)
                         && (lr.StartDate.Year == year || lr.EndDate.Year == year))
            .OrderByDescending(lr => lr.StartDate)
            .Select(lr => new LeaveRequestHistoryDto
            {
                LeaveRequestId = lr.Id,
                LeavePolicyId = lr.LeavePolicyId,
                StartDate = lr.StartDate,
                EndDate = lr.EndDate,
                TotalDays = lr.TotalDays,
                StatusNameEn = lr.LeaveStatus.NameEn,
                StatusNameAr = lr.LeaveStatus.NameAr,
                Reason = lr.Reason,
                ApprovedDate = lr.HRApprovalDate ?? lr.ManagerApprovalDate,
                ManagerComments = lr.ManagerComments,
                HRComments = lr.HRComments
            })
            .ToListAsync(cancellationToken);

        if (leaveHistory.Count == 0)
        {
            return balances;
        }

        var historyLookup = leaveHistory
            .GroupBy(h => h.LeavePolicyId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var balance in balances)
        {
            if (historyLookup.TryGetValue(balance.LeavePolicyId, out var history))
            {
                balance.History = history;
            }
        }

        return balances;
    }

    private async Task<List<EmployeeDocumentDto>> GetEmployeeDocumentsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var rawDocuments = await _context.EmployeeDocuments
            .AsNoTracking()
            .Where(d => d.EmployeeId == employeeId)
            .OrderByDescending(d => d.CreatedDate)
            .Select(d => new
            {
                d.Id,
                d.DocumentName,
                d.DocumentType,
                d.FileUrl,
                d.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        var documents = rawDocuments.Select(d =>
        {
            var info = d.DocumentType.GetInfo();
            return new EmployeeDocumentDto
            {
                Id = d.Id,
                DocumentName = d.DocumentName,
                DocumentTypeNameEn = info.NameEn,
                DocumentTypeNameAr = info.NameAr,
                FileUrl = d.FileUrl,
                ExpiryDate = d.ExpiryDate
            };
        }).ToList();

        // Generate presigned URLs from S3
        foreach (var doc in documents)
        {
            if (!string.IsNullOrEmpty(doc.FileUrl))
            {
                doc.FileUrl = await _storageService.DownloadFileUrl(doc.FileUrl, cancellationToken);
            }
        }

        return documents;
    }

    private async Task<List<EmployeeAssetDto>> GetEmployeeAssetsAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        return await _context.EmployeeAssets
            .Where(a => a.EmployeeId == employeeId)
            .OrderByDescending(a => a.AssignedDate)
            .Select(a => new EmployeeAssetDto
            {
                Id = a.Id,
                AssetType = a.AssetType,
                AssetName = a.AssetName,
                SerialNumber = a.SerialNumber,
                Model = a.Model,
                AssignedDate = a.AssignedDate,
                ExpectedReturnDate = a.ExpectedReturnDate,
                IsReturned = a.IsReturned
            })
            .ToListAsync(cancellationToken);
    }
}
