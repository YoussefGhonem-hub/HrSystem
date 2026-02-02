# Employee Self-Service Requests API Documentation

## Overview

This API allows employees to submit self-service requests (Vacation, Overtime, Training, Miscellaneous, Personal, Feedback) and provides branch-level configuration for controlling which request types are available.

**Base URL:** `/api/EmployeeRequests`

**Authentication:** All endpoints require JWT Bearer token authentication.

---

## 1. Get Available Request Types (Master Data)

Returns the request types enabled for the employee's branch along with type-specific dropdown options.

### Endpoint
```
GET /api/EmployeeRequests/available?branchId={branchId}
```

### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `branchId` | GUID | Optional | Branch ID. Defaults to current user's branch. |

### Response
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "requestType": 1,
      "displayName": "Vacation",
      "isVisibleToEmployees": true,
      "allowEmployeesToSubmit": true,
      "requireAttachment": false,
      "maxOpenRequests": 2,
      "customInstructions": "Please submit at least 3 days in advance",
      "vacationTypes": [
        {
          "id": "guid",
          "nameEn": "Annual Leave",
          "nameAr": "إجازة سنوية",
          "description": "Paid annual leave",
          "isPaid": true,
          "maxDaysPerYear": 21,
          "requiresAttachment": false,
          "requiresManagerApproval": true,
          "sortOrder": 1
        }
      ],
      "overtimeTypes": null,
      "trainingTypes": null,
      "miscellaneousTypes": null,
      "personalTypes": null,
      "feedbackTypes": null
    },
    {
      "requestType": 2,
      "displayName": "OverTime",
      "isVisibleToEmployees": true,
      "allowEmployeesToSubmit": true,
      "requireAttachment": false,
      "maxOpenRequests": 5,
      "overtimeTypes": [
        {
          "id": "guid",
          "nameEn": "Regular Overtime",
          "nameAr": "وقت إضافي عادي",
          "defaultMultiplier": 1.5,
          "requiresManagerApproval": true,
          "sortOrder": 1
        }
      ]
    }
  ]
}
```

### Request Type Enum Values
| Value | Name | Description |
|-------|------|-------------|
| 1 | Vacation | Vacation/Leave requests |
| 2 | OverTime | Overtime work requests |
| 3 | Training | Training/Course requests |
| 4 | Miscellaneous | General miscellaneous requests |
| 5 | Personal | Personal matter requests |
| 6 | Feedback | Feedback/Suggestions |

### Frontend Usage
1. Call this endpoint on login or when entering the request module
2. Use the response to build the request type selector (only show `isVisibleToEmployees: true`)
3. When user selects a request type, populate the type-specific dropdown from the corresponding array (e.g., `vacationTypes` for Vacation)
4. Check `allowEmployeesToSubmit` to enable/disable the submit button
5. Check `requireAttachment` to make attachment field mandatory
6. Display `customInstructions` if present

---

## 2. Get My Requests

Returns the current employee's submitted requests.

### Endpoint
```
GET /api/EmployeeRequests/me?type={requestType}
```

### Parameters
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `type` | int | Optional | Filter by request type (1-6) |

### Response
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "requestType": 1,
      "requestTypeName": "Vacation",
      "status": 1,
      "employeeId": "guid",
      "employeeName": "John Doe",
      "branchId": "guid",
      "title": "Annual Leave Request",
      "description": "Family vacation",
      "requestedDate": "2026-02-01T10:00:00Z",
      "startDate": "2026-02-15",
      "endDate": "2026-02-20",
      "attachmentUrl": null,
      "managerComments": null,
      "rejectionReason": null,
      "approvedBy": null,
      "approvedDate": null,
      "vacationDetail": {
        "vacationTypeId": "guid",
        "vacationTypeName": "Annual Leave",
        "totalDays": 5,
        "managerId": "guid",
        "managerName": "Jane Manager"
      }
    }
  ]
}
```

