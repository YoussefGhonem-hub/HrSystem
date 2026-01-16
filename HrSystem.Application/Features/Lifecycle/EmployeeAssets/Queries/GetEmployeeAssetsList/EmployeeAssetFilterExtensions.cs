namespace HrSystem.Application.Features.Lifecycle.EmployeeAssets.Queries.GetEmployeeAssetsList;

public static class EmployeeAssetFilterExtensions
{
    public static IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> ApplyFilters(
        this IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> query,
        Guid? employeeId,
        string? assetType,
        bool? isReturned,
        DateTime? assignedDateFrom,
        DateTime? assignedDateTo)
    {
        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }

        if (!string.IsNullOrEmpty(assetType))
        {
            query = query.Where(a => a.AssetType.ToLower().Contains(assetType.ToLower()));
        }

        if (isReturned.HasValue)
        {
            query = isReturned.Value
                ? query.Where(a => a.ReturnDate != null)
                : query.Where(a => a.ReturnDate == null);
        }

        if (assignedDateFrom.HasValue)
        {
            query = query.Where(a => a.AssignedDate >= assignedDateFrom.Value.Date);
        }

        if (assignedDateTo.HasValue)
        {
            query = query.Where(a => a.AssignedDate <= assignedDateTo.Value.Date);
        }

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> ApplySorting(
        this IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> query,
        string? sortBy,
        bool isDescending)
    {
        query = sortBy?.ToLower() switch
        {
            "assetname" => isDescending ? query.OrderByDescending(a => a.AssetName) : query.OrderBy(a => a.AssetName),
            "assettype" => isDescending ? query.OrderByDescending(a => a.AssetType) : query.OrderBy(a => a.AssetType),
            "assigneddate" => isDescending ? query.OrderByDescending(a => a.AssignedDate) : query.OrderBy(a => a.AssignedDate),
            "returndate" => isDescending ? query.OrderByDescending(a => a.ReturnDate) : query.OrderBy(a => a.ReturnDate),
            "employee" => isDescending ? query.OrderByDescending(a => a.Employee.FullNameEn) : query.OrderBy(a => a.Employee.FullNameEn),
            "value" => isDescending ? query.OrderByDescending(a => a.Value) : query.OrderBy(a => a.Value),
            _ => query.OrderByDescending(a => a.AssignedDate)
        };

        return query;
    }

    public static IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> ApplyPaging(
        this IQueryable<Domain.Entities.Lifecycle.EmployeeAsset> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
