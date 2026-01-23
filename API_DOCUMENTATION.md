# HR System API Documentation

## Overview
This document provides a comprehensive guide to the HR System API. The API follows RESTful principles and uses JWT Bearer token authentication.

## Base URL
```
Development: https://localhost:7xxx/api
Production: https://your-domain.com/api
```

## Authentication

### Login
**Endpoint:** `POST /api/Auth/login`

**Request Body:**
```json
{
  "username": "string",
  "password": "string"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2026-01-24T12:00:00Z",
  "userId": "guid",
  "fullName": "John Doe",
  "email": "john.doe@example.com",
  "roles": ["Employee", "Manager"]
}
```

### Using Authentication
All protected endpoints require a Bearer token in the Authorization header:
```
Authorization: Bearer {your-token-here}
```

## API Endpoints

### 1. Employees Management

#### Get Employees List
**Endpoint:** `GET /api/Employees`

**Query Parameters:**
- `pageNumber` (int, default: 1)
- `pageSize` (int, default: 10)
- `searchTerm` (string, optional) - Search by name, code, or email
- `status` (EmployeeStatus, optional) - Active, Inactive, OnLeave, Terminated
- `departmentId` (guid, optional)
- `branchId` (guid, optional)
- `jobTitleId` (guid, optional)
- `managerId` (guid, optional)
- `sortBy` (string, optional)
- `sortDescending` (bool, default: false)

**Response:**
```json
{
  "success": true,
  "message": "Employees retrieved successfully",
  "data": {
    "items": [
      {
        "id": "guid",
        "employeeCode": "EMP001",
        "fullNameEn": "John Doe",
        "fullNameAr": "جون دو",
        "email": "john.doe@example.com",
        "phoneNumber": "+1234567890",
        "departmentNameEn": "IT",
        "jobTitleEn": "Software Engineer",
        "status": "Active",
        "hiringDate": "2024-01-01"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10
  }
}
```

#### Get Employee by ID
**Endpoint:** `GET /api/Employees/{id}`

#### Create Employee
**Endpoint:** `POST /api/Employees`

**Request Body:**
```json
{
  "employeeCode": "EMP001",
  "firstNameAr": "جون",
  "lastNameAr": "دو",
  "firstNameEn": "John",
  "lastNameEn": "Doe",
  "nationalId": "1234567890",
  "passportNumber": "ABC123456",
  "dateOfBirth": "1990-01-01",
  "gender": "Male",
  "maritalStatus": "Single",
  "email": "john.doe@example.com",
  "phoneNumber": "+1234567890",
  "mobileNumber": "+1234567890",
  "addressAr": "العنوان بالعربي",
  "addressEn": "Address in English",
  "city": "Dubai",
  "country": "UAE",
  "departmentId": "guid",
  "jobTitleId": "guid",
  "directManagerId": "guid",
  "branchId": "guid",
  "contractType": "FullTime",
  "hiringDate": "2024-01-01",
  "probationPeriodMonths": 6
}
```

#### Update Employee
**Endpoint:** `PUT /api/Employees/{id}`

#### Delete Employee
**Endpoint:** `DELETE /api/Employees/{id}`

---

### 2. Branches Management

#### Get Branches List
**Endpoint:** `GET /api/Branches`

**Query Parameters:**
- `pageNumber` (int)
- `pageSize` (int)
- `searchTerm` (string)
- `country` (Country enum)
- `isActive` (bool)
- `isHeadquarter` (bool)
- `sortBy` (string)
- `sortDescending` (bool)

#### Get Branch by ID
**Endpoint:** `GET /api/Branches/{id}`

#### Create Branch
**Endpoint:** `POST /api/Branches`

**Request Body:**
```json
{
  "nameAr": "الفرع الرئيسي",
  "nameEn": "Main Branch",
  "code": "BR001",
  "description": "Main headquarter branch",
  "country": "UAE",
  "city": "Dubai",
  "addressAr": "العنوان بالعربي",
  "addressEn": "Address in English",
  "postalCode": "12345",
  "latitude": 25.2048,
  "longitude": 55.2708,
  "phoneNumber": "+971-4-1234567",
  "email": "branch@example.com",
  "fax": "+971-4-1234568",
  "timeZone": "Asia/Dubai",
  "currency": "AED",
  "language": "en",
  "isHeadquarter": true,
  "openingDate": "2020-01-01",
  "branchManagerId": "guid"
}
```