### Request Status Enum
| Value | Name | Description |
|-------|------|-------------|
| 0 | Draft | Saved but not submitted |
| 1 | Pending | Awaiting approval |
| 2 | Approved | Request approved |
| 3 | Rejected | Request rejected |
| 4 | Cancelled | Cancelled by employee |

---

## 3. Submit Vacation Request

### Endpoint
```
POST /api/EmployeeRequests/vacation
```

### Request Body
```json
{
  "title": "Annual Leave - Summer Vacation",
  "description": "Family vacation to Egypt",
  "startDate": "2026-07-01",
  "endDate": "2026-07-10",
  "vacationTypeId": "guid-from-vacationTypes-dropdown",
  "totalDays": 10,
  "attachmentUrl": "https://storage.example.com/docs/medical.pdf",
  "emergencyContactName": "Ahmed Ali",
  "emergencyContactPhone": "+201234567890",
  "employeeId": null,
  "branchId": null
}
```

### Fields
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | string | ✅ | Request title (max 250 chars) |
| `description` | string | ❌ | Additional details |
| `startDate` | date | ✅ | Leave start date |
| `endDate` | date | ✅ | Leave end date |
| `vacationTypeId` | GUID | ✅ | **Selected from `vacationTypes` dropdown** |
| `totalDays` | decimal | ✅ | Number of days requested |
| `attachmentUrl` | string | ❌/✅ | URL of uploaded attachment (required if type requires it) |
| `emergencyContactName` | string | ❌ | Emergency contact name |
| `emergencyContactPhone` | string | ❌ | Emergency contact phone |
| `employeeId` | GUID | ❌ | Override employee (admin only) |
| `branchId` | GUID | ❌ | Override branch |

---

## 4. Submit Overtime Request

### Endpoint
```
POST /api/EmployeeRequests/overtime
```

### Request Body
```json
{
  "title": "Weekend Support Coverage",
  "description": "Production deployment support",
  "overtimeDate": "2026-02-08",
  "plannedHours": "04:00:00",
  "multiplier": 2.0,
  "projectCode": "PRJ-2026-001",
  "taskDescription": "Production deployment and monitoring",
  "attachmentUrl": null,
  "employeeId": null,
  "branchId": null
}
```

### Fields
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | string | ✅ | Request title |
| `description` | string | ❌ | Additional details |
| `overtimeDate` | date | ✅ | Date of overtime work |
| `plannedHours` | TimeSpan | ✅ | Duration (format: "HH:mm:ss") |
| `multiplier` | decimal | ✅ | Pay multiplier (default from `overtimeTypes`) |
| `projectCode` | string | ❌ | Project/Cost center code |
| `taskDescription` | string | ❌ | Work description |

---

## 5. Submit Training Request

### Endpoint
```
POST /api/EmployeeRequests/training
```

### Request Body
```json
{
  "title": "AWS Solutions Architect Certification",
  "description": "Professional certification for cloud architecture",
  "trainingTypeId": "guid-from-trainingTypes-dropdown",
  "trainingName": "AWS Solutions Architect Professional",
  "trainingProvider": "Amazon Web Services",
  "trainingLocation": "Online",
  "trainingStartDate": "2026-03-01",
  "trainingEndDate": "2026-03-05",
  "durationDays": 5,
  "estimatedCost": 3500.00,
  "currency": "USD",
  "objectives": "Obtain professional certification to lead cloud migration projects",
  "expectedOutcome": "AWS Solutions Architect Professional Certificate",
  "attachmentUrl": "https://storage.example.com/docs/course-details.pdf",
  "employeeId": null,
  "branchId": null
}
```

