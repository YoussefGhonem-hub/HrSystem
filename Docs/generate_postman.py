import json
import os
import textwrap
from collections import OrderedDict


def raw_json_body(payload: str) -> dict:
    return {
        "mode": "raw",
        "raw": textwrap.dedent(payload).strip() + "\n",
        "options": {
            "raw": {
                "language": "json"
            }
        }
    }


def form_data(items: list) -> dict:
    return {
        "mode": "formdata",
        "formdata": items
    }


def qp(key: str, value: str, description: str | None = None, disabled: bool = False) -> dict:
    entry = {
        "key": key,
        "value": value
    }
    if description:
        entry["description"] = description
    if disabled:
        entry["disabled"] = True
    return entry


def build_url(path: str, query: list | None = None) -> dict:
    clean = path.lstrip("/")
    segments = [segment for segment in clean.split("/") if segment]
    raw_value = f"{{{{baseUrl}}}}/{clean}" if clean else "{{baseUrl}}"
    url = {
        "raw": raw_value,
        "host": ["{{baseUrl}}"],
        "path": segments
    }
    if query:
        url["query"] = query
        raw_query = "&".join(f"{q['key']}={q['value']}" for q in query)
        url["raw"] = f"{raw_value}?{raw_query}"
    return url


def json_headers() -> list:
    return [{"key": "Content-Type", "value": "application/json"}]


folders: "OrderedDict[str, list]" = OrderedDict()


def add_request(
    folder: str,
    name: str,
    method: str,
    path: str,
    description: str,
    query: list | None = None,
    body: dict | None = None,
    headers: list | None = None,
    auth: dict | None = None,
    tests: list | None = None
) -> None:
    if folder not in folders:
        folders[folder] = []

    request_obj = {
        "name": name,
        "request": {
            "method": method,
            "header": headers or [],
            "url": build_url(path, query),
            "description": description
        },
        "response": []
    }

    if body:
        request_obj["request"]["body"] = body

    if auth:
        request_obj["request"]["auth"] = auth

    if tests:
        request_obj["event"] = [{
            "listen": "test",
            "script": {
                "type": "text/javascript",
                "exec": tests
            }
        }]

    folders[folder].append(request_obj)


collection = {
    "info": {
        "_postman_id": "99b48d2f-8d5c-4b7a-9a77-5f41f0ea0001",
        "name": "HrSystem API",
        "description": "Comprehensive Postman collection for the HrSystem API covering authentication, employees, attendance, leave management, payroll, performance, users, and lookups.",
        "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
    },
    "auth": {
        "type": "bearer",
        "bearer": [
            {
                "key": "token",
                "value": "{{accessToken}}",
                "type": "string"
            }
        ]
    }
}

