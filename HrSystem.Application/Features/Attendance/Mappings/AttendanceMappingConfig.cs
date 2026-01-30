using HrSystem.Application.Features.Attendance.Queries.GetAttendanceById;
using HrSystem.Application.Features.Attendance.Queries.GetAttendancesList;
using Mapster;

namespace HrSystem.Application.Features.Attendance.Mappings;

public class AttendanceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Domain.Entities.Attendance.Attendance, AttendanceDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : null)
            .Map(dest => dest.EmployeeCode, src => src.Employee != null ? src.Employee.EmployeeCode : null)
            .Map(dest => dest.StatusNameEn, src => src.Status != null ? src.Status.NameEn : null)
            .Map(dest => dest.StatusNameAr, src => src.Status != null ? src.Status.NameAr : null);

        config.NewConfig<Domain.Entities.Attendance.Attendance, AttendanceListDto>()
            .Map(dest => dest.EmployeeName, src => src.Employee != null ? src.Employee.FullNameEn : string.Empty)
            .Map(dest => dest.EmployeeCode, src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty)
            .Map(dest => dest.StatusNameEn, src => src.Status != null ? src.Status.NameEn : null)
            .Map(dest => dest.StatusNameAr, src => src.Status != null ? src.Status.NameAr : null);
    }
}