#### Update Branch
**Endpoint:** `PUT /api/Branches/{id}`

#### Delete Branch
**Endpoint:** `DELETE /api/Branches/{id}`

---

### 3. Departments Management

#### Get Departments List
**Endpoint:** `GET /api/Departments`

**Query Parameters:**
- `pageNumber` (int)
- `pageSize` (int)
- `searchTerm` (string)
- `branchId` (guid)
- `managerId` (guid)
- `parentDepartmentId` (guid)
- `sortBy` (string)
- `sortDescending` (bool)

#### Get Department by ID
**Endpoint:** `GET /api/Departments/{id}`

#### Create Department
**Endpoint:** `POST /api/Departments`

**Request Body:**
```json
{
  "nameAr": "قسم تقنية المعلومات",
  "nameEn": "IT Department",
  "description": "Information Technology Department",
  "managerId": "guid",
  "parentDepartmentId": "guid",
  "branchId": "guid"
}
```

#### Update Department
**Endpoint:** `PUT /api/Departments/{id}`

#### Delete Department
**Endpoint:** `DELETE /api/Departments/{id}`

---

### 4. Job Titles Management

#### Get Job Titles List
**Endpoint:** `GET /api/JobTitles`

**Query Parameters:**
- `pageNumber` (int)
- `pageSize` (int)
- `searchTerm` (string)
- `level` (int)
- `minSalary` (decimal)
- `maxSalary` (decimal)
- `sortBy` (string)
- `sortDescending` (bool)

#### Get Job Title by ID
**Endpoint:** `GET /api/JobTitles/{id}`

#### Create Job Title
**Endpoint:** `POST /api/JobTitles`

**Request Body:**
```json
{
  "titleAr": "مهندس برمجيات",
  "titleEn": "Software Engineer",
  "description": "Develops and maintains software applications",
  "level": 3,
  "minSalary": 50000,
  "maxSalary": 80000
}
```

#### Update Job Title
**Endpoint:** `PUT /api/JobTitles/{id}`

#### Delete Job Title
**Endpoint:** `DELETE /api/JobTitles/{id}`

---

### 5. Attendance Management

#### Get Attendance Records
**Endpoint:** `GET /api/Attendance`

**Query Parameters:**
- `pageNumber` (int)
- `pageSize` (int)
- `employeeId` (guid)
- `startDate` (datetime)
- `endDate` (datetime)
- `status` (AttendanceStatus) - Present, Absent, Late, OnLeave
- `departmentId` (guid)
- `branchId` (guid)
- `isLate` (bool)
- `isOvertime` (bool)
- `sortBy` (string)
- `sortDescending` (bool)

#### Get Attendance by ID
**Endpoint:** `GET /api/Attendance/{id}`

#### Create Attendance Record
**Endpoint:** `POST /api/Attendance`

**Request Body:**
```json
{
  "employeeId": "guid",
  "date": "2024-01-23",
  "checkInTime": "08:00:00",
  "checkOutTime": "17:00:00",
  "status": "Present",
  "deviceId": "DEVICE001",
  "checkInDeviceId": "DEVICE001",
  "checkOutDeviceId": "DEVICE001",
  "notes": "Regular attendance"
}
```

#### Update Attendance Record
**Endpoint:** `PUT /api/Attendance/{id}`

**Request Body:**
```json
{
  "id": "guid",
  "checkInTime": "08:00:00",
  "checkOutTime": "17:00:00",
  "status": "Present",
  "deviceId": "DEVICE001",
  "checkInDeviceId": "DEVICE001",
  "checkOutDeviceId": "DEVICE001",
  "overtimeHours": "02:00:00",
  "lateMinutes": "00:00:00",
  "earlyLeaveMinutes": "00:00:00",
  "isLate": false,
  "isEarlyLeave": false,
  "isOvertime": true,
  "notes": "Overtime work",
  "approvedBy": "Manager Name"
}
```

#### Delete Attendance Record
**Endpoint:** `DELETE /api/Attendance/{id}`

---

### 6. Leave Management

#### Get Pending Leave Requests
**Endpoint:** `GET /api/Leave/pending`