VARIABLES = [
    {"key": "baseUrl", "value": "https://localhost:7212"},
    {"key": "accessToken", "value": ""},
    {"key": "tokenExpiresAt", "value": ""},
    {"key": "userId", "value": ""},
    {"key": "employeeId", "value": "11111111-1111-1111-1111-111111111111"},
    {"key": "managerId", "value": "22222222-2222-2222-2222-222222222222"},
    {"key": "branchId", "value": "33333333-3333-3333-3333-333333333333"},
    {"key": "departmentId", "value": "44444444-4444-4444-4444-444444444444"},
    {"key": "jobTitleId", "value": "55555555-5555-5555-5555-555555555555"},
    {"key": "contractTypeId", "value": "66666666-6666-6666-6666-666666666666"},
    {"key": "employeeStatusId", "value": "70707070-7070-7070-7070-707070707070"},
    {"key": "genderId", "value": "77777777-7777-7777-7777-777777777777"},
    {"key": "maritalStatusId", "value": "88888888-8888-8888-8888-888888888888"},
    {"key": "countryId", "value": "99999999-9999-9999-9999-999999999999"},
    {"key": "leaveTypeId", "value": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"},
    {"key": "leaveStatusId", "value": "abababab-abab-abab-abab-abababababab"},
    {"key": "leaveRequestId", "value": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"},
    {"key": "leavePolicyId", "value": "cccccccc-cccc-cccc-cccc-cccccccccccc"},
    {"key": "documentTypeId", "value": "dddddddd-dddd-dddd-dddd-dddddddddddd"},
    {"key": "documentId", "value": "eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee"},
    {"key": "attendanceStatusId", "value": "ffffffff-ffff-ffff-ffff-ffffffffffff"},
    {"key": "workScheduleId", "value": "10101010-1010-1010-1010-101010101010"},
    {"key": "employeeAssetId", "value": "12121212-1212-1212-1212-121212121212"},
    {"key": "goalId", "value": "13131313-1313-1313-1313-131313131313"},
    {"key": "goalStatusId", "value": "14141414-1414-1414-1414-141414141414"},
    {"key": "goalPriorityId", "value": "15151515-1515-1515-1515-151515151515"},
    {"key": "kpiId", "value": "16161616-1616-1616-1616-161616161616"},
    {"key": "loanId", "value": "17171717-1717-1717-1717-171717171717"},
    {"key": "payslipId", "value": "18181818-1818-1818-1818-181818181818"},
    {"key": "organizationId", "value": "19191919-1919-1919-1919-191919191919"},
    {"key": "roleId", "value": "1a1a1a1a-1a1a-1a1a-1a1a-1a1a1a1a1a1a"},
    {"key": "subscriptionPlanId", "value": "1b1b1b1b-1b1b-1b1b-1b1b-1b1b1b1b1b1b"}
]

collection["variable"] = VARIABLES

# Authentication
add_request(
    folder="Authentication",
    name="Login (Seeded Org Admin)",
    method="POST",
    path="api/Auth/login",
    description="Authenticate using the seeded organization admin account (organizationadmin@demo001.local / Password@123)",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "username": "organizationadmin@demo001.local",
          "password": "Password@123"
        }
        """
    ),
    auth={"type": "noauth"},
    tests=[
        "pm.test('Responds with 200', function () {",
        "    pm.response.to.have.status(200);",
        "});",
        "",
        "const jsonData = pm.response.json();",
        "pm.test('Success flag is true', function () {",
        "    pm.expect(jsonData.success).to.be.true;",
        "});",
        "",
        "if (jsonData.data) {",
        "    pm.collectionVariables.set('accessToken', jsonData.data.accessToken);",
        "    pm.collectionVariables.set('tokenExpiresAt', jsonData.data.expiresAt);",
        "    pm.collectionVariables.set('userId', jsonData.data.userId);",
        "}",
        ""
    ]
)

# Employees
add_request(
    folder="Employees",
    name="List Employees",
    method="GET",
    path="api/Employees",
    description="Get a paginated, filterable list of employees",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("searchTerm", "john", "Optional search term", True),
        qp("departmentId", "{{departmentId}}", "Filter by department", True),
        qp("branchId", "{{branchId}}", "Filter by branch", True),
        qp("jobTitleId", "{{jobTitleId}}", "Filter by job title", True),
        qp("managerId", "{{managerId}}", "Filter by direct manager", True),
        qp("statusId", "{{employeeStatusId}}", "Filter by employee status", True),
        qp("sortBy", "fullNameEn", "Field to sort by", True),
        qp("sortDescending", "false", "Use true for descending sort", True)
    ]
)

add_request(
    folder="Employees",
    name="Get Employee By Id",
    method="GET",
    path="api/Employees/{{employeeId}}",
    description="Retrieve a single employee core profile"
)

add_request(
    folder="Employees",
    name="Get Employee Details",
    method="GET",
    path="api/Employees/{{employeeId}}/details",
    description="Aggregated employee profile including attendance, leave, payroll, documents, and assets",
    query=[
        qp("attendanceRecentCount", "10"),
        qp("leaveBalanceYear", "2026", "Optional leave balance year", True)
    ]
)

add_request(
    folder="Employees",
    name="Get My Profile",
    method="GET",
    path="api/Employees/me",
    description="Return personal information for the currently authenticated employee"
)

add_request(
    folder="Employees",
    name="Get My Documents",
    method="GET",
    path="api/Employees/me/documents",
    description="Fetch documents grouped by category for the logged-in employee"
)

add_request(
    folder="Employees",
    name="Get Employee Documents",
    method="GET",
    path="api/Employees/{{employeeId}}/documents",
    description="HR/Admin access to a specific employee document library"
)

add_request(
    folder="Employees",
    name="Download Employee Document",
    method="GET",
    path="api/Employees/documents/{{documentId}}/download",
    description="Generate a time-bound download URL for a stored employee document"
)

add_request(
    folder="Employees",
    name="Upload Employee Document",
    method="POST",
    path="api/Employees/{{employeeId}}/documents",
    description="Upload a document for the specified employee (HR or owner)",
    body=form_data([
        {"key": "employeeId", "value": "{{employeeId}}", "type": "text"},
        {"key": "documentTypeId", "value": "{{documentTypeId}}", "type": "text"},
        {"key": "documentName", "value": "Passport Copy", "type": "text"},
        {"key": "description", "value": "Scanned passport", "type": "text"},
        {"key": "expiryDate", "value": "2026-12-31", "type": "text"},
        {"key": "file", "type": "file", "src": ""}
    ])
)

add_request(
    folder="Employees",
    name="Get Employee Salaries",
    method="GET",
    path="api/Employees/{{employeeId}}/salaries",
    description="Return salary history for a specific employee"
)

add_request(
    folder="Employees",
    name="Add Employee Salary",
    method="POST",
    path="api/Employees/{{employeeId}}/salaries",
    description="Add a salary package for the target employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "basicSalary": 15000,
          "effectiveDate": "2026-02-01",
          "notes": "Annual increment"
        }
        """
    )
)

add_request(
    folder="Employees",
    name="Create Employee",
    method="POST",
    path="api/Employees",
    description="Create a new employee with core identity and job placement data",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeCode": "EMP-2001",
          "firstNameAr": "Ahmed",
          "lastNameAr": "Saleh",
          "firstNameEn": "Ahmed",
          "lastNameEn": "Saleh",
          "nationalId": "A1234567890",
          "passportNumber": "P1234567",
          "dateOfBirth": "1992-05-14",
          "genderId": "{{genderId}}",
          "maritalStatusId": "{{maritalStatusId}}",
          "email": "ahmed.saleh@example.com",
          "phoneNumber": "+20123456781",
          "mobileNumber": "+20123456782",
          "addressAr": "Address in Arabic",
          "addressEn": "123 Nile St, Cairo",
          "city": "Cairo",
          "country": "Egypt",
          "departmentId": "{{departmentId}}",
          "jobTitleId": "{{jobTitleId}}",
          "directManagerId": "{{managerId}}",
          "branchId": "{{branchId}}",
          "contractTypeId": "{{contractTypeId}}",
          "hiringDate": "2024-09-01",
          "probationPeriodMonths": 3
        }
        """
    )
)

add_request(
    folder="Employees",
    name="Assign Direct Manager",
    method="POST",
    path="api/Employees/direct-manager",
    description="Assign or change the direct manager for an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "directManagerId": "{{managerId}}"
        }
        """
    )
)

