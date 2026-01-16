using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;

public static class KPIFilterExtensions
{
    public static IQueryable<KPI> ApplyFilters(
        this IQueryable<KPI> query,
        string? searchTerm,
        string? category,
        Guid? jobTitleId,
        Guid? departmentId,
        int? weightMin,
        int? weightMax)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = searchTerm.ToLower();
            query = query.Where(k =>
                k.NameEn.ToLower().Contains(search) ||
                k.NameAr.Contains(searchTerm) ||
                (k.DescriptionEn != null && k.DescriptionEn.ToLower().Contains(search)) ||
                (k.DescriptionAr != null && k.DescriptionAr.Contains(searchTerm)));
        }

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(k => k.Category == category);

        if (jobTitleId.HasValue)
            query = query.Where(k => k.JobTitleId == jobTitleId.Value);

        if (departmentId.HasValue)
            query = query.Where(k => k.DepartmentId == departmentId.Value);

        if (weightMin.HasValue)
            query = query.Where(k => k.Weight >= weightMin.Value);

        if (weightMax.HasValue)
            query = query.Where(k => k.Weight <= weightMax.Value);

        return query;
    }

    public static IQueryable<KPI> ApplyPaging(
        this IQueryable<KPI> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
