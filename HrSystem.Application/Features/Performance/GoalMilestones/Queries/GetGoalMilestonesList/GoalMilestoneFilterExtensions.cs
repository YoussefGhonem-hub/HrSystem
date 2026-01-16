using HrSystem.Domain.Entities.Performance;

namespace HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;

public static class GoalMilestoneFilterExtensions
{
    public static IQueryable<GoalMilestone> ApplyFilters(
        this IQueryable<GoalMilestone> query,
        Guid? goalId,
        bool? isCompleted,
        DateTime? dueDateFrom,
        DateTime? dueDateTo)
    {
        if (goalId.HasValue)
            query = query.Where(gm => gm.GoalId == goalId.Value);

        if (isCompleted.HasValue)
            query = query.Where(gm => gm.IsCompleted == isCompleted.Value);

        if (dueDateFrom.HasValue)
            query = query.Where(gm => gm.DueDate >= dueDateFrom.Value);

        if (dueDateTo.HasValue)
            query = query.Where(gm => gm.DueDate <= dueDateTo.Value);

        return query;
    }

    public static IQueryable<GoalMilestone> ApplyPaging(
        this IQueryable<GoalMilestone> query,
        int pageNumber,
        int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}