### Fields
| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `title` | string | ✅ | Request title |
| `trainingTypeId` | GUID | ✅ | **Selected from `trainingTypes` dropdown** |
| `trainingName` | string | ✅ | Name of course/training |
| `trainingProvider` | string | ❌ | Training provider name |
| `trainingLocation` | string | ❌ | Location (Online/City name) |
| `trainingStartDate` | date | ✅ | Training start date |
| `trainingEndDate` | date | ✅ | Training end date |
| `durationDays` | int | ✅ | Duration in days |
| `estimatedCost` | decimal | ❌ | Cost estimate |
| `currency` | string | ❌ | Currency code (default: EGP) |
| `objectives` | string | ❌ | Learning objectives |
| `expectedOutcome` | string | ❌ | Expected outcome/certification |

---

## 6. Submit Generic Request (Miscellaneous/Personal/Feedback)

For Miscellaneous, Personal, and Feedback requests, use the generic endpoint with type-specific details.

### Endpoint
```
POST /api/EmployeeRequests
```

### Request Body - Miscellaneous
```json
{
  "requestType": 4,
  "title": "Government Document Processing",
  "description": "Need HR letter for visa application",
  "startDate": null,
  "endDate": null,
  "attachmentUrl": null,
  "employeeId": null,
  "branchId": null
}
```

### Request Body - Personal
```json
{
  "requestType": 5,
  "title": "Family Emergency",
  "description": "Need to attend to urgent family matter",
  "startDate": "2026-02-05",
  "endDate": "2026-02-05",
  "attachmentUrl": null
}
```

### Request Body - Feedback
```json
{
  "requestType": 6,
  "title": "Suggestion: Improve Onboarding Process",
  "description": "Detailed feedback about improving new employee onboarding...",
  "startDate": null,
  "endDate": null,
  "attachmentUrl": null
}
```

---

## 7. Update Branch Request Settings (Admin Only)

Configure which request types are available for a branch.

### Endpoint
```
PUT /api/EmployeeRequests/branches/{branchId}/settings
```

### Authorization
Requires role: `Admin`, `OrganizationAdmin`, or `HRManager`

### Request Body
```json
[
  {
    "requestType": 1,
    "isVisibleToEmployees": true,
    "allowEmployeesToSubmit": true,
    "requireAttachment": false,
    "maxOpenRequests": 2,
    "customInstructions": "Submit vacation requests 3 days in advance"
  },
  {
    "requestType": 2,
    "isVisibleToEmployees": true,
    "allowEmployeesToSubmit": true,
    "requireAttachment": false,
    "maxOpenRequests": 5,
    "customInstructions": null
  },
  {
    "requestType": 3,
    "isVisibleToEmployees": true,
    "allowEmployeesToSubmit": true,
    "requireAttachment": true,
    "maxOpenRequests": null,
    "customInstructions": "Attach course details and quotation"
  }
]
```

---

## Login Response - Branch Request Access

When an employee logs in, the response includes available request types for their branch:

```json
{
  "accessToken": "jwt-token",
  "expiresAt": "2026-02-02T10:00:00Z",
  "userId": "guid",
  "fullName": "Ahmed Mohamed",
  "branchId": "guid",
  "employeeId": "guid",
  "branchRequestAccess": [
    {
      "requestType": 1,
      "displayName": "Vacation",
      "isVisibleToEmployees": true,
      "allowEmployeesToSubmit": true,
      "vacationTypes": [...]
    }
  ]
}
```

---

## Master Types Summary

### 1. VacationTypes
| Field | Description |
|-------|-------------|
| `isPaid` | Whether this leave type is paid |
| `maxDaysPerYear` | Maximum allowed days per year |
| `requiresAttachment` | Attachment mandatory |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Annual Leave, Sick Leave, Unpaid Leave, Emergency Leave

### 2. OvertimeTypes
| Field | Description |
|-------|-------------|
| `defaultMultiplier` | Default pay multiplier (1.5, 2.0, 2.5) |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Regular Overtime (1.5x), Weekend Overtime (2x), Holiday Overtime (2.5x)

### 3. TrainingTypes
| Field | Description |
|-------|-------------|
| `requiresBudgetApproval` | Needs budget/finance approval |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Internal Workshop, External Course, Online Learning, Conference/Seminar

