using HrSystem.API.Controllers.Shared;
using HrSystem.Application.Features.Employees.Commands.AssignDirectManager;
using HrSystem.Application.Features.Employees.Commands.AddEmployeeSalary;
using HrSystem.Application.Features.Employees.Commands.CreateEmployee;
using HrSystem.Application.Features.Employees.Commands.DeleteEmployee;
using HrSystem.Application.Features.Employees.Commands.SyncEmployeeAssets;
using HrSystem.Application.Features.Employees.Commands.SyncEmployeeDocuments;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployee;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeeDocument;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeeJobInfo;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeePersonalInfo;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeePayroll;
using HrSystem.Application.Features.Employees.Commands.UploadEmployeeDocument;
using HrSystem.Application.Features.Employees.Commands.UpdateMyProfileImage;
using HrSystem.Application.Features.Employees.Commands.UpdateProfileImage;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeeRoles;
using HrSystem.Application.Features.Employees.Commands.UpdateEmployeeStatusAndProfile;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeDocuments;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeSalaries;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeDetails;
using HrSystem.Application.Features.Employees.Queries.GetMyDocuments;
using HrSystem.Application.Features.Employees.Queries.GetMyProfile;
using HrSystem.Application.Features.Employees.Queries.GetEmployeesList;
using HrSystem.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HrSystem.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class EmployeesController : APIBaseController
{
    private readonly ISender _mediator;
    public EmployeesController(ISender mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// Get a paginated list of employees with filters and sorting
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? statusId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? branchId = null,
        [FromQuery] Guid? jobTitleId = null,
        [FromQuery] Guid? managerId = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var query = new GetEmployeesListQuery(
            pageNumber,
            pageSize,
            searchTerm,
            statusId,
            departmentId,
            branchId,
            jobTitleId,
            managerId,
            sortBy,
            sortDescending);

        var result = await _mediator.Send(query);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var query = new GetEmployeeByIdQuery(id);
        var result = await _mediator.Send(query);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update payroll configuration for a specific employee (salary, allowances, deductions, payment method)
    /// </summary>
    [HttpPut("{employeeId:guid}/payroll")]
    public async Task<IActionResult> UpdateEmployeePayroll(Guid employeeId, [FromBody] UpdateEmployeePayrollCommand command)
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
    /// Create a new employee with full details
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateEmployee([FromForm] CreateEmployeeFullCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get full employee details (personal info, job info, latest payroll summary, recent attendance, leave balances, documents, assets)
    /// </summary>
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetEmployeeDetails(Guid id, [FromQuery] int attendanceRecentCount = 10, [FromQuery] int? leaveBalanceYear = null)
    {
        var result = await _mediator.Send(new GetEmployeeDetailsQuery(id, attendanceRecentCount, leaveBalanceYear));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get personal information for the currently logged-in user
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var result = await _mediator.Send(new GetMyProfileQuery());

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get documents for the currently logged-in user grouped by category
    /// </summary>
    [HttpGet("me/documents")]
    public async Task<IActionResult> GetMyDocuments()
    {
        var result = await _mediator.Send(new GetMyDocumentsQuery());

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update the current user's profile image (stored on S3)
    /// </summary>
    [HttpPost("me/profile-image")]
    public async Task<IActionResult> UpdateMyProfileImage([FromForm] UpdateMyProfileImageCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get documents for a specific employee (HR or owner)
    /// </summary>
    [HttpGet("{id:guid}/documents")]
    public async Task<IActionResult> GetEmployeeDocuments(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeDocumentsQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get a pre-signed download URL for an employee document
    /// </summary>
    [HttpGet("documents/{documentId:guid}/download-url")]
    public async Task<IActionResult> GetDocumentDownloadUrl(Guid documentId)
    {
        var result = await _mediator.Send(new GetEmployeeDocumentDownloadUrlQuery(documentId));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Download an employee document file directly
    /// </summary>
    [HttpGet("documents/{documentId:guid}/download")]
    public async Task<IActionResult> DownloadDocument(Guid documentId)
    {
        var result = await _mediator.Send(new DownloadEmployeeDocumentQuery(documentId));

        return result.Match(
            file => File(file.Contents, file.ContentType, file.FileName),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Upload a document for a specific employee (HR or owner)
    /// </summary>
    [HttpPost("{id:guid}/documents")]
    public async Task<IActionResult> UploadEmployeeDocument(Guid id, [FromForm] UploadEmployeeDocumentCommand command)
    {
        if (id != command.EmployeeId)
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
    /// Update profile image for a specific employee (HR/admin)
    /// </summary>
    [HttpPost("{id:guid}/profile-image")]
    public async Task<IActionResult> UpdateEmployeeProfileImage(Guid id, [FromForm] UpdateProfileImageCommand command)
    {
        if (id != command.EmployeeId)
        {
            // Ensure route id is authoritative
            command = command with { EmployeeId = id };
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update roles for a specific employee (HR/admin)
    /// </summary>
    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> UpdateEmployeeRoles(Guid id, [FromBody] UpdateEmployeeRolesCommand command)
    {
        if (id != command.EmployeeId)
        {
            command = command with { EmployeeId = id };
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing employee document with optional file replacement (HR or owner)
    /// </summary>
    [HttpPut("documents/{documentId:guid}")]
    public async Task<IActionResult> UpdateEmployeeDocument(Guid documentId, [FromForm] UpdateEmployeeDocumentCommand command)
    {
        if (documentId != command.DocumentId)
        {
            return BadRequest("Document ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Sync employee documents: create new, update existing, delete removed
    /// - Documents with DocumentId: update existing
    /// - Documents without DocumentId: create new
    /// - Documents in DB but not in request: delete
    /// </summary>
    [HttpPut("{id:guid}/documents/sync")]
    public async Task<IActionResult> SyncEmployeeDocuments(Guid id, [FromForm] SyncEmployeeDocumentsCommand command)
    {
        if (id != command.EmployeeId)
        {
            return BadRequest("Employee ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get salaries for a specific employee (HR or owner)
    /// </summary>
    [HttpGet("{id:guid}/salaries")]
    public async Task<IActionResult> GetEmployeeSalaries(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeSalariesQuery(id));

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Add salary for a specific employee (HR or owner)
    /// </summary>
    [HttpPost("{id:guid}/salaries")]
    public async Task<IActionResult> AddEmployeeSalary(Guid id, [FromBody] AddEmployeeSalaryCommand command)
    {
        if (id != command.EmployeeId)
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
    /// Update personal information for an existing employee
    /// </summary>
    [HttpPut("{employeeId:guid}/personal-info")]
    public async Task<IActionResult> UpdateEmployeePersonalInfo(Guid employeeId, [FromBody] UpdateEmployeePersonalInfoCommand command)
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
    /// Update job information for an existing employee
    /// </summary>
    [HttpPut("{employeeId:guid}/job-info")]
    public async Task<IActionResult> UpdateEmployeeJobInfo(Guid employeeId, [FromBody] UpdateEmployeeJobInfoCommand command)
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
    /// Update employee status and/or profile picture
    /// </summary>
    [HttpPost("{employeeId:guid}/status-profile")]
    public async Task<IActionResult> UpdateEmployeeStatusAndProfile(Guid employeeId, [FromForm] UpdateEmployeeStatusAndProfileCommand command)
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
    /// Assign direct manager to an existing employee (command as parameter)
    /// </summary>
    [HttpPost("direct-manager")]
    public async Task<IActionResult> AssignDirectManager([FromBody] AssignDirectManagerCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeCommand command)
    {
        if (id != command.Id)
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
    /// Delete an employee (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEmployee(Guid id)
    {
        var command = new DeleteEmployeeCommand(id);
        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Sync employee assets (create new, update existing, remove by ID)
    /// - Assets with AssetId: update existing
    /// - Assets without AssetId: create new
    /// - AssetsToRemove: list of asset IDs to delete
    /// </summary>
    [HttpPut("{id:guid}/assets/sync")]
    public async Task<IActionResult> SyncEmployeeAssets(Guid id, [FromBody] SyncEmployeeAssetsCommand command)
    {
        if (id != command.EmployeeId)
        {
            return BadRequest("Employee ID mismatch");
        }

        var result = await _mediator.Send(command);

        return result.Match(
            response => Ok(response),
            errors => Problem(errors)
        );
    }
}
