using ErrorOr;
using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Commands.DeleteEmployeeAsset;

public class DeleteEmployeeAssetCommandHandler : IRequestHandler<DeleteEmployeeAssetCommand, ErrorOr<GenericResponse>>
{
    private readonly ApplicationDbContext _context;

    public DeleteEmployeeAssetCommandHandler(ApplicationDbContext context) => _context = context;

    public async Task<ErrorOr<GenericResponse>> Handle(
        DeleteEmployeeAssetCommand request,
        CancellationToken cancellationToken)
    {
        var asset = await _context.EmployeeAssets
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (asset == null)
        {
            return Error.NotFound(description: "Employee asset not found");
        }

        _context.EmployeeAssets.Remove(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return new GenericResponse
        {
            Success = true,
            Message = "Employee asset deleted successfully"
        };
    }
}
