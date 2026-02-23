using FluentValidation;

namespace HrSystem.Application.Features.Attendance.Commands.ImportAttendanceExcel;

public class ImportAttendanceExcelCommandValidator : AbstractValidator<ImportAttendanceExcelCommand>
{
    public ImportAttendanceExcelCommandValidator()
    {
        RuleFor(x => x.BranchId)
            .NotEmpty().WithMessage("Branch ID is required.");

        RuleFor(x => x.File)
            .NotNull().WithMessage("Excel file is required.");
    }
}