add_request(
    folder="Employees",
    name="Update Employee",
    method="PUT",
    path="api/Employees/{{employeeId}}",
    description="Update personal and job placement data for an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{employeeId}}",
          "firstNameAr": "Ahmed",
          "lastNameAr": "Saleh",
          "firstNameEn": "Ahmed",
          "lastNameEn": "Saleh",
          "passportNumber": "P1234567",
          "maritalStatusId": "{{maritalStatusId}}",
          "email": "ahmed.saleh@example.com",
          "phoneNumber": "+20123456781",
          "mobileNumber": "+20123456782",
          "addressAr": "Address in Arabic",
          "addressEn": "123 Nile St, Cairo",
          "city": "Cairo",
          "country": "Egypt",
          "departmentId": "{{departmentId}}",
          "jobTitleId": "{{jobTitleId}}",
          "directManagerId": "{{managerId}}",
          "branchId": "{{branchId}}",
          "contractTypeId": "{{contractTypeId}}",
          "statusId": "{{employeeStatusId}}"
        }
        """
    )
)

add_request(
    folder="Employees",
    name="Delete Employee",
    method="DELETE",
    path="api/Employees/{{employeeId}}",
    description="Soft-delete an employee record"
)

# Branches
add_request(
    folder="Branches",
    name="List Branches",
    method="GET",
    path="api/Branches",
    description="Get paginated branches with optional filters",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("searchTerm", "HQ", "Filter by name/code", True),
        qp("countryId", "{{countryId}}", "Filter by country", True),
        qp("isActive", "true", "Only active branches", True),
        qp("sortBy", "nameEn", "Field to sort", True),
        qp("sortDescending", "false", "Descending order", True)
    ]
)

add_request(
    folder="Branches",
    name="Get Branch By Id",
    method="GET",
    path="api/Branches/{{branchId}}",
    description="Retrieve branch details"
)

add_request(
    folder="Branches",
    name="Create Branch",
    method="POST",
    path="api/Branches",
    description="Create a new branch for the current organization",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "nameAr": "Al Far Alraeesy",
          "nameEn": "Main Branch",
          "code": "BR-HQ",
          "description": "Headquarters branch",
          "countryId": "{{countryId}}",
          "city": "Cairo",
          "addressAr": "Ealamat Almarkaz",
          "addressEn": "123 Nile Corniche",
          "postalCode": "11511",
          "phoneNumber": "+20123456780",
          "email": "hq@example.com",
          "timeZone": "Egypt Standard Time",
          "currency": "EGP",
          "language": "ar",
          "isHeadquarter": true,
          "openingDate": "2020-01-01"
        }
        """
    )
)

add_request(
    folder="Branches",
    name="Update Branch",
    method="PUT",
    path="api/Branches/{{branchId}}",
    description="Update branch profile",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{branchId}}",
          "nameAr": "Al Far Alraeesy",
          "nameEn": "Main Branch",
          "code": "BR-HQ",
          "description": "Headquarters branch",
          "countryId": "{{countryId}}",
          "city": "Cairo",
          "addressAr": "Ealamat Almarkaz",
          "addressEn": "123 Nile Corniche",
          "postalCode": "11511",
          "phoneNumber": "+20123456780",
          "email": "hq@example.com",
          "timeZone": "Egypt Standard Time",
          "currency": "EGP",
          "language": "ar",
          "isHeadquarter": true,
          "openingDate": "2020-01-01"
        }
        """
    )
)

add_request(
    folder="Branches",
    name="Delete Branch",
    method="DELETE",
    path="api/Branches/{{branchId}}",
    description="Soft-delete branch"
)

# Departments
add_request(
    folder="Departments",
    name="List Departments",
    method="GET",
    path="api/Departments",
    description="Get paginated departments with optional filters",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("searchTerm", "IT", "Search term", True),
        qp("branchId", "{{branchId}}", "Filter by branch", True),
        qp("sortBy", "nameEn", "Sort column", True),
        qp("sortDescending", "false", "Descending sort", True)
    ]
)

add_request(
    folder="Departments",
    name="Get Department By Id",
    method="GET",
    path="api/Departments/{{departmentId}}",
    description="Retrieve department details"
)

add_request(
    folder="Departments",
    name="Create Department",
    method="POST",
    path="api/Departments",
    description="Create a new department",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "nameAr": "Qesm Taqniat Almaelomat",
          "nameEn": "IT Department",
          "description": "Handles internal systems",
          "managerId": "{{managerId}}",
          "parentDepartmentId": null,
          "branchId": "{{branchId}}"
        }
        """
    )
)

add_request(
    folder="Departments",
    name="Update Department",
    method="PUT",
    path="api/Departments/{{departmentId}}",
    description="Update department data",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{departmentId}}",
          "nameAr": "Qesm Taqniat Almaelomat",
          "nameEn": "IT Department",
          "description": "Handles internal systems",
          "managerId": "{{managerId}}",
          "parentDepartmentId": null,
          "branchId": "{{branchId}}"
        }
        """
    )
)

add_request(
    folder="Departments",
    name="Delete Department",
    method="DELETE",
    path="api/Departments/{{departmentId}}",
    description="Soft-delete department"
)

add_request(
    folder="Departments",
    name="Assign Department Manager",
    method="POST",
    path="api/Departments/{{departmentId}}/manager",
    description="Assign a department manager",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "departmentId": "{{departmentId}}",
          "employeeId": "{{managerId}}"
        }
        """
    )
)

# Job Titles
add_request(
    folder="Job Titles",
    name="List Job Titles",
    method="GET",
    path="api/JobTitles",
    description="Get paginated job titles",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("searchTerm", "Engineer", "Search term", True),
        qp("sortBy", "titleEn", "Sort column", True),
        qp("sortDescending", "false", "Descending sort", True)
    ]
)

