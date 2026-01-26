namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceDashboard;

public class AttendanceDashboardDto
{
    public int TotalPresent { get; set; }
    public int LateArrivalToday { get; set; }
    public int AbsentToday { get; set; }
    public int OnLeaveToday { get; set; }
    public DateTime Date { get; set; }
}
