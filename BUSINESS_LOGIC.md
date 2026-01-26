# HR System — Business Logic Overview

This document summarizes the core business logic implemented across the HR System, organized by modules and key workflows. It references source files to help you quickly verify rules and behaviors.

## Roles & Access Control
- **SuperAdmin**: Creates organizations and system users; full access.
- **OrganizationAdmin**: Manages users within an organization; HR-level permissions.
- **HRManager / HRSpecialist**: Manages employees, salaries, attendance biometrics, approvals.
- **Department/Direct Manager**: Approves/rejects direct reports’ leave requests.
- **Employee**: Accesses personal profile, documents, balances, and payroll info.

References:
- [OrganizationsController.cs](HrSystem.API/Controllers/OrganizationsController.cs)
- [UsersController.cs](HrSystem.API/Controllers/UsersController.cs)
- Role usage in validators and handlers (e.g., [ApproveLeaveRequestCommandValidator.cs](HrSystem.Application/Features/Leave/Commands/ApproveLeaveRequest/ApproveLeaveRequestCommandValidator.cs))

## Multi‑Tenancy & Auditing
- All domain entities inherit audit fields and support soft delete.
- `TenantId` associates records to an organization; audit helpers track creators/modifiers.

References:
- [BaseEntity.cs](HrSystem.Domain/Common/BaseEntity.cs)
- [BaseAuditableEntity.cs](HrSystem.Domain/Common/BaseAuditableEntity.cs)

## Authentication & Identity
- JWT-based login returns role claims and employee context (employee ID, job title ID, direct manager ID).
- Claims are used for downstream authorization and scoping.

References:
- [AuthController.cs](HrSystem.API/Controllers/AuthController.cs)
- [LOGIN_IMPLEMENTATION.md](LOGIN_IMPLEMENTATION.md)

## Master Data (Organization Setup)
- **Organizations**: SuperAdmin can bootstrap organizations with branches and an admin user.
- **Branches / Departments / Job Titles**: Standard CRUD via respective controllers.
- **Lookups**: Centralized endpoints for statuses and enums (leave, attendance, payroll, performance).

References:
- [OrganizationsController.cs](HrSystem.API/Controllers/OrganizationsController.cs)
- [LookupsController.cs](HrSystem.API/Controllers/LookupsController.cs)
- Quick endpoint list: [API_QUICK_REFERENCE.md](API_QUICK_REFERENCE.md)

## Employees Module
- **Create/Update/Delete** employees.
- **Profile access**: Employees can view their own profile and documents.
- **Documents**: HR or the employee can upload documents; secure downloads via pre‑signed URLs.
- **Salaries**: Add salary records; keeps a single `IsCurrent` salary, auto‑ends previous records.
- **Probation**: `ProbationEndDate` computed from hiring date + period.

References:
- Controller: [EmployeesController.cs](HrSystem.API/Controllers/EmployeesController.cs)
- Create: [CreateEmployeeCommandHandler.cs](HrSystem.Application/Features/Employees/Commands/CreateEmployee/CreateEmployeeCommandHandler.cs)
- Salary: [AddEmployeeSalaryCommandHandler.cs](HrSystem.Application/Features/Employees/Commands/AddEmployeeSalary/AddEmployeeSalaryCommandHandler.cs)

Key Rules:
- Salary updates demote previous current salary and set `EndDate` relative to new `EffectiveDate`.
- HR roles or the employee themself can upload docs and manage salaries, subject to checks in handlers.

## Attendance Module
- **Manual Records**: CRUD for attendance items with filters and sorting.
- **Biometrics Enrollment**: Store hashed biometric templates per employee and type; replace/update allowed.
- **Biometric Verification (Check‑In/Out)**:
  - Verifies template hash match against the active biometric.
  - Creates day record if missing; prevents double check‑in/out.
  - Computes `WorkedHours` when both times exist.
  - Records device IDs and maintains `Present` status.

References:
- Controller: [AttendanceController.cs](HrSystem.API/Controllers/AttendanceController.cs)
- Enroll: [EnrollEmployeeBiometricCommandHandler.cs](HrSystem.Application/Features/Attendance/Commands/EnrollEmployeeBiometric/EnrollEmployeeBiometricCommandHandler.cs)
- Verify: [VerifyBiometricAttendanceCommandHandler.cs](HrSystem.Application/Features/Attendance/Commands/VerifyBiometricAttendance/VerifyBiometricAttendanceCommandHandler.cs)

Key Rules:
- HR roles may enroll/verify for any employee; non‑HRs only for themselves.
- Hashing: SHA‑256 computed on Base64 template; stored as `TemplateHash`.
- Duplicate prevention: rejects if already checked in/out for the date.

