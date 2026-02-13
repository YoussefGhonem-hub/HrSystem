using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Payroll.Commands.ConfigureEmployeePayroll;
using HrSystem.Application.Features.Payroll.Queries.GetMyLoans;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslips;
using HrSystem.Application.Features.Payroll.Queries.GetMyPayslipDetails;
using HrSystem.Application.Features.Payroll.Queries.GetMySalarySummary;
using HrSystem.Application.Features.Payroll.Queries.GetMyNetSalaryStatus;
using HrSystem.Application.Features.Payroll.Queries.GetMySalaryBreakdown;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollSummary;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipsList;
using HrSystem.Application.Features.Payroll.Queries.GetMyPaymentDetails;
using HrSystem.Application.Features.Payroll.Queries.GetPayrollOverview;
using HrSystem.Application.Features.Payroll.Queries.GetPayslipsWithStatistics;
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
}
