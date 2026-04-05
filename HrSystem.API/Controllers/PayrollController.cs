using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Application.Features.Payroll.Commands.CreateBankExportProfile;
using HrSystem.Application.Features.Payroll.Commands.DeleteBankExportProfile;
using HrSystem.Application.Features.Payroll.Commands.DeletePayslips;
using HrSystem.Application.Features.Payroll.Commands.GeneratePayslips;
using HrSystem.Application.Features.Payroll.Commands.MarkPayslipsAsPaid;
using HrSystem.Application.Features.Payroll.Commands.UpdateBankExportProfile;
using HrSystem.Application.Features.Payroll.Commands.UploadBankProfileTemplate;
using HrSystem.Application.Features.Payroll.Queries.GetMyLoans;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipDetails;
using HrSystem.Application.Features.Payroll.Queries.GeneratePayslipPdf;
using HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;
using HrSystem.Application.Features.Payroll.Queries.GetMyNetSalaryStatus;
using HrSystem.Application.Features.Payroll.Queries.GetMySalaryBreakdown;
using HrSystem.Application.Features.Payroll.Queries.ExportBankFile;
using HrSystem.Application.Features.Payroll.Queries.GetBankExportProfiles;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollSummary;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipsList;
using HrSystem.Application.Features.Payroll.Queries.GetMyPaymentDetails;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PayrollController : APIBaseController
{
    private readonly ISender _mediator;

    public PayrollController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Generate payslips for a given month/year. Includes overtime from approved requests and loan deductions.
    /// Optionally pass employeeId to generate for a single employee.
    /// Only HR Managers and Admins can generate payslips.
    /// </summary>
    [HttpPost("generate-payslips")]
    [Authorize(Roles = "HRManager,OrganizationAdmin,SuperAdmin")]
    public async Task<IActionResult> GeneratePayslips([FromBody] GeneratePayslipsCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Delete payslips for a given month/year. Optionally pass employeeId to delete for a single employee.
    /// Only HR Managers and Admins can delete payslips.
    /// </summary>
    [HttpDelete("delete-payslips")]
    [Authorize(Roles = "HRManager,OrganizationAdmin,SuperAdmin")]
    public async Task<IActionResult> DeletePayslips([FromQuery] int month, [FromQuery] int year, [FromQuery] Guid? employeeId = null)
    {
        var command = new DeletePayslipsCommand(month, year, employeeId);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Configure payroll settings (salary, allowances, deductions, payment method) for a specific employee
    /// </summary>
    [HttpPost("employees/{employeeId:guid}/configuration")]
    public async Task<IActionResult> ConfigureEmployeePayroll(Guid employeeId, [FromBody] ConfigureEmployeePayrollCommand command)
    {
        if (employeeId != command.EmployeeId)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get transfer/payment details for the currently logged-in user
    /// </summary>
    [HttpGet("my-payment-details")]
    public async Task<IActionResult> GetMyPaymentDetails([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetMyPaymentDetailsQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get salary breakdown (earnings, deductions, totals) for current or given month
    /// </summary>
    [HttpGet("my-salary-breakdown")]
    public async Task<IActionResult> GetMySalaryBreakdown([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetMySalaryBreakdownQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get only net salary and current month paid status for the logged-in user
    /// </summary>
    [HttpGet("my-net-salary")]
    public async Task<IActionResult> GetMyNetSalaryStatus([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetMyNetSalaryStatusQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get gross and net salary for the currently logged-in user
    /// </summary>
    [HttpGet("my-salary")]
    public async Task<IActionResult> GetMySalary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetMySalarySummaryQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payroll payslip history with filters and pagination
    /// - HR/Admin can filter by employee; others restricted to own data
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetPayslipsHistory(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetPayslipsListQuery(
            employeeId,
            year,
            month,
            isPaid,
            fromDate,
            toDate,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get aggregated payroll summary (employees paid, gross, deductions, net)
    /// </summary>
    [HttpGet("summary")]
    public async Task<IActionResult> GetPayrollSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
    {
        var result = await _mediator.Send(new GetPayrollSummaryQuery(year, month));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payroll overview with statistics and department-wise breakdown
    /// For HR/Admin: View payroll overview by department with filters
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year (e.g., 2026)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="departmentId">Filter by department</param>
    /// <param name="branchId">Filter by branch</param>
    /// <param name="status">Filter by status (Pending, Processed)</param>
    /// <param name="searchTerm">Search term</param>
    /// <param name="sortBy">Sort by field (DepartmentName, EmployeeCount, GrossSalary, NetSalary, Status)</param>
    /// <param name="sortDescending">Sort descending</param>
    [HttpGet("overview")]
    public async Task<IActionResult> GetPayrollOverview(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "DepartmentName",
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetPayrollOverviewQuery(
            month,
            year,
            pageNumber,
            pageSize,
            departmentId,
            branchId,
            status,
            searchTerm,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payslips with statistics and employee-wise details
    /// For HR/Admin: View all employee payslips with filters and statistics
    /// </summary>
    /// <param name="month">Month (1-12)</param>
    /// <param name="year">Year (e.g., 2026)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="employeeId">Filter by employee</param>
    /// <param name="departmentId">Filter by department</param>
    /// <param name="isPaid">Filter by payment status</param>
    /// <param name="searchTerm">Search term (employee code, name, payslip number)</param>
    /// <param name="sortBy">Sort by field (EmployeeCode, EmployeeName, Department, GrossSalary, NetSalary, Status)</param>
    /// <param name="sortDescending">Sort descending</param>
    [HttpGet("payslips")]
    public async Task<IActionResult> GetPayslips(
        [FromQuery] int month,
        [FromQuery] int year,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = "EmployeeCode",
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetPayslipsWithStatisticsQuery(
            month,
            year,
            pageNumber,
            pageSize,
            employeeId,
            departmentId,
            isPaid,
            searchTerm,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payroll history for all employees with filters and pagination.
    /// Returns a paginated grid of payslips with employee info and full salary breakdown per month.
    /// </summary>
    /// <param name="year">Filter by year (e.g., 2025)</param>
    /// <param name="month">Filter by month (1-12)</param>
    /// <param name="employeeId">Filter by specific employee</param>
    /// <param name="departmentId">Filter by department</param>
    /// <param name="isPaid">Filter by payment status</param>
    /// <param name="searchTerm">Search by employee code, name, or payslip number</param>
    /// <param name="sortBy">Sort field (EmployeeCode, EmployeeName, Department, NetSalary, GrossSalary, Month, Status)</param>
    /// <param name="sortDescending">Sort descending (default: true)</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    [HttpGet("payroll-history")]
    public async Task<IActionResult> GetPayrollHistory(
        [FromQuery] int? year = null,
        [FromQuery] int? month = null,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] bool? isPaid = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetPayrollHistoryQuery(
            year,
            month,
            employeeId,
            departmentId,
            isPaid,
            searchTerm,
            sortBy,
            sortDescending,
            pageNumber,
            pageSize);

        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get loans list for the currently logged-in user
    /// </summary>
    [HttpGet("my-loans")]
    public async Task<IActionResult> GetMyLoans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMyLoansQuery(pageNumber, pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get loan details for the currently logged-in user
    /// </summary>
    [HttpGet("my-loans/{loanId:guid}")]
    public async Task<IActionResult> GetMyLoanDetails(Guid loanId)
    {
        var result = await _mediator.Send(new GetMyLoanDetailsQuery(loanId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payslips list for the currently logged-in user
    /// </summary>
    [HttpGet("my-payslips")]
    public async Task<IActionResult> GetMyPayslips(
        [FromQuery] int? year = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetMyPayslipsQuery(year, pageNumber, pageSize));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get payslip full details for the currently logged-in user
    /// </summary>
    [HttpGet("my-payslips/{payslipId:guid}")]
    public async Task<IActionResult> GetMyPayslipDetails(Guid payslipId)
    {
        var result = await _mediator.Send(new GetMyPayslipDetailsQuery(payslipId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get full payslip details for HR/Admin - can view any employee's payslip
    /// </summary>
    [HttpGet("payslips/{payslipId:guid}")]
    public async Task<IActionResult> GetPayslipDetails(Guid payslipId)
    {
        var result = await _mediator.Send(new GetPayslipDetailsQuery(payslipId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Generate and download PDF for a specific payslip
    /// </summary>
    [HttpGet("payslips/{payslipId:guid}/pdf")]
    public async Task<IActionResult> GeneratePayslipPdf(Guid payslipId)
    {
        var result = await _mediator.Send(new GeneratePayslipPdfQuery(payslipId));

        return result.Match(
            pdfBytes => File(pdfBytes, "application/pdf", $"Payslip_{payslipId}.pdf"),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Export bank transfer file (CIB Excel format) for unpaid payslips in a given month/year.
    /// Uses the specified bank export profile, or the default profile if none specified.
    /// </summary>
    [HttpGet("export-bank-file")]
    public async Task<IActionResult> ExportBankFile([FromQuery] int month, [FromQuery] int year, [FromQuery] Guid? profileId)
    {
        var result = await _mediator.Send(new ExportBankFileQuery(month, year, profileId));

        return result.Match(
            response =>
            {
                if (response.Data == null)
                    return Ok(response);
                return File(response.Data.FileContent, response.Data.ContentType, response.Data.FileName);
            },
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Mark payslips as paid for a given month/year. Optionally specify payslip IDs to mark specific ones.
    /// </summary>
    [HttpPost("mark-as-paid")]
    public async Task<IActionResult> MarkPayslipsAsPaid([FromBody] MarkPayslipsAsPaidCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    // ═══════════════════════════════════════════
    // Bank Export Profile Management
    // ═══════════════════════════════════════════

    /// <summary>
    /// Get all bank export profiles.
    /// </summary>
    [HttpGet("bank-export-profiles")]
    public async Task<IActionResult> GetBankExportProfiles()
    {
        var result = await _mediator.Send(new GetBankExportProfilesQuery());
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Create a new bank export profile.
    /// </summary>
    [HttpPost("bank-export-profiles")]
    public async Task<IActionResult> CreateBankExportProfile([FromBody] CreateBankExportProfileCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing bank export profile.
    /// </summary>
    [HttpPut("bank-export-profiles/{id:guid}")]
    public async Task<IActionResult> UpdateBankExportProfile(Guid id, [FromBody] UpdateBankExportProfileCommand command)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch.");

        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Delete a bank export profile.
    /// </summary>
    [HttpDelete("bank-export-profiles/{id:guid}")]
    public async Task<IActionResult> DeleteBankExportProfile(Guid id)
    {
        var result = await _mediator.Send(new DeleteBankExportProfileCommand(id));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Upload an Excel template for a bank export profile.
    /// When exporting, data will be filled into this template instead of generating from scratch.
    /// </summary>
    [HttpPost("bank-export-profiles/{id:guid}/template")]
    public async Task<IActionResult> UploadBankProfileTemplate(Guid id, IFormFile templateFile)
    {
        var result = await _mediator.Send(new UploadBankProfileTemplateCommand(id, templateFile));
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
