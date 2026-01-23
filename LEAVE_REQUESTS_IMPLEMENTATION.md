# Leave Requests Implementation - Role-Based Filtering

## Overview
The leave requests query has been refactored from `GetPendingLeaveRequestsQuery` to `GetLeaveRequestsQuery` with role-based filtering that adapts to the current user's role.

## Changes Summary

### 1. Renamed Query
- **Old**: `GetPendingLeaveRequestsQuery` (no parameters, only pending requests)
- **New**: `GetLeaveRequestsQuery` (with comprehensive filtering parameters)

### 2. Created Extension Methods
**File**: `LeaveRequestFilterExtensions.cs`

Provides modular filtering methods:
- `ApplyRoleBasedFilters` - Main role-based filtering logic
- `ApplyStatusFilter` - Filter by leave status
- `ApplyLeaveTypeFilter` - Filter by leave type (Annual, Sick, etc.)
- `ApplyDateRangeFilter` - Filter by start date range
- `ApplyEmployeeFilter` - Filter by specific employee (for HR/Managers)
- `ApplySorting` - Flexible sorting by multiple fields

### 3. Role-Based Access Control

#### Employee Role
- **Access**: All their own leave requests regardless of status
- **Filter**: `lr.EmployeeId == currentEmployeeId`
- **Use Case**: Employees can view their entire leave history (Pending, Approved, Rejected)

#### Department Manager Role
- **Access**: Pending leave requests from direct reports only
- **Filter**: `lr.Status == LeaveStatus.Pending && lr.Employee.DirectManagerId == currentEmployeeId`
- **Use Case**: Managers see requests waiting for their approval

#### HR Manager Role
- **Access**: Manager-approved requests waiting for HR approval
- **Filter**: `lr.Status == LeaveStatus.ManagerApproved`
- **Use Case**: HR sees requests that passed manager approval and need HR sign-off

### 4. Query Parameters

The new query accepts the following parameters:

```csharp
public record GetLeaveRequestsQuery(
    LeaveStatus? Status = null,          // Filter by status (Pending, Approved, Rejected, etc.)
    LeaveType? LeaveType = null,         // Filter by type (Annual, Sick, Emergency, etc.)
    DateTime? StartDateFrom = null,      // Start date range - from
    DateTime? StartDateTo = null,        // Start date range - to
    Guid? EmployeeId = null,             // Filter by specific employee (HR/Manager only)
    string? SortBy = null,               // Sort field (StartDate, EndDate, Status, LeaveType, TotalDays, CreatedDate)
    bool SortDescending = false,         // Sort direction
    int PageNumber = 1,                  // Pagination - page number
    int PageSize = 10                    // Pagination - page size
)
```

### 5. API Endpoint Update

**File**: `LeaveController.cs`

#### Old Endpoint
```csharp
[HttpGet("pending")]
public async Task<IActionResult> GetPendingLeaveRequests()
```

#### New Endpoint
```csharp
[HttpGet]
public async Task<IActionResult> GetLeaveRequests(
    [FromQuery] LeaveStatus? status = null,
    [FromQuery] LeaveType? leaveType = null,
    [FromQuery] DateTime? startDateFrom = null,
    [FromQuery] DateTime? startDateTo = null,
    [FromQuery] Guid? employeeId = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] bool sortDescending = false,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
```

## API Usage Examples

### Employee Viewing Their Requests
```http
GET /api/leave?pageNumber=1&pageSize=10
Authorization: Bearer {employee_token}
```
Returns: All leave requests for the logged-in employee

### Employee Filtering Their Approved Requests
```http
GET /api/leave?status=2&sortBy=StartDate&sortDescending=true
Authorization: Bearer {employee_token}
```
Returns: All approved requests for the employee, sorted by start date (newest first)

### Manager Viewing Pending Approvals
```http
GET /api/leave?pageNumber=1&pageSize=20
Authorization: Bearer {manager_token}
```
Returns: All pending leave requests from direct reports

