# Demo Users & Login Credentials

This document contains the login credentials for all demo users in the HR System.

## Role Users

| Role | Email | Password | Has Employee Record |
|------|-------|----------|---------------------|
| SuperAdmin | superadmin@demo001.local | Password@123 | No |
| OrganizationAdmin | organizationadmin@demo001.local | Password@123 | No |
| HRManager | hrmanager@demo001.local | Password@123 | Yes |
| HRSpecialist | hrspecialist@demo001.local | Password@123 | Yes |
| DepartmentManager | departmentmanager@demo001.local | Password@123 | Yes |
| Employee | employee@demo001.local | Password@123 | No |

## HR & Manager Role Users (With Employee Records)

These users have linked employee profiles and can be managed as employees:

| Role | Full Name | Department | Job Title | Employee Code |
|------|-----------|------------|-----------|---------------|
| HRManager | Sarah Johnson | Human Resources | HR Manager | EMP-0900 |
| HRSpecialist | Omar Ahmed | Human Resources | HR Specialist | EMP-0901 |
| DepartmentManager | Mohamed Ali | Operations | Operations Manager | EMP-0902 |

## Seeded Employees

The following employees are seeded from `Employees.json`:

| Employee Code | Name (EN) | Name (AR) | Department | Email |
|---------------|-----------|-----------|------------|-------|
| EMP-0001 | Ahmed Hassan | أحمد حسن | Engineering | ahmed.hassan@demo.com |
| EMP-0002 | Sara Mohamed | سارة محمد | Human Resources | sara.mohamed@demo.com |
| EMP-0003 | Mohamed Ali | محمد علي | Finance | mohamed.ali@demo.com |
| EMP-0004 | Fatima Ahmed | فاطمة أحمد | Marketing | fatima.ahmed@demo.com |
| EMP-0005 | Omar Khaled | عمر خالد | Operations | omar.khaled@demo.com |

## Role Permissions Summary

### SuperAdmin
- Full system access
- Can manage all organizations
- No branch scope restriction

### OrganizationAdmin
- Full access within their organization
- Can manage branches, departments, employees
- No branch scope restriction

### HRManager
- Manage all HR functions
- Approve/reject leave requests
- Manage payroll and attendance
- Branch-scoped

### HRSpecialist
- Handle day-to-day HR operations
- Process leave requests
- Manage employee records
- Branch-scoped

### DepartmentManager
- Manage employees in their department
- Approve department-level requests
- View department reports
- Branch-scoped

### Employee
- View own profile and records
- Submit leave requests
- View own payroll and attendance
- Branch-scoped

## Notes

- All passwords are: `Password@123`
- Organization code: `DEMO001`
- Email pattern: `{rolename}@{orgcode}.local`
- The database is seeded automatically on first run
