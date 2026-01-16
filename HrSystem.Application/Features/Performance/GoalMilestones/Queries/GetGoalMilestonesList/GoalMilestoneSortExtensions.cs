using HrSystem.Application.Common.PaginatedList;
using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;

public static class GoalMilestoneSortExtensions
{
    public static IQueryable<GoalMilestone> ApplySorting(
        this IQueryable<GoalMilestone> query,
        string? sortBy,
        bool sortDescending)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            sortBy = "duedate";

        query = sortBy.ToLower() switch
        {
            "duedate" => sortDescending
                ? query.OrderByDescending(gm => gm.DueDate)
                : query.OrderBy(gm => gm.DueDate),
            "title" => sortDescending
                ? query.OrderByDescending(gm => gm.TitleEn)
                : query.OrderBy(gm => gm.TitleEn),
            "titlear" => sortDescending
                ? query.OrderByDescending(gm => gm.TitleAr)
                : query.OrderBy(gm => gm.TitleAr),
            "completion" => sortDescending
                ? query.OrderByDescending(gm => gm.CompletionDate)
                : query.OrderBy(gm => gm.CompletionDate),
            "createdate" => sortDescending
                ? query.OrderByDescending(gm => gm.CreatedDate)
                : query.OrderBy(gm => gm.CreatedDate),
            _ => query.OrderBy(gm => gm.DueDate)
        };

        return query;
    }
}
