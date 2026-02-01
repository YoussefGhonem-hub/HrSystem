namespace HrSystem.Domain.Enums;

public enum EmployeeRequestType
{
    Vacation = 1,       // Full day leaves (annual, sick, etc.) - days-based
    OverTime = 2,       // Overtime work requests
    Training = 3,       // Training requests
    Miscellaneous = 4,  // Other requests
    Personal = 5,       // Personal requests
    Feedback = 6,       // Feedback submissions
    Permission = 7      // Permission requests (leave early, come late, short absence) - hours-based
}
