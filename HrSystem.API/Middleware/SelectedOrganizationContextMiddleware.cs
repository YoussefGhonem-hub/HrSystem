using HrSystem.Infrustructure.Persistence;
using HrSystem.Shared.Constants;
using HrSystem.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.API.Middleware;

public sealed class SelectedOrganizationContextMiddleware
{
    private readonly RequestDelegate _next;

    public SelectedOrganizationContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ApplicationDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var selectedOrganizationRaw = context.Request.Headers[CurrentUser.SelectedOrganizationHeaderName].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(selectedOrganizationRaw))
        {
            CurrentUser.SetSelectedOrganizationId(null);
            await _next(context);
            return;
        }

        var isSuperAdmin = context.User.IsInRole(RoleNames.SuperAdmin);
        if (!isSuperAdmin)
        {
            // Non-superadmin users are always scoped by their assigned organization from claims.
            CurrentUser.SetSelectedOrganizationId(null);
            await _next(context);
            return;
        }

        if (!Guid.TryParse(selectedOrganizationRaw, out var selectedOrganizationId))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Invalid organization selection format."
            });
            return;
        }

        var organizationExists = await dbContext.Organizations
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(o => o.Id == selectedOrganizationId && !o.IsDeleted);

        if (!organizationExists)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Selected organization was not found."
            });
            return;
        }

        CurrentUser.SetSelectedOrganizationId(selectedOrganizationId);
        await _next(context);
    }
}