### 4. MiscellaneousTypes
| Field | Description |
|-------|-------------|
| `requiresAttachment` | Attachment mandatory |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Government Paperwork, Equipment Request, Travel Arrangement, Other

### 5. PersonalTypes
| Field | Description |
|-------|-------------|
| `requiresAttachment` | Attachment mandatory |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Family Emergency, Medical Appointment, Personal Matter

### 6. FeedbackTypes
| Field | Description |
|-------|-------------|
| `isAnonymousAllowed` | Can submit anonymously |
| `requiresManagerApproval` | Needs manager approval |

**Seeded Values:** Product Feedback, Process Improvement, Workplace Concern, Recognition

---

## Frontend Implementation Checklist

### On Login
- [ ] Store `branchRequestAccess` from login response
- [ ] Build request type menu from available types

### Request Submission Form
- [ ] Show request type selector (filter by `isVisibleToEmployees`)
- [ ] Load type-specific dropdown based on selection
- [ ] Make attachment required if `requireAttachment: true`
- [ ] Show `customInstructions` as info message
- [ ] Disable submit if `allowEmployeesToSubmit: false`
- [ ] Validate against `maxOpenRequests` before submission

### Type-Specific Fields
| Request Type | Required Dropdown | Additional Fields |
|--------------|-------------------|-------------------|
| Vacation | `vacationTypes` | startDate, endDate, totalDays, emergencyContact |
| OverTime | `overtimeTypes` | overtimeDate, plannedHours, multiplier, projectCode |
| Training | `trainingTypes` | trainingName, provider, location, dates, cost |
| Miscellaneous | `miscellaneousTypes` | additionalNotes, referenceNumber |
| Personal | `personalTypes` | reason, isUrgent |
| Feedback | `feedbackTypes` | content, rating, isAnonymous |

---

## Error Codes

| HTTP Code | Description |
|-----------|-------------|
| 400 | Validation error / Missing required fields |
| 401 | Not authenticated |
| 403 | Request type disabled for branch / Max open requests reached |
| 404 | Employee/Branch not found |
| 500 | Server error |

---

# Master Type CRUD API Documentation

## Overview

These APIs allow administrators to manage the dropdown options for each request type. Each master type has full CRUD operations.

**Base URL:** `/api/RequestTypes`

**Authorization:** Requires `Admin`, `OrganizationAdmin`, or `HRManager` role (except GET endpoints which are public).

---

## VacationType CRUD

### Get All Vacation Types
```
GET /api/RequestTypes/vacation?isActive={bool}&searchTerm={string}
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "nameAr": "إجازة سنوية",
      "nameEn": "Annual Leave",
      "description": "Paid annual leave",
      "isPaid": true,
      "maxDaysPerYear": 21,
      "requiresAttachment": false,
      "requiresManagerApproval": true,
      "isActive": true,
      "sortOrder": 1,
      "createdDate": "2026-01-01T00:00:00Z",
      "modifiedDate": null
    }
  ]
}
```

### Get Vacation Type By ID
```
GET /api/RequestTypes/vacation/{id}
```

### Create Vacation Type
```
POST /api/RequestTypes/vacation
```

**Request Body:**
```json
{
  "nameAr": "إجازة مرضية",
  "nameEn": "Sick Leave",
  "description": "Medical leave with doctor certificate",
  "isPaid": true,
  "maxDaysPerYear": 15,
  "requiresAttachment": true,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 2
}
```

### Update Vacation Type
```
PUT /api/RequestTypes/vacation/{id}
```

**Request Body:**
```json
{
  "id": "guid",
  "nameAr": "إجازة مرضية",
  "nameEn": "Sick Leave",
  "description": "Medical leave with doctor certificate",
  "isPaid": true,
  "maxDaysPerYear": 20,
  "requiresAttachment": true,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 2
}
```

### Delete Vacation Type (Soft Delete)
```
DELETE /api/RequestTypes/vacation/{id}
```