add_request(
    folder="Job Titles",
    name="Get Job Title By Id",
    method="GET",
    path="api/JobTitles/{{jobTitleId}}",
    description="Retrieve job title details"
)

add_request(
    folder="Job Titles",
    name="Create Job Title",
    method="POST",
    path="api/JobTitles",
    description="Create a job title",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "titleAr": "Muhandis Barmajyat",
          "titleEn": "Software Engineer",
          "description": "Builds and maintains software",
          "level": 3,
          "minSalary": 25000,
          "maxSalary": 45000
        }
        """
    )
)

add_request(
    folder="Job Titles",
    name="Update Job Title",
    method="PUT",
    path="api/JobTitles/{{jobTitleId}}",
    description="Update a job title",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{jobTitleId}}",
          "titleAr": "Muhandis Barmajyat",
          "titleEn": "Software Engineer",
          "description": "Builds and maintains software",
          "level": 4,
          "minSalary": 27000,
          "maxSalary": 50000
        }
        """
    )
)

add_request(
    folder="Job Titles",
    name="Delete Job Title",
    method="DELETE",
    path="api/JobTitles/{{jobTitleId}}",
    description="Soft-delete job title"
)

# Attendance
add_request(
    folder="Attendance",
    name="List Attendance",
    method="GET",
    path="api/Attendance",
    description="Paginated attendance records with filters",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True),
        qp("startDate", "2026-01-01", "Filter by start date", True),
        qp("endDate", "2026-01-31", "Filter by end date", True),
        qp("statusId", "{{attendanceStatusId}}", "Filter by status", True),
        qp("isLate", "false", "Only late records", True),
        qp("isOvertime", "false", "Only overtime records", True)
    ]
)

add_request(
    folder="Attendance",
    name="Attendance History",
    method="GET",
    path="api/Attendance/history",
    description="Attendance list alias for history view",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True)
    ]
)

add_request(
    folder="Attendance",
    name="Attendance Dashboard",
    method="GET",
    path="api/Attendance/dashboard",
    description="Daily attendance dashboard metrics",
    query=[qp("date", "2026-01-15", "Optional date", True)]
)

add_request(
    folder="Attendance",
    name="Get Attendance By Id",
    method="GET",
    path="api/Attendance/{{attendanceId}}",
    description="Retrieve a specific attendance record"
)

add_request(
    folder="Attendance",
    name="Create Attendance",
    method="POST",
    path="api/Attendance",
    description="Create an attendance entry manually",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "date": "2026-01-15",
          "checkInTime": "08:55:00",
          "checkOutTime": "17:05:00",
          "statusId": "{{attendanceStatusId}}",
          "deviceId": "WIFI-01",
          "checkInDeviceId": "WIFI-01",
          "checkOutDeviceId": "WIFI-01",
          "notes": "Manual entry"
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Update Attendance",
    method="PUT",
    path="api/Attendance/{{attendanceId}}",
    description="Update attendance entry",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{attendanceId}}",
          "checkInTime": "09:00:00",
          "checkOutTime": "17:00:00",
          "statusId": "{{attendanceStatusId}}",
          "deviceId": "WIFI-02",
          "checkInDeviceId": "WIFI-02",
          "checkOutDeviceId": "WIFI-02",
          "overtimeHours": "00:30:00",
          "lateMinutes": "00:10:00",
          "earlyLeaveMinutes": "00:00:00",
          "isLate": true,
          "isEarlyLeave": false,
          "isOvertime": true,
          "notes": "Adjusted after approval",
          "approvedBy": "HR Manager"
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Delete Attendance",
    method="DELETE",
    path="api/Attendance/{{attendanceId}}",
    description="Soft-delete attendance record"
)

add_request(
    folder="Attendance",
    name="Enroll Biometric Template",
    method="POST",
    path="api/Attendance/biometrics/enroll",
    description="Enroll biometric template for an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "biometricType": 2,
          "templateBase64": "QmFzZTY0RW5jb2RlZFRlbXBsYXRl==",
          "provider": "ZKTeco",
          "deviceId": "BIO-01",
          "isActive": true
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Verify Biometric Punch",
    method="POST",
    path="api/Attendance/biometrics/verify",
    description="Verify biometric data and register attendance punch",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "biometricType": 2,
          "punchType": 1,
          "templateBase64": "QmFzZTY0RW5jb2RlZFRlbXBsYXRl==",
          "deviceId": "BIO-01",
          "eventTime": "2026-01-15T08:55:00Z"
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Get Work Schedules",
    method="GET",
    path="api/Attendance/work-schedules",
    description="List work schedules defined for the organization"
)

add_request(
    folder="Attendance",
    name="Create Work Schedule",
    method="POST",
    path="api/Attendance/work-schedules",
    description="Create a reusable work schedule",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "name": "Standard 9-5",
          "startTime": "09:00:00",
          "endTime": "17:00:00",
          "breakDuration": "01:00:00",
          "workingHoursPerDay": 8,
          "workingDaysPerWeek": 5,
          "gracePeriodLate": "00:10:00",
          "gracePeriodEarlyLeave": "00:05:00",
          "isSaturday": false,
          "isSunday": true,
          "isMonday": true,
          "isTuesday": true,
          "isWednesday": true,
          "isThursday": true,
          "isFriday": false,
          "isDefault": true
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Update Work Schedule",
    method="PUT",
    path="api/Attendance/work-schedules/{{workScheduleId}}",
    description="Update working hours template",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{workScheduleId}}",
          "name": "Standard 9-5",
          "startTime": "09:00:00",
          "endTime": "17:30:00",
          "breakDuration": "01:00:00",
          "workingHoursPerDay": 8,
          "workingDaysPerWeek": 5,
          "gracePeriodLate": "00:10:00",
          "gracePeriodEarlyLeave": "00:05:00",
          "isSaturday": false,
          "isSunday": true,
          "isMonday": true,
          "isTuesday": true,
          "isWednesday": true,
          "isThursday": true,
          "isFriday": false,
          "isDefault": true
        }
        """
    )
)

add_request(
    folder="Attendance",
    name="Assign Employee Work Schedule",
    method="POST",
    path="api/Attendance/work-schedules/assign",
    description="Assign a work schedule to an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "workScheduleId": "{{workScheduleId}}",
          "effectiveDate": "2026-02-01"
        }
        """
    )
)