**Description:** Returns leave requests pending approval by the current user (as manager or HR).

**Response:**
```json
{
  "success": true,
  "message": "Leave requests retrieved successfully",
  "data": [
    {
      "id": "guid",
      "employeeId": "guid",
      "employeeCode": "EMP001",
      "employeeName": "John Doe",
      "departmentName": "IT",
      "jobTitle": "Software Engineer",
      "leaveType": "Annual",
      "startDate": "2024-02-01",
      "endDate": "2024-02-05",
      "totalDays": 5,
      "reason": "Vacation",
      "status": "Pending",
      "requiresHRApproval": true,
      "currentApprovalLevel": "Manager"
    }
  ]
}
```

#### Approve Leave Request
**Endpoint:** `POST /api/Leave/{id}/approve`

**Request Body:**
```json
{
  "comments": "Approved for requested dates"
}
```

**Workflow:**
1. **Manager Approval:** Direct manager approves first (status changes to `ManagerApproved`)
2. **HR Approval:** If required, HR approves second (status changes to `Approved`)

#### Reject Leave Request
**Endpoint:** `POST /api/Leave/{id}/reject`

**Request Body:**
```json
{
  "rejectionReason": "Insufficient leave balance or conflicting dates"
}
```

---

### 7. Performance Management - Goals

#### Get Goals List
**Endpoint:** `GET /api/Goals`

**Query Parameters:**
- `employeeId` (guid)
- `status` (string)
- `priority` (string)
- `startDateFrom` (datetime)
- `startDateTo` (datetime)
- `targetDateFrom` (datetime)
- `targetDateTo` (datetime)
- `sortBy` (string)
- `isDescending` (bool)
- `pageNumber` (int)
- `pageSize` (int)

#### Get Goal by ID
**Endpoint:** `GET /api/Goals/{id}`

#### Create Goal
**Endpoint:** `POST /api/Goals`

**Request Body:**
```json
{
  "employeeId": "guid",
  "titleAr": "تحسين الأداء",
  "titleEn": "Improve Performance",
  "descriptionAr": "وصف الهدف بالعربي",
  "descriptionEn": "Goal description in English",
  "startDate": "2024-01-01",
  "targetDate": "2024-06-30",
  "statusId": "guid",
  "priorityId": "guid",
  "assignedBy": "guid"
}
```

#### Update Goal
**Endpoint:** `PUT /api/Goals/{id}`

**Request Body:**
```json
{
  "id": "guid",
  "titleAr": "تحسين الأداء",
  "titleEn": "Improve Performance",
  "descriptionAr": "وصف الهدف بالعربي",
  "descriptionEn": "Goal description in English",
  "startDate": "2024-01-01",
  "targetDate": "2024-06-30",
  "statusId": "guid",
  "progress": 75,
  "priorityId": "guid",
  "assignedBy": "guid",
  "completionNotes": "Good progress"
}
```

**Note:** When progress reaches 100%, `completionDate` is automatically set.

#### Delete Goal
**Endpoint:** `DELETE /api/Goals/{id}`

---

### 8. Performance Management - KPIs

#### Get KPIs List
**Endpoint:** `GET /api/KPIs`

**Query Parameters:**
- `searchTerm` (string)
- `category` (string)
- `jobTitleId` (guid)
- `departmentId` (guid)
- `sortBy` (string)
- `isDescending` (bool)
- `pageNumber` (int)
- `pageSize` (int)

#### Get KPI by ID
**Endpoint:** `GET /api/KPIs/{id}`

#### Create KPI
**Endpoint:** `POST /api/KPIs`

**Request Body:**
```json
{
  "nameAr": "مؤشر الأداء",
  "nameEn": "Performance Indicator",
  "descriptionAr": "وصف المؤشر بالعربي",
  "descriptionEn": "KPI description in English",
  "category": "Productivity",
  "weight": 20,
  "measurementCriteria": "Number of tasks completed per month",
  "jobTitleId": "guid",
  "departmentId": "guid"
}
```

#### Update KPI
**Endpoint:** `PUT /api/KPIs/{id}`

#### Delete KPI
**Endpoint:** `DELETE /api/KPIs/{id}`

---

### 9. Lifecycle - Onboarding Tasks

#### Get Onboarding Tasks
**Endpoint:** `GET /api/OnboardingTasks`

