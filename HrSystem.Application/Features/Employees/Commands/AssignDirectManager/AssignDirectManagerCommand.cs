using ErrorOr;
using HrSystem.Application.Features.Employees.Queries.GetEmployeeById;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Employees.Commands.AssignDirectManager;

public record AssignDirectManagerCommand(Guid EmployeeId, Guid DirectManagerId)
    : IRequest<ErrorOr<GenericResponse<EmployeeDto>>>;

public class AssignDirectManagerCommandHandler : IRequestHandler<AssignDirectManagerCommand, ErrorOr<GenericResponse<EmployeeDto>>>
{
    private readonly ApplicationDbContext _context;

    public AssignDirectManagerCommandHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<GenericResponse<EmployeeDto>>> Handle(
        AssignDirectManagerCommand request,
        CancellationToken cancellationToken)
    {
        if (request.EmployeeId == request.DirectManagerId)
        {
            return Error.Validation("Employee.SelfManager", "An employee cannot be their own direct manager");
        }

        var employee = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.JobTitle)
            .Include(e => e.Branch)
            .Include(e => e.Gender)
            .Include(e => e.MaritalStatus)
            .Include(e => e.ContractType)
            .Include(e => e.Status)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
        {
            return Error.NotFound("Employee.NotFound", "Employee not found");
        }

        var manager = await _context.Employees
            .FirstOrDefaultAsync(e => e.Id == request.DirectManagerId, cancellationToken);

        if (manager is null)
        {
            return Error.NotFound("Manager.NotFound", "Direct manager not found");
        }

        // Optional validation: ensure same branch when both have one
        if (employee.BranchId.HasValue && manager.BranchId.HasValue && employee.BranchId != manager.BranchId)
        {
            return Error.Validation("Branch.Mismatch", "Direct manager must belong to the same branch");
        }

        employee.DirectManagerId = manager.Id;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new EmployeeDto
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
            DepartmentId = employee.DepartmentId,
            DepartmentNameEn = employee.Department?.NameEn ?? string.Empty,
            DepartmentNameAr = employee.Department?.NameAr ?? string.Empty,
            JobTitleId = employee.JobTitleId,
            JobTitleEn = employee.JobTitle?.TitleEn ?? string.Empty,
            JobTitleAr = employee.JobTitle?.TitleAr ?? string.Empty,
            DirectManagerId = employee.DirectManagerId,
            DirectManagerName = manager.FullNameEn,
            BranchId = employee.BranchId,
            BranchName = employee.Branch?.NameEn,
            ContractTypeId = employee.ContractTypeId,
            ContractTypeNameEn = employee.ContractType?.NameEn,
            ContractTypeNameAr = employee.ContractType?.NameAr,
            HiringDate = employee.HiringDate,
            ProbationPeriodMonths = employee.ProbationPeriodMonths,
            ProbationEndDate = employee.ProbationEndDate,
            StatusId = employee.StatusId,
            StatusNameEn = employee.Status?.NameEn,
            StatusNameAr = employee.Status?.NameAr,
            CreatedDate = employee.CreatedDate.DateTime
        };

        return new GenericResponse<EmployeeDto>
        {
            Success = true,
            Message = "Direct manager assigned successfully",
            Data = dto
        };
    }
}