# Leave Management
add_request(
    folder="Leave",
    name="List Leave Requests",
    method="GET",
    path="api/Leave",
    description="Context-aware leave list (employee/manager/HR)",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("statusId", "{{leaveStatusId}}", "Filter by status", True),
        qp("leaveTypeId", "{{leaveTypeId}}", "Filter by type", True),
        qp("startDateFrom", "2026-02-01", "Filter start", True),
        qp("startDateTo", "2026-02-15", "Filter end", True),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True),
        qp("sortBy", "startDate", "Sort column", True),
        qp("sortDescending", "false", "Descending sort", True)
    ]
)

add_request(
    folder="Leave",
    name="Leave History",
    method="GET",
    path="api/Leave/history",
    description="Historical leave requests with same filters",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10")
    ]
)

add_request(
    folder="Leave",
    name="Create Leave Request",
    method="POST",
    path="api/Leave",
    description="Submit a new leave request",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "leaveTypeId": "{{leaveTypeId}}",
          "startDate": "2026-02-10",
          "endDate": "2026-02-14",
          "reason": "Annual vacation with family",
          "documentUrl": null,
          "emergencyContactName": "Omar Ali",
          "emergencyContactPhone": "+20123456789"
        }
        """
    )
)

add_request(
    folder="Leave",
    name="Get Leave Request By Id",
    method="GET",
    path="api/Leave/{{leaveRequestId}}",
    description="Retrieve leave details"
)

add_request(
    folder="Leave",
    name="My Leave Dashboard",
    method="GET",
    path="api/Leave/my-dashboard",
    description="Dashboard metrics for current user",
    query=[
        qp("year", "2026", "Optional year", True),
        qp("historyCount", "5", "Number of recent requests", True)
    ]
)

add_request(
    folder="Leave",
    name="My Leave Balances",
    method="GET",
    path="api/Leave/my-balances",
    description="View remaining balances",
    query=[qp("year", "2026", "Optional year", True)]
)

add_request(
    folder="Leave",
    name="HR Leave Summary",
    method="GET",
    path="api/Leave/hr/summary",
    description="Leave summary for HR roles",
    query=[
        qp("startDateFrom", "2026-02-01", "Optional start", True),
        qp("startDateTo", "2026-02-28", "Optional end", True)
    ]
)

add_request(
    folder="Leave",
    name="HR Leave Requests",
    method="GET",
    path="api/Leave/hr/requests",
    description="HR branch leave requests",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("statusId", "{{leaveStatusId}}", "Filter by status", True),
        qp("leaveTypeId", "{{leaveTypeId}}", "Filter by type", True)
    ]
)

add_request(
    folder="Leave",
    name="Manager Overview",
    method="GET",
    path="api/Leave/manager/overview",
    description="Combined view of manager requests and pending approvals",
    query=[
        qp("myPageNumber", "1"),
        qp("myPageSize", "5"),
        qp("pendingPageNumber", "1"),
        qp("pendingPageSize", "5")
    ]
)

add_request(
    folder="Leave",
    name="My Leave Requests",
    method="GET",
    path="api/Leave/my-requests",
    description="Explicit endpoint for employee requests",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("statusId", "{{leaveStatusId}}", "Filter by status", True)
    ]
)

add_request(
    folder="Leave",
    name="Approve Leave Request",
    method="POST",
    path="api/Leave/{{leaveRequestId}}/approve",
    description="Approve leave according to workflow",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "leaveRequestId": "{{leaveRequestId}}",
          "comments": "Approved - enjoy your vacation"
        }
        """
    )
)

add_request(
    folder="Leave",
    name="Reject Leave Request",
    method="POST",
    path="api/Leave/{{leaveRequestId}}/reject",
    description="Reject leave with reason",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "leaveRequestId": "{{leaveRequestId}}",
          "rejectionReason": "Peak period"
        }
        """
    )
)

add_request(
    folder="Leave",
    name="List Leave Policies",
    method="GET",
    path="api/Leave/policies",
    description="List configured leave policies"
)

add_request(
    folder="Leave",
    name="Get Leave Policy By Id",
    method="GET",
    path="api/Leave/policies/{{leavePolicyId}}",
    description="Retrieve leave policy"
)

add_request(
    folder="Leave",
    name="Create Leave Policy",
    method="POST",
    path="api/Leave/policies",
    description="Define leave balance rules",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "leaveTypeId": "{{leaveTypeId}}",
          "nameAr": "Siaset Agazat",
          "nameEn": "Annual Leave Policy",
          "defaultDaysPerYear": 21,
          "maxCarryForward": 5,
          "requiresApproval": true,
          "requiresManagerApproval": true,
          "requiresHRApproval": true,
          "isPaid": true,
          "maxConsecutiveDays": 14,
          "minDaysNotice": 3,
          "requiresDocument": false,
          "description": "Standard policy"
        }
        """
    )
)