## Leave Management
- **Listing & Detail**: Role‑based filtering across employees, managers, and HR.
- **Approvals**: Two‑stage workflow — Manager then optional HR based on policy.
- **Rejection**: Allowed by manager or HR with validated reasons.
- **Balances & Dashboard**: Per‑employee views via dedicated queries.

References:
- Controller: [LeaveController.cs](HrSystem.API/Controllers/LeaveController.cs)
- Listing: [LEAVE_REQUESTS_IMPLEMENTATION.md](LEAVE_REQUESTS_IMPLEMENTATION.md)
- Approve: [ApproveLeaveRequestCommandHandler.cs](HrSystem.Application/Features/Leave/Commands/ApproveLeaveRequest/ApproveLeaveRequestCommand.cs)
- Approve validation: [ApproveLeaveRequestCommandValidator.cs](HrSystem.Application/Features/Leave/Commands/ApproveLeaveRequest/ApproveLeaveRequestCommandValidator.cs)
- Reject: [RejectLeaveRequestCommandHandler.cs](HrSystem.Application/Features/Leave/Commands/RejectLeaveRequest/RejectLeaveRequestCommand.cs)
- Reject validation: [RejectLeaveRequestCommandValidator.cs](HrSystem.Application/Features/Leave/Commands/RejectLeaveRequest/RejectLeaveRequestCommandValidator.cs)

Role‑Based Listing Highlights:
- **Employee**: Sees all their requests (any status).
- **Direct Manager**: Sees pending requests from direct reports.
- **HR**: Sees manager‑approved items awaiting HR approval.

Approval Workflow:
- Start in `Pending`.
- Manager action → `ManagerApproved`.
- If policy requires HR approval → HR action → `Approved`.
- If not required, manager approval completes as `Approved`.

## Payroll & Loans
- **My Salary Summary**: Gross/net salary from latest payslip; filter by year/month.
- **Payslips & Loans (Self)**: Employees can list and view personal payslips and loan details.
- **Loans (Admin/HR)**: Full CRUD for loans with filters, sorting, and paging.

References:
- Controller (self‑service): [PayrollController.cs](HrSystem.API/Controllers/PayrollController.cs)
- Loans CRUD: [LoansController.cs](HrSystem.API/Controllers/LoansController.cs)
- Salary summary: [GetMySalarySummaryQuery.cs](HrSystem.Application/Features/Payroll/Queries/GetMySalarySummary/GetMySalarySummaryQuery.cs)

Key Rules:
- Self‑service endpoints infer employee from JWT claims; unauthorized if no employee link.
- Loan updates require ID match validation at controller level.

## Users & Branch‑Scoped Roles
- Create users with roles scoped to specific branches for fine‑grained access.

References:
- [UsersController.cs](HrSystem.API/Controllers/UsersController.cs)

## Lookups & Enums
- Centralized endpoints provide active values for dropdowns and validations across modules (leave statuses/types, attendance statuses, payroll statuses, performance review types/statuses, goal priorities).

References:
- [LookupsController.cs](HrSystem.API/Controllers/LookupsController.cs)

## Common API Behavior
- **Pagination & Sorting**: All list endpoints accept `pageNumber`, `pageSize`, `sortBy`, `sortDescending`.
- **Standard Responses**: Success responses wrap data in `GenericResponse`; errors returned via ProblemDetails.
- **Authorization**: All endpoints secured except login.

References:
- Quick ref: [API_QUICK_REFERENCE.md](API_QUICK_REFERENCE.md)

## Auditing, Soft Deletes & Safety
- `MarkAsCreated/Modified/Deleted` used throughout handlers.
- Soft delete preferred; records maintain `DeletedDate` and `DeletedBy`.
- Validators enforce existence checks, permission checks, and business rules (e.g., cannot reject an approved leave).

## Notable Workflows
- **Login**: Validates credentials, builds token with employee context. See [LOGIN_IMPLEMENTATION.md](LOGIN_IMPLEMENTATION.md).
- **Leave Approval**: Multi‑level per policy; validators ensure actor is manager or HR at each stage.
- **Attendance via Biometrics**: Enrollment, verification, day record management, and hours computation.
- **Salary Change**: End previous current salary; set new effective window and optional notes.

## Observed Boundaries & Future Hooks
- Leave creation endpoint is not visible in current API controllers; listing and approval are present.
- Employee documents use storage services for downloads (pre‑signed URLs implied by queries), implementation resides under `Storage.AWS3`.

## How To Explore Further
- Review feature folders per module under [HrSystem.Application/Features](HrSystem.Application/Features) for detailed validators and DTOs.
- Use Swagger to inspect request/response contracts live.