---

## OvertimeType CRUD

### Get All Overtime Types
```
GET /api/RequestTypes/overtime?isActive={bool}&searchTerm={string}
```

### Get Overtime Type By ID
```
GET /api/RequestTypes/overtime/{id}
```

### Create Overtime Type
```
POST /api/RequestTypes/overtime
```

**Request Body:**
```json
{
  "nameAr": "وقت إضافي عادي",
  "nameEn": "Regular Overtime",
  "description": "Standard weekday overtime",
  "defaultMultiplier": 1.5,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 1
}
```

### Update Overtime Type
```
PUT /api/RequestTypes/overtime/{id}
```

### Delete Overtime Type (Soft Delete)
```
DELETE /api/RequestTypes/overtime/{id}
```

---

## TrainingType CRUD

### Get All Training Types
```
GET /api/RequestTypes/training?isActive={bool}&searchTerm={string}
```

### Get Training Type By ID
```
GET /api/RequestTypes/training/{id}
```

### Create Training Type
```
POST /api/RequestTypes/training
```

**Request Body:**
```json
{
  "nameAr": "تدريب داخلي",
  "nameEn": "Internal Workshop",
  "description": "In-house training sessions",
  "requiresBudgetApproval": false,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 1
}
```

### Update Training Type
```
PUT /api/RequestTypes/training/{id}
```

### Delete Training Type (Soft Delete)
```
DELETE /api/RequestTypes/training/{id}
```

---

## MiscellaneousType CRUD

### Get All Miscellaneous Types
```
GET /api/RequestTypes/miscellaneous?isActive={bool}&searchTerm={string}
```

### Get Miscellaneous Type By ID
```
GET /api/RequestTypes/miscellaneous/{id}
```

### Create Miscellaneous Type
```
POST /api/RequestTypes/miscellaneous
```

**Request Body:**
```json
{
  "nameAr": "معاملات حكومية",
  "nameEn": "Government Paperwork",
  "description": "Documents for government agencies",
  "requiresAttachment": false,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 1
}
```

### Update Miscellaneous Type
```
PUT /api/RequestTypes/miscellaneous/{id}
```

### Delete Miscellaneous Type (Soft Delete)
```
DELETE /api/RequestTypes/miscellaneous/{id}
```

---

## PersonalType CRUD

### Get All Personal Types
```
GET /api/RequestTypes/personal?isActive={bool}&searchTerm={string}
```

### Get Personal Type By ID
```
GET /api/RequestTypes/personal/{id}
```

### Create Personal Type
```
POST /api/RequestTypes/personal
```

**Request Body:**
```json
{
  "nameAr": "طوارئ عائلية",
  "nameEn": "Family Emergency",
  "description": "Urgent family matters",
  "requiresAttachment": false,
  "requiresManagerApproval": true,
  "isActive": true,
  "sortOrder": 1
}
```

### Update Personal Type
```
PUT /api/RequestTypes/personal/{id}
```

### Delete Personal Type (Soft Delete)
```
DELETE /api/RequestTypes/personal/{id}
```

---

## FeedbackType CRUD

### Get All Feedback Types
```
GET /api/RequestTypes/feedback?isActive={bool}&searchTerm={string}
```

### Get Feedback Type By ID
```
GET /api/RequestTypes/feedback/{id}
```

### Create Feedback Type
```
POST /api/RequestTypes/feedback
```

**Request Body:**
```json
{
  "nameAr": "اقتراح للتحسين",
  "nameEn": "Process Improvement",
  "description": "Suggestions to improve processes",
  "isAnonymousAllowed": true,
  "requiresManagerApproval": false,
  "isActive": true,
  "sortOrder": 1
}
```

### Update Feedback Type
```
PUT /api/RequestTypes/feedback/{id}
```

### Delete Feedback Type (Soft Delete)
```
DELETE /api/RequestTypes/feedback/{id}
```

---

# Branch Request Settings API

## Overview