### HR Manager Viewing Requests Awaiting HR Approval
```http
GET /api/leave?pageNumber=1&pageSize=50
Authorization: Bearer {hr_token}
```
Returns: All manager-approved requests waiting for HR approval

### HR Filtering by Employee
```http
GET /api/leave?employeeId={employee-guid}&startDateFrom=2024-01-01&startDateTo=2024-12-31
Authorization: Bearer {hr_token}
```
Returns: All requests for a specific employee within date range

## Validation Rules

### Page Number
- Must be greater than 0

### Page Size
- Must be greater than 0
- Must not exceed 100

### Date Range
- `startDateFrom` must be less than or equal to `startDateTo`

## Leave Status Enum Values
```
0 = Pending           - Awaiting manager approval
1 = Approved          - Fully approved (by manager or HR)
2 = Rejected          - Rejected by manager or HR
3 = ManagerApproved   - Approved by manager, awaiting HR approval
4 = Cancelled         - Cancelled by employee
```

## Leave Type Enum Values
```
0 = Annual
1 = Sick
2 = Emergency
3 = Unpaid
4 = Maternity
5 = Paternity
6 = Study
7 = Bereavement
8 = Marriage
9 = Hajj
```

## Sort Options
- `startdate` - Sort by leave start date
- `enddate` - Sort by leave end date
- `status` - Sort by status
- `leavetype` - Sort by leave type
- `totaldays` - Sort by total days
- `createddate` - Sort by creation date

Default: Sorts by `StartDate` ascending

## Security Notes

1. **Authentication Required**: All endpoints require a valid JWT Bearer token
2. **Role-Based Filtering**: Data automatically filtered based on user's role
3. **Employee Isolation**: Employees cannot access other employees' data
4. **Manager Scope**: Managers only see direct reports' pending requests
5. **HR Access**: HR sees manager-approved requests only (not all requests)

## Response Format

```json
{
  "success": true,
  "message": "Leave requests retrieved successfully",
  "data": {
    "items": [
      {
        "id": "guid",
        "employeeId": "guid",
        "employeeCode": "EMP001",
        "employeeName": "John Doe",
        "employeeNameAr": "جون دو",
        "departmentName": "IT",
        "jobTitle": "Software Engineer",
        "branchName": "Main Branch",
        "leaveType": 0,
        "leaveTypeName": "Annual",
        "startDate": "2024-01-15T00:00:00Z",
        "endDate": "2024-01-19T00:00:00Z",
        "totalDays": 5.0,
        "reason": "Family vacation",
        "status": 0,
        "statusName": "Pending",
        "managerApprovalDate": null,
        "managerComments": null,
        "hrApprovalDate": null,
        "hrComments": null,
        "documentUrl": null,
        "createdDate": "2024-01-01T10:30:00Z",
        "requiresHRApproval": true,
        "currentApprovalLevel": "Manager"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

## Migration Notes

### Breaking Changes
- The `/api/leave/pending` endpoint has been replaced with `/api/leave`
- Frontend applications must update their API calls to use the new endpoint
- The new endpoint requires no route parameters but accepts query parameters

### Backward Compatibility
To maintain backward compatibility, you can add the old endpoint as an alias:

```csharp
[HttpGet("pending")]
public async Task<IActionResult> GetPendingLeaveRequests()
{
    // Redirect to main endpoint with no filters
    return await GetLeaveRequests();
}
```

## Files Modified/Created

### Created
1. `/HrSystem.Application/Features/Leave/Queries/GetLeaveRequests/GetLeaveRequestsQuery.cs`
2. `/HrSystem.Application/Features/Leave/Queries/GetLeaveRequests/GetLeaveRequestsQueryValidator.cs`
3. `/HrSystem.Application/Features/Leave/Queries/GetLeaveRequests/LeaveRequestFilterExtensions.cs`

### Modified
1. `/HrSystem.API/Controllers/LeaveController.cs`

### Deleted
1. `/HrSystem.Application/Features/Leave/Queries/GetPendingLeaveRequests/` (entire folder)
