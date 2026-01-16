using HrSystem.Application.Features.Performance.Feedbacks.Common;
using HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbackById;
using HrSystem.Application.Features.Performance.Feedbacks.Queries.GetFeedbacksList;
using HrSystem.Application.Features.Performance.GoalMilestones.Common;
using HrSystem.Application.Features.Performance.GoalMilestones.Queries.GetGoalMilestonesList;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalById;
using HrSystem.Application.Features.Performance.Goals.Queries.GetGoalsList;
using HrSystem.Application.Features.Performance.KPIEvaluations.Common;
using HrSystem.Application.Features.Performance.KPIEvaluations.Queries.GetKPIEvaluationsList;
using HrSystem.Application.Features.Performance.KPIs.Common;
using HrSystem.Application.Features.Performance.KPIs.Queries.GetKPIsList;
using HrSystem.Application.Features.Performance.PerformanceReviews.Queries.GetPerformanceReviewsList;
using HrSystem.Domain.Entities.Performance;
using Mapster;

namespace HrSystem.Application.Features.Performance.Mappings;

public class PerformanceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Feedback mappings
        config.NewConfig<Feedback, FeedbackDto>()
            .Map(dest => dest.ProviderName, src => src.Provider != null ? src.Provider.FullNameEn : string.Empty)
            .Map(dest => dest.EmployeeName, src => src.PerformanceReview != null && src.PerformanceReview.Employee != null 
                ? src.PerformanceReview.Employee.FullNameEn : string.Empty);

        config.NewConfig<Feedback, FeedbackListDto>()
            .Map(dest => dest.ProviderName, src => src.Provider != null ? src.Provider.FullNameEn : string.Empty)
            .Map(dest => dest.EmployeeName, src => src.PerformanceReview != null && src.PerformanceReview.Employee != null 
                ? src.PerformanceReview.Employee.FullNameEn : string.Empty);

        // PerformanceReview mappings
        config.NewConfig<PerformanceReview, PerformanceReviewListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty)
            .Map(dest => dest.ReviewerName, src => src.Reviewer != null ? src.Reviewer.FullNameEn : string.Empty);

        // KPI mappings
        config.NewConfig<KPI, KPIDto>()
            .Map(dest => dest.JobTitleEn, src => src.JobTitle != null ? src.JobTitle.TitleEn : null)
            .Map(dest => dest.JobTitleAr, src => src.JobTitle != null ? src.JobTitle.TitleAr : null)
            .Map(dest => dest.DepartmentNameEn, src => src.Department != null ? src.Department.NameEn : null)
            .Map(dest => dest.DepartmentNameAr, src => src.Department != null ? src.Department.NameAr : null);

        config.NewConfig<KPI, KPIListDto>()
            .Map(dest => dest.JobTitleEn, src => src.JobTitle != null ? src.JobTitle.TitleEn : null)
            .Map(dest => dest.JobTitleAr, src => src.JobTitle != null ? src.JobTitle.TitleAr : null)
            .Map(dest => dest.DepartmentNameEn, src => src.Department != null ? src.Department.NameEn : null)
            .Map(dest => dest.DepartmentNameAr, src => src.Department != null ? src.Department.NameAr : null);

        // KPIEvaluation mappings
        config.NewConfig<KPIEvaluation, KPIEvaluationDto>()
            .Map(dest => dest.KPINameEn, src => src.KPI != null ? src.KPI.NameEn : string.Empty)
            .Map(dest => dest.KPINameAr, src => src.KPI != null ? src.KPI.NameAr : string.Empty)
            .Map(dest => dest.EmployeeName, src => src.PerformanceReview != null && src.PerformanceReview.Employee != null 
                ? src.PerformanceReview.Employee.FullNameEn : string.Empty);

        config.NewConfig<KPIEvaluation, KPIEvaluationListDto>()
            .Map(dest => dest.KPINameEn, src => src.KPI != null ? src.KPI.NameEn : string.Empty)
            .Map(dest => dest.KPINameAr, src => src.KPI != null ? src.KPI.NameAr : string.Empty)
            .Map(dest => dest.EmployeeName, src => src.PerformanceReview != null && src.PerformanceReview.Employee != null 
                ? src.PerformanceReview.Employee.FullNameEn : string.Empty);

        // GoalMilestone mappings
        config.NewConfig<GoalMilestone, GoalMilestoneDto>();
        config.NewConfig<GoalMilestone, GoalMilestoneListDto>();

        // Goal mappings
        config.NewConfig<Goal, GoalDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null);

        config.NewConfig<Goal, GoalListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty);
    }
}
