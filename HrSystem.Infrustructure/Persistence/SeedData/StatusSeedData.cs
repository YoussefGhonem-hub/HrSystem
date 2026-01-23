using HrSystem.Domain.Entities.Attendance;
using HrSystem.Domain.Entities.Organization;
using HrSystem.Domain.Entities.Performance;
using Microsoft.EntityFrameworkCore;

namespace HrSystem.Infrustructure.Persistence.SeedData;

public static class StatusSeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed Review Types
        if (!await context.ReviewTypes.AnyAsync())
        {
            var reviewTypes = new List<ReviewType>
            {
                new ReviewType
                {
                    Id = Guid.NewGuid(),
                    Code = "ANNUAL",
                    NameAr = "التقييم السنوي",
                    NameEn = "Annual Review",
                    DescriptionAr = "تقييم الأداء السنوي الشامل",
                    DescriptionEn = "Comprehensive annual performance review",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewType
                {
                    Id = Guid.NewGuid(),
                    Code = "QUARTERLY",
                    NameAr = "التقييم الربع سنوي",
                    NameEn = "Quarterly Review",
                    DescriptionAr = "تقييم الأداء الربع سنوي",
                    DescriptionEn = "Quarterly performance review",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewType
                {
                    Id = Guid.NewGuid(),
                    Code = "PROBATION",
                    NameAr = "تقييم فترة التجربة",
                    NameEn = "Probation Review",
                    DescriptionAr = "تقييم الموظف خلال فترة التجربة",
                    DescriptionEn = "Employee probation period review",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewType
                {
                    Id = Guid.NewGuid(),
                    Code = "MIDYEAR",
                    NameAr = "تقييم منتصف العام",
                    NameEn = "Mid-Year Review",
                    DescriptionAr = "تقييم الأداء في منتصف العام",
                    DescriptionEn = "Mid-year performance review",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewType
                {
                    Id = Guid.NewGuid(),
                    Code = "PROJECT",
                    NameAr = "تقييم المشروع",
                    NameEn = "Project Review",
                    DescriptionAr = "تقييم الأداء بعد انتهاء المشروع",
                    DescriptionEn = "Post-project performance review",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.ReviewTypes.AddRangeAsync(reviewTypes);
            await context.SaveChangesAsync();
        }

        // Seed Review Statuses
        if (!await context.ReviewStatuses.AnyAsync())
        {
            var reviewStatuses = new List<ReviewStatus>
            {
                new ReviewStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "DRAFT",
                    NameAr = "مسودة",
                    NameEn = "Draft",
                    DescriptionAr = "التقييم قيد الإعداد",
                    DescriptionEn = "Review is being prepared",
                    ColorCode = "#9E9E9E",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "SUBMITTED",
                    NameAr = "مقدم",
                    NameEn = "Submitted",
                    DescriptionAr = "تم تقديم التقييم للمراجعة",
                    DescriptionEn = "Review has been submitted",
                    ColorCode = "#2196F3",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "APPROVED",
                    NameAr = "موافق عليه",
                    NameEn = "Approved",
                    DescriptionAr = "تمت الموافقة على التقييم",
                    DescriptionEn = "Review has been approved",
                    ColorCode = "#4CAF50",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "COMPLETED",
                    NameAr = "مكتمل",
                    NameEn = "Completed",
                    DescriptionAr = "التقييم مكتمل ونهائي",
                    DescriptionEn = "Review is completed and finalized",
                    ColorCode = "#8BC34A",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new ReviewStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "REJECTED",
                    NameAr = "مرفوض",
                    NameEn = "Rejected",
                    DescriptionAr = "تم رفض التقييم",
                    DescriptionEn = "Review has been rejected",
                    ColorCode = "#F44336",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.ReviewStatuses.AddRangeAsync(reviewStatuses);
            await context.SaveChangesAsync();
        }

        // Seed Goal Statuses
        if (!await context.GoalStatuses.AnyAsync())
        {
            var goalStatuses = new List<GoalStatus>
            {
                new GoalStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "NOT_STARTED",
                    NameAr = "لم يبدأ",
                    NameEn = "Not Started",
                    DescriptionAr = "لم يتم البدء في الهدف",
                    DescriptionEn = "Goal has not been started",
                    ColorCode = "#9E9E9E",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "IN_PROGRESS",
                    NameAr = "قيد التنفيذ",
                    NameEn = "In Progress",
                    DescriptionAr = "الهدف قيد التنفيذ",
                    DescriptionEn = "Goal is in progress",
                    ColorCode = "#2196F3",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "COMPLETED",
                    NameAr = "مكتمل",
                    NameEn = "Completed",
                    DescriptionAr = "تم إنجاز الهدف",
                    DescriptionEn = "Goal has been completed",
                    ColorCode = "#4CAF50",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "ON_HOLD",
                    NameAr = "معلق",
                    NameEn = "On Hold",
                    DescriptionAr = "الهدف معلق مؤقتاً",
                    DescriptionEn = "Goal is temporarily on hold",
                    ColorCode = "#FF9800",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "CANCELLED",
                    NameAr = "ملغى",
                    NameEn = "Cancelled",
                    DescriptionAr = "تم إلغاء الهدف",
                    DescriptionEn = "Goal has been cancelled",
                    ColorCode = "#F44336",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.GoalStatuses.AddRangeAsync(goalStatuses);
            await context.SaveChangesAsync();
        }

        // Seed Goal Priorities
        if (!await context.GoalPriorities.AnyAsync())
        {
            var goalPriorities = new List<GoalPriority>
            {
                new GoalPriority
                {
                    Id = Guid.NewGuid(),
                    Code = "LOW",
                    NameAr = "منخفضة",
                    NameEn = "Low",
                    ColorCode = "#4CAF50",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalPriority
                {
                    Id = Guid.NewGuid(),
                    Code = "MEDIUM",
                    NameAr = "متوسطة",
                    NameEn = "Medium",
                    ColorCode = "#FF9800",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalPriority
                {
                    Id = Guid.NewGuid(),
                    Code = "HIGH",
                    NameAr = "عالية",
                    NameEn = "High",
                    ColorCode = "#F44336",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new GoalPriority
                {
                    Id = Guid.NewGuid(),
                    Code = "CRITICAL",
                    NameAr = "حرجة",
                    NameEn = "Critical",
                    ColorCode = "#9C27B0",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.GoalPriorities.AddRangeAsync(goalPriorities);
            await context.SaveChangesAsync();
        }

        // Seed Overtime Statuses
        if (!await context.OvertimeStatuses.AnyAsync())
        {
            var overtimeStatuses = new List<OvertimeStatus>
            {
                new OvertimeStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "PENDING",
                    NameAr = "قيد الانتظار",
                    NameEn = "Pending",
                    ColorCode = "#FF9800",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OvertimeStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "APPROVED",
                    NameAr = "موافق عليه",
                    NameEn = "Approved",
                    ColorCode = "#4CAF50",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OvertimeStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "REJECTED",
                    NameAr = "مرفوض",
                    NameEn = "Rejected",
                    ColorCode = "#F44336",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new OvertimeStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "CANCELLED",
                    NameAr = "ملغى",
                    NameEn = "Cancelled",
                    ColorCode = "#9E9E9E",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.OvertimeStatuses.AddRangeAsync(overtimeStatuses);
            await context.SaveChangesAsync();
        }

        // Seed Invoice Statuses
        if (!await context.InvoiceStatuses.AnyAsync())
        {
            var invoiceStatuses = new List<InvoiceStatus>
            {
                new InvoiceStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "PENDING",
                    NameAr = "قيد الانتظار",
                    NameEn = "Pending",
                    ColorCode = "#FF9800",
                    IsActive = true,
                    DisplayOrder = 1,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new InvoiceStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "PAID",
                    NameAr = "مدفوعة",
                    NameEn = "Paid",
                    ColorCode = "#4CAF50",
                    IsActive = true,
                    DisplayOrder = 2,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new InvoiceStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "OVERDUE",
                    NameAr = "متأخرة",
                    NameEn = "Overdue",
                    ColorCode = "#F44336",
                    IsActive = true,
                    DisplayOrder = 3,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new InvoiceStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "PARTIALLY_PAID",
                    NameAr = "مدفوعة جزئياً",
                    NameEn = "Partially Paid",
                    ColorCode = "#2196F3",
                    IsActive = true,
                    DisplayOrder = 4,
                    CreatedDate = DateTimeOffset.UtcNow
                },
                new InvoiceStatus
                {
                    Id = Guid.NewGuid(),
                    Code = "CANCELLED",
                    NameAr = "ملغاة",
                    NameEn = "Cancelled",
                    ColorCode = "#9E9E9E",
                    IsActive = true,
                    DisplayOrder = 5,
                    CreatedDate = DateTimeOffset.UtcNow
                }
            };

            await context.InvoiceStatuses.AddRangeAsync(invoiceStatuses);
            await context.SaveChangesAsync();
        }
    }
}
