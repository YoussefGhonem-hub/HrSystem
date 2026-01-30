using HrSystem.Application.Features.Attendance.Mappings;
using HrSystem.Application.Features.Branches.Mappings;
using HrSystem.Application.Features.Departments.Mappings;
using HrSystem.Application.Features.Employees.Mappings;
using HrSystem.Application.Features.JobTitles.Mappings;
using HrSystem.Application.Features.Lifecycle.Mappings;
using HrSystem.Application.Features.Performance.Mappings;
using Mapster;

namespace HrSystem.Application.Common.Mappings;

public static class MappingConfig
{
    public static void Register(TypeAdapterConfig config)
    {
        // Register all mapping profiles so DTO adapters are available application-wide
        new AttendanceMappingConfig().Register(config);
        new BranchMappingConfig().Register(config);
        new DepartmentMappingConfig().Register(config);
        new EmployeeMappingConfig().Register(config);
        new JobTitleMappingConfig().Register(config);
        new LifecycleMappingConfig().Register(config);
        new PerformanceMappingConfig().Register(config);
    }
}