add_request(
    folder="Leave",
    name="Update Leave Policy",
    method="PUT",
    path="api/Leave/policies/{{leavePolicyId}}",
    description="Update policy settings",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{leavePolicyId}}",
          "nameAr": "Siaset Agazat",
          "nameEn": "Annual Leave Policy",
          "defaultDaysPerYear": 24,
          "maxCarryForward": 7,
          "requiresApproval": true,
          "requiresManagerApproval": true,
          "requiresHRApproval": true,
          "isPaid": true,
          "maxConsecutiveDays": 18,
          "minDaysNotice": 2,
          "requiresDocument": false,
          "description": "Updated policy"
        }
        """
    )
)

add_request(
    folder="Leave",
    name="Delete Leave Policy",
    method="DELETE",
    path="api/Leave/policies/{{leavePolicyId}}",
    description="Soft-delete leave policy"
)

# Lifecycle - Employee Assets
add_request(
    folder="Lifecycle - Assets",
    name="List Employee Assets",
    method="GET",
    path="api/EmployeeAssets",
    description="Paginated employee asset assignments",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True),
        qp("assetType", "Laptop", "Filter by type", True)
    ]
)

add_request(
    folder="Lifecycle - Assets",
    name="My Assets",
    method="GET",
    path="api/EmployeeAssets/me",
    description="Assets assigned to the authenticated employee"
)

add_request(
    folder="Lifecycle - Assets",
    name="Get Employee Asset",
    method="GET",
    path="api/EmployeeAssets/{{employeeAssetId}}",
    description="Retrieve employee asset assignment"
)

add_request(
    folder="Lifecycle - Assets",
    name="Create Employee Asset",
    method="POST",
    path="api/EmployeeAssets",
    description="Assign an asset to an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "assetType": "Laptop",
          "assetNameAr": "Hasoob Mahmol",
          "assetNameEn": "Dell Latitude",
          "serialNumber": "DL-5520-0001",
          "assignedDate": "2026-01-20",
          "returnDate": null,
          "status": "Assigned",
          "value": 1500.00,
          "notes": "With docking station"
        }
        """
    )
)

add_request(
    folder="Lifecycle - Assets",
    name="Update Employee Asset",
    method="PUT",
    path="api/EmployeeAssets/{{employeeAssetId}}",
    description="Update asset assignment",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{employeeAssetId}}",
          "employeeId": "{{employeeId}}",
          "assetType": "Laptop",
          "assetNameAr": "Hasoob Mahmol",
          "assetNameEn": "Dell Latitude",
          "serialNumber": "DL-5520-0001",
          "assignedDate": "2026-01-20",
          "returnDate": "2026-06-30",
          "status": "Returned",
          "value": 1500.00,
          "notes": "Device returned in good condition"
        }
        """
    )
)

add_request(
    folder="Lifecycle - Assets",
    name="Delete Employee Asset",
    method="DELETE",
    path="api/EmployeeAssets/{{employeeAssetId}}",
    description="Soft-delete employee asset record"
)

# Performance - Goals
add_request(
    folder="Performance - Goals",
    name="List Goals",
    method="GET",
    path="api/Goals",
    description="Paginated performance goals",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True),
        qp("status", "{{goalStatusId}}", "Filter by status", True)
    ]
)

add_request(
    folder="Performance - Goals",
    name="Get Goal",
    method="GET",
    path="api/Goals/{{goalId}}",
    description="Retrieve goal details"
)

add_request(
    folder="Performance - Goals",
    name="Create Goal",
    method="POST",
    path="api/Goals",
    description="Create a goal for an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "titleAr": "Hadaf Alparmaat",
          "titleEn": "Increase Sales Pipeline",
          "descriptionAr": "Wasf Alhadaf",
          "descriptionEn": "Grow qualified pipeline by 15%",
          "startDate": "2026-01-01",
          "targetDate": "2026-06-30",
          "statusId": "{{goalStatusId}}",
          "priorityId": "{{goalPriorityId}}",
          "assignedBy": "{{managerId}}"
        }
        """
    )
)

add_request(
    folder="Performance - Goals",
    name="Update Goal",
    method="PUT",
    path="api/Goals/{{goalId}}",
    description="Update existing goal",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{goalId}}",
          "titleAr": "Hadaf Alparmaat",
          "titleEn": "Increase Sales Pipeline",
          "descriptionAr": "Wasf Alhadaf",
          "descriptionEn": "Grow qualified pipeline by 15%",
          "startDate": "2026-01-01",
          "targetDate": "2026-06-30",
          "statusId": "{{goalStatusId}}",
          "progress": 55,
          "priorityId": "{{goalPriorityId}}",
          "assignedBy": "{{managerId}}",
          "completionNotes": "Milestones achieved"
        }
        """
    )
)

add_request(
    folder="Performance - Goals",
    name="Delete Goal",
    method="DELETE",
    path="api/Goals/{{goalId}}",
    description="Soft-delete goal"
)

# Performance - KPIs
add_request(
    folder="Performance - KPIs",
    name="List KPIs",
    method="GET",
    path="api/KPIs",
    description="Paginated KPI definitions",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("searchTerm", "Utilization", "Search term", True)
    ]
)

add_request(
    folder="Performance - KPIs",
    name="Get KPI",
    method="GET",
    path="api/KPIs/{{kpiId}}",
    description="Retrieve KPI details"
)

add_request(
    folder="Performance - KPIs",
    name="Create KPI",
    method="POST",
    path="api/KPIs",
    description="Create KPI definition",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "nameAr": "Mueashir Ada",
          "nameEn": "Customer Satisfaction",
          "descriptionAr": "Wasf almueashir",
          "descriptionEn": "Quarterly CSAT target",
          "category": "Service",
          "weight": 25,
          "measurementCriteria": "Post-support survey >= 90%",
          "jobTitleId": "{{jobTitleId}}",
          "departmentId": "{{departmentId}}"
        }
        """
    )
)

