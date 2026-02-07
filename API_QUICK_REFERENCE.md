# HR System API - Quick Reference Guide

## 🚀 Getting Started

### 1. Run the Application
```bash
cd HrSystem.API
dotnet run
```

### 2. Access Swagger UI
```
https://localhost:7xxx/swagger
```

### 3. Login to Get Token
```http
POST /api/Auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}
```

### 4. Use Token in Requests
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 📋 API Endpoints Quick Reference

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Auth/login` | Login and get JWT token |

### Employees
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Employees` | Get paginated list |
| GET | `/api/Employees/{id}` | Get employee by ID |
| GET | `/api/Employees/{id}/details` | Get aggregated employee profile (personal, job, payroll, attendance, leave, documents, assets) |
| POST | `/api/Employees` | Create employee |
| PUT | `/api/Employees/{id}` | Update employee |
| DELETE | `/api/Employees/{id}` | Delete employee |

### Branches
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Branches` | Get paginated list |
| GET | `/api/Branches/{id}` | Get branch by ID |
| POST | `/api/Branches` | Create branch |
| PUT | `/api/Branches/{id}` | Update branch |
| DELETE | `/api/Branches/{id}` | Delete branch |

### Departments
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Departments` | Get paginated list |
| GET | `/api/Departments/{id}` | Get department by ID |
| POST | `/api/Departments` | Create department |
| PUT | `/api/Departments/{id}` | Update department |
| DELETE | `/api/Departments/{id}` | Delete department |

### Job Titles
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/JobTitles` | Get paginated list |
| GET | `/api/JobTitles/{id}` | Get job title by ID |
| POST | `/api/JobTitles` | Create job title |
| PUT | `/api/JobTitles/{id}` | Update job title |
| DELETE | `/api/JobTitles/{id}` | Delete job title |

### Attendance
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Attendance` | Get paginated list |
| GET | `/api/Attendance/{id}` | Get attendance by ID |
| GET | `/api/Attendance/dashboard` | Get attendance dashboard counts for a specific date |
| GET | `/api/Attendance/history` | Get attendance history (paginated with advanced filters) |
| POST | `/api/Attendance` | Create attendance record |
| PUT | `/api/Attendance/{id}` | Update attendance |
| DELETE | `/api/Attendance/{id}` | Delete attendance |

### Leave Management
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Leave/hr/summary` | HR summary metrics (roles: OrganizationAdmin, HRManager, HRSpecialist) |
| GET | `/api/Leave/hr/requests` | HR branch leave requests (roles: OrganizationAdmin, HRManager, HRSpecialist) |
| GET | `/api/Leave/manager/overview` | Department manager overview (roles: OrganizationAdmin, DepartmentManager) |
| GET | `/api/Leave/my-requests` | Current user's leave requests |
| GET | `/api/Leave/history` | Leave requests history (paginated with advanced filters) |
| GET | `/api/Leave/pending` | Get pending leave requests |
| POST | `/api/Leave/{id}/approve` | Approve leave request |
| POST | `/api/Leave/{id}/reject` | Reject leave request |

### Performance - Goals
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Goals` | Get paginated list |
| GET | `/api/Goals/{id}` | Get goal by ID |
| POST | `/api/Goals` | Create goal |
| PUT | `/api/Goals/{id}` | Update goal |
| DELETE | `/api/Goals/{id}` | Delete goal |

### Performance - KPIs
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/KPIs` | Get paginated list |
| GET | `/api/KPIs/{id}` | Get KPI by ID |
| POST | `/api/KPIs` | Create KPI |
| PUT | `/api/KPIs/{id}` | Update KPI |
| DELETE | `/api/KPIs/{id}` | Delete KPI |

### Lifecycle - Employee Assets
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/EmployeeAssets` | Get paginated list |
| GET | `/api/EmployeeAssets/{id}` | Get asset by ID |
| POST | `/api/EmployeeAssets` | Create asset assignment |
| PUT | `/api/EmployeeAssets/{id}` | Update asset |
| DELETE | `/api/EmployeeAssets/{id}` | Delete asset |

