namespace HrSystem.Application.Features.Attendance.Queries.GetAttendanceDashboard;

public class AttendanceDashboardDto
{
    public DateTime Date { get; set; }

    // Total Employee Present
    public int TotalPresent { get; set; }
    /// <summary>Percentage change compared to yesterday (e.g. 5 means ↑5%)</summary>
    public double TotalPresentChangePercent { get; set; }

    // Late Arrival Today
    public int LateArrivalToday { get; set; }
    /// <summary>Absolute difference compared to same day last week</summary>
    public int LateArrivalChangeFromLastWeek { get; set; }

    // Absent Today
    public int AbsentToday { get; set; }
    /// <summary>Absolute difference compared to same day last week</summary>
    public int AbsentChangeFromLastWeek { get; set; }

    // On Leave
    public int OnLeaveToday { get; set; }
    /// <summary>Percentage change compared to yesterday (e.g. 3 means ↑3%)</summary>
    public double OnLeaveChangePercent { get; set; }
}