add_request(
    folder="Performance - KPIs",
    name="Update KPI",
    method="PUT",
    path="api/KPIs/{{kpiId}}",
    description="Update KPI definition",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{kpiId}}",
          "nameAr": "Mueashir Ada",
          "nameEn": "Customer Satisfaction",
          "descriptionAr": "Wasf almueashir",
          "descriptionEn": "Quarterly CSAT target",
          "category": "Service",
          "weight": 30,
          "measurementCriteria": "Post-support survey >= 92%",
          "jobTitleId": "{{jobTitleId}}",
          "departmentId": "{{departmentId}}"
        }
        """
    )
)

add_request(
    folder="Performance - KPIs",
    name="Delete KPI",
    method="DELETE",
    path="api/KPIs/{{kpiId}}",
    description="Soft-delete KPI"
)

# Payroll
add_request(
    folder="Payroll",
    name="My Salary Summary",
    method="GET",
    path="api/Payroll/my-salary",
    description="Gross/net salary for logged-in employee",
    query=[
        qp("year", "2026", "Optional year", True),
        qp("month", "1", "Optional month", True)
    ]
)

add_request(
    folder="Payroll",
    name="Payslips History",
    method="GET",
    path="api/Payroll/history",
    description="HR-accessible payslip history",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee (HR)", True),
        qp("year", "2026", "Filter year", True),
        qp("month", "1", "Filter month", True)
    ]
)

add_request(
    folder="Payroll",
    name="Payroll Summary",
    method="GET",
    path="api/Payroll/summary",
    description="Aggregated payroll KPIs",
    query=[
        qp("year", "2026", "Optional year", True),
        qp("month", "1", "Optional month", True)
    ]
)

add_request(
    folder="Payroll",
    name="My Loans",
    method="GET",
    path="api/Payroll/my-loans",
    description="Loans associated with current user",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10")
    ]
)

add_request(
    folder="Payroll",
    name="My Loan Details",
    method="GET",
    path="api/Payroll/my-loans/{{loanId}}",
    description="Retrieve a single loan detail for current user"
)

add_request(
    folder="Payroll",
    name="My Payslips",
    method="GET",
    path="api/Payroll/my-payslips",
    description="Current user payslip list",
    query=[qp("year", "2026", "Filter year", True), qp("pageNumber", "1"), qp("pageSize", "6")]
)

add_request(
    folder="Payroll",
    name="My Payslip Details",
    method="GET",
    path="api/Payroll/my-payslips/{{payslipId}}",
    description="Full payslip details for the authenticated user"
)

# Loans
add_request(
    folder="Loans",
    name="List Loans",
    method="GET",
    path="api/Loans",
    description="Paginated loan assignments",
    query=[
        qp("pageNumber", "1"),
        qp("pageSize", "10"),
        qp("employeeId", "{{employeeId}}", "Filter by employee", True),
        qp("isActive", "true", "Filter active loans", True)
    ]
)

add_request(
    folder="Loans",
    name="Get Loan",
    method="GET",
    path="api/Loans/{{loanId}}",
    description="Retrieve loan details"
)

add_request(
    folder="Loans",
    name="Create Loan",
    method="POST",
    path="api/Loans",
    description="Create a new loan for an employee",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "employeeId": "{{employeeId}}",
          "loanName": "Education Loan",
          "totalAmount": 60000,
          "monthlyDeduction": 2500,
          "installmentMonths": 24,
          "startDate": "2026-01-01",
          "endDate": "2027-12-31",
          "isActive": true,
          "notes": "Payroll deduction"
        }
        """
    )
)

add_request(
    folder="Loans",
    name="Update Loan",
    method="PUT",
    path="api/Loans/{{loanId}}",
    description="Update existing loan",
    headers=json_headers(),
    body=raw_json_body(
        """
        {
          "id": "{{loanId}}",
          "employeeId": "{{employeeId}}",
          "loanName": "Education Loan",
          "totalAmount": 60000,
          "remainingAmount": 45000,
          "monthlyDeduction": 2500,
          "installmentMonths": 24,
          "startDate": "2026-01-01",
          "endDate": "2027-12-31",
          "isActive": true,
          "notes": "Payroll deduction"
        }
        """
    )
)

add_request(
    folder="Loans",
    name="Delete Loan",
    method="DELETE",
    path="api/Loans/{{loanId}}",
    description="Soft-delete loan"
)

# Lookups
lookup_endpoints = [
    ("Employees Lookup", "employees", "Non-paginated employee list"),
    ("Departments Lookup", "departments", "Departments for dropdowns"),
    ("Countries Lookup", "countries", "Supported countries"),
    ("Contract Types Lookup", "contract-types", "Contract type dropdown"),
    ("Employee Statuses Lookup", "employee-statuses", "Employee status options"),
    ("Attendance Statuses Lookup", "attendance-statuses", "Attendance statuses"),
    ("Leave Statuses Lookup", "leave-statuses", "Leave status options"),
    ("Leave Types Lookup", "leave-types", "Leave type dropdown"),
    ("Goal Statuses Lookup", "goal-statuses", "Goal status list"),
    ("Goal Priorities Lookup", "goal-priorities", "Goal priority list"),
    ("Review Types Lookup", "review-types", "Performance review types"),
    ("Review Statuses Lookup", "review-statuses", "Review status list"),
    ("Overtime Statuses Lookup", "overtime-statuses", "Overtime request statuses"),
    ("Payroll Statuses Lookup", "payroll-statuses", "Payroll status list"),
    ("Invoice Statuses Lookup", "invoice-statuses", "Invoice status list"),
    ("Genders Lookup", "genders", "Gender options"),
    ("Marital Statuses Lookup", "marital-statuses", "Marital status options")
]