### Payroll
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Payroll/my-salary` | Get gross and net salary for current user |
| GET | `/api/Payroll/my-loans` | Get current user's loans (paginated) |
| GET | `/api/Payroll/my-loans/{loanId}` | Get loan details for current user |
| GET | `/api/Payroll/my-payslips` | Get current user's payslips (paginated, filter by year) |
| GET | `/api/Payroll/my-payslips/{payslipId}` | Get full payslip details for current user |
| GET | `/api/Payroll/summary` | Payroll dashboard summary (employees paid, gross, deductions, net) |
| GET | `/api/Payroll/history` | Payslips history (paginated with advanced filters; role-aware) |

### Lookups
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Lookups/employees` | Non-paginated employees list for dropdowns |
| GET | `/api/Lookups/departments` | Non-paginated departments list for dropdowns |
## 🔍 Common Query Parameters

All list endpoints support:
- `pageNumber` (default: 1)
- `pageSize` (default: 10)
- `sortBy` (field name)
- `sortDescending` (true/false)

## 📊 Common Enums

### EmployeeStatus
- `Active`
- `Inactive`
- `OnLeave`
- `Terminated`

### ContractType
- `FullTime`
- `PartTime`
- `Contract`
- `Temporary`

### AttendanceStatus
- `Present`
- `Absent`
- `Late`
- `OnLeave`
- `Holiday`

### LeaveStatus
- `Pending`
- `ManagerApproved`
- `Approved`
- `Rejected`
- `Cancelled`

### LeaveType
- `Annual`
- `Sick`
- `Emergency`
- `Maternity`
- `Paternity`
- `Unpaid`
- `Study`
- `Hajj`

### Gender
- `Male`
- `Female`

### MaritalStatus
- `Single`
- `Married`
- `Divorced`
- `Widowed`

### Country
- `UAE`
- `SaudiArabia`
- `Kuwait`
- `Qatar`
- `Bahrain`
- `Oman`
- `Egypt`
- `Jordan`
- `Lebanon`
- `Other`

## 🎯 Quick Examples

### Create Employee
```json
POST /api/Employees
{
  "employeeCode": "EMP001",
  "firstNameEn": "John",
  "lastNameEn": "Doe",
  "firstNameAr": "جون",
  "lastNameAr": "دو",
  "nationalId": "1234567890",
  "dateOfBirth": "1990-01-01",
  "gender": "Male",
  "maritalStatus": "Single",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "addressAr": "العنوان",
  "addressEn": "Address",
  "departmentId": "guid",
  "jobTitleId": "guid",
  "contractType": "FullTime",
  "hiringDate": "2024-01-01",
  "probationPeriodMonths": 6
}
```

### Create Attendance
```json
POST /api/Attendance
{
  "employeeId": "guid",
  "date": "2024-01-23",
  "checkInTime": "08:00:00",
  "checkOutTime": "17:00:00",
  "status": "Present",
  "notes": "On time"
}
```

### Approve Leave Request
```json
POST /api/Leave/{id}/approve
{
  "comments": "Approved for requested dates"
}
```

### Create Goal
```json
POST /api/Goals
{
  "employeeId": "guid",
  "titleEn": "Improve Sales",
  "titleAr": "تحسين المبيعات",
  "startDate": "2024-01-01",
  "targetDate": "2024-06-30",
  "statusId": "guid",
  "priorityId": "guid"
}
```

## 🛠️ Development Tools

### Recommended Extensions
- REST Client (VS Code)
- Thunder Client (VS Code)
- Postman

### Testing Workflow
1. Login to get token
2. Copy access token
3. Add to Authorization header: `Bearer {token}`
4. Make requests

## 📝 Response Format

### Success Response
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": { ... }
}
```

### Error Response
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation error",
  "status": 400,
  "errors": {
    "FieldName": ["Error message"]
  }
}
```

### Paginated Response
```json
{
  "success": true,
  "data": {
    "items": [...],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

## 🔒 Security

- All endpoints except `/api/Auth/login` require authentication
- JWT tokens expire after configured time
- Role-based authorization implemented
- HTTPS required in production

## 📚 Documentation

- **Swagger UI:** `/swagger`
- **Full API Documentation:** `API_DOCUMENTATION.md`
- **Implementation Summary:** `API_IMPLEMENTATION_SUMMARY.md`

## 🆘 Common Issues

### 401 Unauthorized
- Token missing or expired
- Login again to get new token

### 400 Bad Request
- Validation failed
- Check request body against requirements

### 404 Not Found
- Resource doesn't exist
- Verify GUID/ID is correct

### 409 Conflict
- Duplicate data (e.g., employee code)
- Use unique identifiers

## 📞 Support

For issues or questions:
- Check Swagger documentation
- Review API_DOCUMENTATION.md
- Verify request format matches examples
- Ensure authentication token is valid

---

**Last Updated:** January 26, 2026
**API Version:** 1.0