These APIs allow administrators to configure which request types are available for employees in each branch.

**Base URL:** `/api/BranchRequestSettings`

**Authorization:** Requires `Admin`, `OrganizationAdmin`, or `HRManager` role.

---

## Get All Branch Settings
```
GET /api/BranchRequestSettings?branchId={guid}&requestType={int}
```

**Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `branchId` | GUID | Optional | Filter by branch |
| `requestType` | int | Optional | Filter by request type (1-6) |

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "branchId": "guid",
      "branchName": "Cairo Branch",
      "requestType": 1,
      "requestTypeName": "Vacation",
      "isVisibleToEmployees": true,
      "allowEmployeesToSubmit": true,
      "requireAttachment": false,
      "maxOpenRequests": 2,
      "customInstructions": "Submit at least 3 days in advance",
      "createdDate": "2026-01-01T00:00:00Z",
      "modifiedDate": null
    }
  ]
}
```

---

## Get Branch Setting By ID
```
GET /api/BranchRequestSettings/{id}
```

---

## Get Settings for a Specific Branch
```
GET /api/BranchRequestSettings/branches/{branchId}
```

Returns all 6 request type settings for the specified branch.

---

## Get Branches Summary
```
GET /api/BranchRequestSettings/branches-summary
```

Returns a summary of all branches showing how many request types are configured.

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "id": "guid",
      "nameEn": "Cairo Branch",
      "nameAr": "فرع القاهرة",
      "configuredRequestTypesCount": 4,
      "missingRequestTypesCount": 2
    }
  ]
}
```

---

## Create Single Branch Setting
```
POST /api/BranchRequestSettings
```

**Request Body:**
```json
{
  "branchId": "guid",
  "requestType": 1,
  "isVisibleToEmployees": true,
  "allowEmployeesToSubmit": true,
  "requireAttachment": false,
  "maxOpenRequests": 2,
  "customInstructions": "Please submit vacation requests 3 days in advance"
}
```

---

## Update Branch Setting
```
PUT /api/BranchRequestSettings/{id}
```

**Request Body:**
```json
{
  "id": "guid",
  "isVisibleToEmployees": true,
  "allowEmployeesToSubmit": true,
  "requireAttachment": true,
  "maxOpenRequests": 3,
  "customInstructions": "Updated instructions"
}
```

---

## Delete Branch Setting (Hard Delete)
```
DELETE /api/BranchRequestSettings/{id}
```

---

## Initialize Branch Settings
```
POST /api/BranchRequestSettings/branches/{branchId}/initialize
```

Creates settings for all 6 request types for a branch if they don't exist.

**Request Body (Optional):**
```json
{
  "branchId": "guid",
  "enableAllRequestTypes": true
}
```

**Response:**
```json
{
  "success": true,
  "message": "Branch settings initialized. 6 new settings created.",
  "data": [/* array of all 6 settings */]
}
```

---

## Admin Workflow: Setting Up a New Branch

1. **Check branches status:**
   ```
   GET /api/BranchRequestSettings/branches-summary
   ```
   
2. **Initialize settings for branches with missing configurations:**
   ```
   POST /api/BranchRequestSettings/branches/{branchId}/initialize
   ```
   
3. **Customize individual settings as needed:**
   ```
   PUT /api/BranchRequestSettings/{settingId}
   ```

---

## Frontend Admin Panel Implementation

### Master Types Management Page
1. For each of the 6 master types, provide a CRUD table
2. Allow sorting by `sortOrder` field
3. Show active/inactive status with toggle
4. Support Arabic and English names

### Branch Settings Page
1. Show branch selector dropdown
2. Display 6 cards (one per request type)
3. Each card shows:
   - Toggle: `isVisibleToEmployees`
   - Toggle: `allowEmployeesToSubmit`
   - Toggle: `requireAttachment`
   - Number input: `maxOpenRequests`
   - Textarea: `customInstructions`
4. "Initialize All" button to create default settings