for lookup_name, slug, desc in lookup_endpoints:
    add_request(
        folder="Lookups",
        name=lookup_name,
        method="GET",
        path=f"api/Lookups/{slug}",
        description=desc
    )

# Organizations (SuperAdmin)
add_request(
        folder="Organizations",
        name="Create Organization With Admin",
        method="POST",
        path="api/Organizations",
        description="Provision a new tenant, default branches, and organization admin user",
        headers=json_headers(),
        body=raw_json_body(
                """
                {
                    "organization": {
                        "nameAr": "Munazama Jadida",
                        "nameEn": "New Org",
                        "code": "ORG100",
                        "subscriptionPlanId": "{{subscriptionPlanId}}",
                        "logoUrl": null,
                        "commercialRegistrationNumber": "CR-9988",
                        "taxRegistrationNumber": "TX-7788",
                        "legalEntityType": "LLC",
                        "email": "admin@neworg.example",
                        "phoneNumber": "+20123450999",
                        "website": "https://neworg.example",
                        "addressAr": "Ealamat Riyadh",
                        "addressEn": "King Fahd Road",
                        "city": "Riyadh",
                        "country": "Saudi Arabia",
                        "postalCode": "11564",
                        "timeZone": "Arab Standard Time",
                        "currency": "SAR",
                        "weekStartDay": "Sunday",
                        "isTrialPeriod": true,
                        "trialDays": 30
                    },
                    "branches": [
                        {
                            "nameAr": "Fr Main",
                            "nameEn": "HQ",
                            "code": "ORG100-HQ",
                            "countryId": "{{countryId}}",
                            "description": "Headquarters",
                            "city": "Riyadh",
                            "addressAr": "Ealamat Riyadh",
                            "addressEn": "King Fahd Road",
                            "postalCode": "11564",
                            "latitude": 24.7136,
                            "longitude": 46.6753,
                            "phoneNumber": "+96611222333",
                            "email": "hq@neworg.example",
                            "fax": null,
                            "timeZone": "Arab Standard Time",
                            "currency": "SAR",
                            "language": "ar",
                            "isHeadquarter": true,
                            "openingDate": "2026-01-01"
                        }
                    ],
                    "adminUser": {
                        "email": "organizationadmin@neworg.example",
                        "fullName": "New Org Admin",
                        "password": "Password@123",
                        "userName": "neworg.admin"
                    }
                }
                """
        )
)

add_request(
        folder="Organizations",
        name="List Organizations",
        method="GET",
        path="api/Organizations",
        description="Paginated organizations (SuperAdmin only)",
        query=[qp("pageNumber", "1"), qp("pageSize", "10"), qp("searchTerm", "ORG", "Search term", True)]
)

add_request(
        folder="Organizations",
        name="Get Organization",
        method="GET",
        path="api/Organizations/{{organizationId}}",
        description="Retrieve details for a specific organization"
)

# Users
add_request(
        folder="Users",
        name="Create User With Branch Roles",
        method="POST",
        path="api/Users",
        description="Provision a user scoped to specific branches and roles",
        headers=json_headers(),
        body=raw_json_body(
                """
                {
                    "email": "branchmanager@demo.local",
                    "fullName": "Branch Manager",
                    "password": "Password@123",
                    "userName": "branch.manager",
                    "organizationId": "{{organizationId}}",
                    "branchRoles": [
                        {
                            "branchId": "{{branchId}}",
                            "roleIds": [
                                "{{roleId}}"
                            ]
                        }
                    ]
                }
                """
        )
)

add_request(
        folder="Users",
        name="List Users",
        method="GET",
        path="api/Users",
        description="Paginated list of system users",
        query=[
                qp("pageNumber", "1"),
                qp("pageSize", "20"),
                qp("searchTerm", "manager", "Search term", True),
                qp("isActive", "true", "Filter by active", True),
                qp("branchId", "{{branchId}}", "Filter by branch", True),
                qp("roleId", "{{roleId}}", "Filter by role", True)
        ]
)

add_request(
        folder="Users",
        name="Get User",
        method="GET",
        path="api/Users/{{userId}}",
        description="Retrieve user details"
)

add_request(
        folder="Users",
        name="Update User",
        method="PUT",
        path="api/Users/{{userId}}",
        description="Update user profile",
        headers=json_headers(),
        body=raw_json_body(
                """
                {
                    "id": "{{userId}}",
                    "fullName": "Branch Manager",
                    "userName": "branch.manager",
                    "isActive": true
                }
                """
        )
)

add_request(
        folder="Users",
        name="Deactivate User",
        method="POST",
        path="api/Users/{{userId}}/deactivate",
        description="Deactivate user account"
)

add_request(
        folder="Users",
        name="Activate User",
        method="POST",
        path="api/Users/{{userId}}/activate",
        description="Activate user account"
)

add_request(
        folder="Users",
        name="Change User Roles",
        method="PUT",
        path="api/Users/{{userId}}/roles",
        description="Replace branch-specific roles for a user",
        headers=json_headers(),
        body=raw_json_body(
                """
                {
                    "userId": "{{userId}}",
                    "branchId": "{{branchId}}",
                    "roleIds": [
                        "{{roleId}}"
                    ]
                }
                """
        )
)

output_path = os.path.join(os.path.dirname(__file__), "HrSystem-Login.postman_collection.json")

collection["item"] = [
    {
        "name": folder,
        "item": items
    }
    for folder, items in folders.items()
]

with open(output_path, "w", encoding="utf-8") as target:
    json.dump(collection, target, indent=2)
    target.write("\n")