**Query Parameters:**
- `employeeId` (guid)
- `isCompleted` (bool)
- `category` (string)
- `dueDateFrom` (datetime)
- `dueDateTo` (datetime)
- `assignedTo` (guid)
- `sortBy` (string)
- `isDescending` (bool)
- `pageNumber` (int)
- `pageSize` (int)

#### Get Onboarding Task by ID
**Endpoint:** `GET /api/OnboardingTasks/{id}`

#### Create Onboarding Task
**Endpoint:** `POST /api/OnboardingTasks`

**Request Body:**
```json
{
  "employeeId": "guid",
  "taskNameAr": "مهمة التأهيل",
  "taskNameEn": "Onboarding Task",
  "descriptionAr": "وصف المهمة بالعربي",
  "descriptionEn": "Task description in English",
  "sequence": 1,
  "dueDate": "2024-02-01",
  "assignedTo": "guid",
  "category": "Documentation",
  "notes": "Complete within first week"
}
```

#### Update Onboarding Task
**Endpoint:** `PUT /api/OnboardingTasks/{id}`

#### Delete Onboarding Task
**Endpoint:** `DELETE /api/OnboardingTasks/{id}`

---

### 10. Lifecycle - Employee Assets

#### Get Employee Assets
**Endpoint:** `GET /api/EmployeeAssets`

**Query Parameters:**
- `employeeId` (guid)
- `assetType` (string)
- `status` (string)
- `assignedDateFrom` (datetime)
- `assignedDateTo` (datetime)
- `sortBy` (string)
- `isDescending` (bool)
- `pageNumber` (int)
- `pageSize` (int)

#### Get Employee Asset by ID
**Endpoint:** `GET /api/EmployeeAssets/{id}`

#### Create Employee Asset
**Endpoint:** `POST /api/EmployeeAssets`

**Request Body:**
```json
{
  "employeeId": "guid",
  "assetType": "Laptop",
  "assetNameAr": "جهاز محمول",
  "assetNameEn": "Laptop Computer",
  "serialNumber": "SN123456",
  "assignedDate": "2024-01-15",
  "returnDate": null,
  "status": "Assigned",
  "value": 1500.00,
  "notes": "Dell Latitude 5520"
}
```

#### Update Employee Asset
**Endpoint:** `PUT /api/EmployeeAssets/{id}`

#### Delete Employee Asset
**Endpoint:** `DELETE /api/EmployeeAssets/{id}`

---

## Common Response Format

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
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "FieldName": [
      "Error message"
    ]
  }
}
```

## Status Codes

- `200 OK` - Successful GET, PUT operations
- `201 Created` - Successful POST operations
- `400 Bad Request` - Validation errors
- `401 Unauthorized` - Missing or invalid authentication
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Conflict with existing data
- `500 Internal Server Error` - Server error

## Enums

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

### Gender
- `Male`
- `Female`

### MaritalStatus
- `Single`
- `Married`
- `Divorced`
- `Widowed`

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

## Pagination

All list endpoints support pagination with the following parameters:
- `pageNumber` - The page number to retrieve (default: 1)
- `pageSize` - Number of items per page (default: 10)

Pagination response includes:
```json
{
  "items": [...],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 100,
  "totalPages": 10,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

## Sorting

List endpoints support sorting with:
- `sortBy` - Field name to sort by
- `sortDescending` - Sort direction (default: false)

## Filtering

Each entity supports specific filters relevant to its data. Common filters include:
- Search terms (searches across multiple fields)
- Date ranges
- Status filters
- Reference IDs (department, branch, etc.)

## Best Practices

1. **Always include authentication token** for protected endpoints
2. **Use appropriate page sizes** to optimize performance
3. **Implement error handling** for all API calls
4. **Cache reference data** (departments, branches, job titles) when possible
5. **Use UTC dates** for all date/time values
6. **Follow the approval workflow** for leave requests (Manager -> HR)
7. **Validate data** on the client side before sending requests

## Rate Limiting

Currently, no rate limiting is implemented. This may be added in future versions.

## Versioning

API versioning is handled through URL path:
- Current version: `/api/v1/...` (implicit, no version in path)
- Future versions: `/api/v2/...`

## Support

For API support or questions, contact: support@hrsystem.com
