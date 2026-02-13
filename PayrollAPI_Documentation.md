# Payroll Overview & Payslips API Documentation

This document describes the two new API endpoints for the Payroll module with complete filtering, sorting, and pagination support.

---

## 📊 API 1: Payroll Overview

**Endpoint:** `GET /api/payroll/overview`

**Purpose:** Display payroll overview grouped by department with 3 statistics cards and a paginated grid.

### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| month | int | ✅ Yes | - | Month (1-12) |
| year | int | ✅ Yes | - | Year (e.g., 2026) |
| pageNumber | int | No | 1 | Page number for pagination |
| pageSize | int | No | 10 | Items per page |
| departmentId | Guid? | No | null | Filter by specific department |
| branchId | Guid? | No | null | Filter by specific branch |
| status | string? | No | null | Filter by status ("Pending", "Processed") |
| searchTerm | string? | No | null | Search departments by name (Arabic/English) |
| sortBy | string? | No | "DepartmentName" | Sort field: DepartmentName, EmployeeCount, GrossSalary, NetSalary, Status |
| sortDescending | bool | No | false | Sort in descending order |

### Example Requests

```http
# Get overview for February 2026 (default pagination)
GET /api/payroll/overview?month=2&year=2026

# Filter by department and sort by net salary descending
GET /api/payroll/overview?month=2&year=2026&departmentId=550e8400-e29b-41d4-a716-446655440000&sortBy=NetSalary&sortDescending=true

# Search for IT department with pending status
GET /api/payroll/overview?month=2&year=2026&searchTerm=IT&status=Pending

# Get specific branch with pagination
GET /api/payroll/overview?month=2&year=2026&branchId=660e8400-e29b-41d4-a716-446655440000&pageNumber=1&pageSize=20
```

### Response Structure

```json
{
  "success": true,
  "message": "Payroll overview retrieved successfully",
  "data": {
    "statistics": {
      "totalMonthlyPayroll": 362500.00,
      "lastMonthPayroll": 344200.00,
      "payrollChangePercentage": 5.32,
      "totalEmployees": 65,
      "fullTimeEmployees": 60,
      "contractEmployees": 5,
      "pendingPayrollActions": 3
    },
    "payrollData": {
      "items": [
        {
          "departmentId": "550e8400-e29b-41d4-a716-446655440000",
          "departmentName": "IT Department",
          "employeeCount": 25,
          "grossSalary": 125000.00,
          "totalDeductions": 12500.00,
          "netSalary": 112500.00,
          "status": "Processed",
          "month": 2,
          "year": 2026
        },
        {
          "departmentId": "660e8400-e29b-41d4-a716-446655440001",
          "departmentName": "HR Department",
          "employeeCount": 10,
          "grossSalary": 50000.00,
          "totalDeductions": 5000.00,
          "netSalary": 45000.00,
          "status": "Pending",
          "month": 2,
          "year": 2026
        }
      ],
      "totalCount": 10,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 1
    }
  },
  "errors": null
}
```

### Statistics Cards (3 Cards)

#### Card 1: Total Monthly Payroll 💰
- **Value:** `totalMonthlyPayroll` (current month net salary total)
- **Comparison:** `lastMonthPayroll`
- **Trend:** `payrollChangePercentage` (positive = increase, negative = decrease)

#### Card 2: Employees on Payroll 👥
- **Value:** `totalEmployees`
- **Breakdown:** `fullTimeEmployees` + `contractEmployees`

#### Card 3: Pending Payroll Actions ⏳
- **Value:** `pendingPayrollActions` (unpaid payslips count)

### Grid Columns

| Column | Field | Description |
|--------|-------|-------------|
| Department | departmentName | Department name (English) |
| Employees | employeeCount | Number of employees in department |
| Gross Salary | grossSalary | Total gross salary for department |
| Deductions | totalDeductions | Total deductions |
| Net Salary | netSalary | Total net salary |
| Status | status | "Processed" or "Pending" |
| Month/Year | month, year | Payroll period |

---

## 📄 API 2: Payslips with Statistics

**Endpoint:** `GET /api/payroll/payslips`

**Purpose:** Display individual employee payslips with 3 statistics cards and a paginated grid.

### Request Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| month | int | ✅ Yes | - | Month (1-12) |
| year | int | ✅ Yes | - | Year (e.g., 2026) |
| pageNumber | int | No | 1 | Page number for pagination |
| pageSize | int | No | 10 | Items per page |
| employeeId | Guid? | No | null | Filter by specific employee |
| departmentId | Guid? | No | null | Filter by department |
| isPaid | bool? | No | null | Filter by payment status (true/false) |
| searchTerm | string? | No | null | Search by employee code, name, or payslip number |
| sortBy | string? | No | "EmployeeCode" | Sort field: EmployeeCode, EmployeeName, Department, GrossSalary, NetSalary, Status |
| sortDescending | bool | No | false | Sort in descending order |

### Example Requests

```http
# Get all payslips for February 2026
GET /api/payroll/payslips?month=2&year=2026

# Filter paid payslips only
GET /api/payroll/payslips?month=2&year=2026&isPaid=true

# Search employee by name
GET /api/payroll/payslips?month=2&year=2026&searchTerm=Ahmed

# Filter by department and sort by salary
GET /api/payroll/payslips?month=2&year=2026&departmentId=550e8400-e29b-41d4-a716-446655440000&sortBy=NetSalary&sortDescending=true

# Get pending payslips with pagination
GET /api/payroll/payslips?month=2&year=2026&isPaid=false&pageNumber=1&pageSize=20
```

### Response Structure

```json
{
  "success": true,
  "message": "Payslips retrieved successfully",
  "data": {
    "statistics": {
      "totalPayslips": 65,
      "sentPayslips": 62,
      "pendingPayslips": 3,
      "averageNetSalary": 5576.92,
      "medianSalary": 5200.00,
      "totalDeductions": 36250.00,
      "taxDeductions": 18000.00,
      "insuranceDeductions": 12000.00,
      "otherDeductions": 6250.00
    },
    "payslipsData": {
      "items": [
        {
          "id": "770e8400-e29b-41d4-a716-446655440000",
          "employeeCode": "EMP001",
          "employeeNameAr": "أحمد محمد",
          "employeeNameEn": "Ahmed Mohamed",
          "departmentName": "IT",
          "payslipNumber": "PS-2026-02-001",
          "basicSalary": 5000.00,
          "grossSalary": 5500.00,
          "totalDeductions": 500.00,
          "netSalary": 5000.00,
          "month": 2,
          "year": 2026,
          "monthName": "February",
          "isPaid": true,
          "paidDate": "2026-02-28T10:00:00Z",
          "status": "Paid",
          "pdfFileUrl": "https://storage.com/payslips/PS-2026-02-001.pdf",
          "generatedDate": "2026-02-25T10:00:00Z"
        }
      ],
      "totalCount": 65,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 7
    }
  },
  "errors": null
}
```

### Statistics Cards (3 Cards)

#### Card 1: Payslips This Month 📋
- **Value:** `totalPayslips`
- **Breakdown:** `sentPayslips` sent / `pendingPayslips` pending

#### Card 2: Average Net Salary 📊
- **Value:** `averageNetSalary` (mean)
- **Comparison:** `medianSalary` (median)

#### Card 3: Total Deductions 📉
- **Value:** `totalDeductions`
- **Breakdown:** 
  - Tax: `taxDeductions`
  - Insurance: `insuranceDeductions`
  - Other: `otherDeductions`

### Grid Columns

| Column | Field | Description |
|--------|-------|-------------|
| Employee Code | employeeCode | Unique employee identifier |
| Employee Name | employeeNameEn / employeeNameAr | Employee full name |
| Department | departmentName | Department name |
| Payslip Number | payslipNumber | Unique payslip identifier |
| Basic Salary | basicSalary | Base salary amount |
| Gross Salary | grossSalary | Total earnings |
| Deductions | totalDeductions | Total deductions |
| Net Salary | netSalary | Final payment amount |
| Month | monthName | Month name (localized) |
| Status | status | "Paid", "Generated", or "Draft" |
| PDF | pdfFileUrl | Download link (if generated) |

---

## 🎨 Angular Implementation Example

### 1. TypeScript Service

```typescript
// payroll.service.ts
export interface PayrollOverviewParams {
  month: number;
  year: number;
  pageNumber?: number;
  pageSize?: number;
  departmentId?: string;
  branchId?: string;
  status?: string;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface PayslipsParams {
  month: number;
  year: number;
  pageNumber?: number;
  pageSize?: number;
  employeeId?: string;
  departmentId?: string;
  isPaid?: boolean;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

@Injectable({ providedIn: 'root' })
export class PayrollService {
  private apiUrl = 'api/payroll';

  constructor(private http: HttpClient) {}

  getPayrollOverview(params: PayrollOverviewParams): Observable<ApiResponse<PayrollOverviewResponse>> {
    return this.http.get<ApiResponse<PayrollOverviewResponse>>(
      `${this.apiUrl}/overview`,
      { params: this.toHttpParams(params) }
    );
  }

  getPayslips(params: PayslipsParams): Observable<ApiResponse<PayslipsResponse>> {
    return this.http.get<ApiResponse<PayslipsResponse>>(
      `${this.apiUrl}/payslips`,
      { params: this.toHttpParams(params) }
    );
  }

  private toHttpParams(obj: any): HttpParams {
    let params = new HttpParams();
    Object.keys(obj).forEach(key => {
      if (obj[key] !== null && obj[key] !== undefined) {
        params = params.set(key, obj[key].toString());
      }
    });
    return params;
  }
}
```

### 2. Component Example

```typescript
// payroll-overview.component.ts
export class PayrollOverviewComponent implements OnInit {
  statistics: PayrollStatistics;
  payrollData: PayrollOverviewDto[] = [];
  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;

  filters = {
    month: new Date().getMonth() + 1,
    year: new Date().getFullYear(),
    departmentId: null,
    branchId: null,
    status: null,
    searchTerm: '',
    sortBy: 'DepartmentName',
    sortDescending: false
  };

  constructor(private payrollService: PayrollService) {}

  ngOnInit(): void {
    this.loadPayrollOverview();
  }

  loadPayrollOverview(): void {
    const params: PayrollOverviewParams = {
      ...this.filters,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    };

    this.payrollService.getPayrollOverview(params).subscribe({
      next: (response) => {
        this.statistics = response.data.statistics;
        this.payrollData = response.data.payrollData.items;
        this.totalCount = response.data.payrollData.totalCount;
      },
      error: (err) => console.error('Error loading payroll overview', err)
    });
  }

  onFilterChange(): void {
    this.pageNumber = 1; // Reset to first page
    this.loadPayrollOverview();
  }

  onPageChange(page: number): void {
    this.pageNumber = page;
    this.loadPayrollOverview();
  }

  onSort(field: string): void {
    if (this.filters.sortBy === field) {
      this.filters.sortDescending = !this.filters.sortDescending;
    } else {
      this.filters.sortBy = field;
      this.filters.sortDescending = false;
    }
    this.loadPayrollOverview();
  }
}
```

### 3. HTML Template Example

```html
<!-- Statistics Cards -->
<div class="statistics-cards">
  <div class="card">
    <h3>Total Monthly Payroll</h3>
    <p class="value">{{ statistics.totalMonthlyPayroll | currency:'EGP' }}</p>
    <p class="trend" [class.up]="statistics.payrollChangePercentage > 0">
      {{ statistics.payrollChangePercentage }}% vs last month
    </p>
  </div>

  <div class="card">
    <h3>Employees on Payroll</h3>
    <p class="value">{{ statistics.totalEmployees }}</p>
    <p class="breakdown">
      {{ statistics.fullTimeEmployees }} Full-time, 
      {{ statistics.contractEmployees }} Contract
    </p>
  </div>

  <div class="card">
    <h3>Pending Actions</h3>
    <p class="value">{{ statistics.pendingPayrollActions }}</p>
    <p class="subtitle">Requires attention</p>
  </div>
</div>

<!-- Filters -->
<div class="filters">
  <input type="text" [(ngModel)]="filters.searchTerm" 
         placeholder="Search departments..." 
         (ngModelChange)="onFilterChange()">
  
  <select [(ngModel)]="filters.departmentId" (ngModelChange)="onFilterChange()">
    <option [value]="null">All Departments</option>
    <option *ngFor="let dept of departments" [value]="dept.id">
      {{ dept.name }}
    </option>
  </select>

  <select [(ngModel)]="filters.status" (ngModelChange)="onFilterChange()">
    <option [value]="null">All Status</option>
    <option value="Processed">Processed</option>
    <option value="Pending">Pending</option>
  </select>
</div>

<!-- Data Grid -->
<table class="data-grid">
  <thead>
    <tr>
      <th (click)="onSort('DepartmentName')">Department</th>
      <th (click)="onSort('EmployeeCount')">Employees</th>
      <th (click)="onSort('GrossSalary')">Gross Salary</th>
      <th (click)="onSort('NetSalary')">Net Salary</th>
      <th (click)="onSort('Status')">Status</th>
      <th>Actions</th>
    </tr>
  </thead>
  <tbody>
    <tr *ngFor="let row of payrollData">
      <td>{{ row.departmentName }}</td>
      <td>{{ row.employeeCount }}</td>
      <td>{{ row.grossSalary | currency:'EGP' }}</td>
      <td>{{ row.netSalary | currency:'EGP' }}</td>
      <td>
        <span class="badge" [class.processed]="row.status === 'Processed'">
          {{ row.status }}
        </span>
      </td>
      <td>
        <button (click)="viewDetails(row)">View Details</button>
      </td>
    </tr>
  </tbody>
</table>

<!-- Pagination -->
<div class="pagination">
  <button [disabled]="pageNumber === 1" (click)="onPageChange(pageNumber - 1)">
    Previous
  </button>
  <span>Page {{ pageNumber }} of {{ totalPages }}</span>
  <button [disabled]="pageNumber === totalPages" (click)="onPageChange(pageNumber + 1)">
    Next
  </button>
</div>
```

---

## ✅ Features Implemented

### Filters
- ✅ Month & Year (required)
- ✅ Department filter (both APIs)
- ✅ Branch filter (Overview only)
- ✅ Employee filter (Payslips only)
- ✅ Status/Payment status filter
- ✅ Search by text (department names / employee info)

### Sorting
- ✅ Multiple sort fields
- ✅ Ascending/Descending order
- ✅ Default sorting

### Pagination
- ✅ Page number & size
- ✅ Total count calculation
- ✅ Total pages calculation

### Statistics
- ✅ 3 cards per page with real-time calculations
- ✅ Comparison with previous period (Overview)
- ✅ Detailed breakdowns

### Performance
- ✅ Efficient database queries with proper includes
- ✅ Grouped queries for department summaries
- ✅ Pagination at database level

---

## 🔒 Authorization

Both endpoints require authentication (`[Authorize]` attribute).

Recommended role-based access:
- **HRManager**: Full access
- **HRSpecialist**: Full access
- **DepartmentManager**: Access to own department only
- **Employee**: No access (use existing `/my-payslips` endpoint)

---

## 📝 Notes

1. **Month/Year are required** - Both endpoints require valid month (1-12) and year parameters
2. **Status values** - "Pending" = has unpaid payslips, "Processed" = all payslips paid
3. **Median calculation** - Properly calculated for even/odd count of values
4. **Department grouping** - Overview API groups by department automatically
5. **Null handling** - Employees without departments show as "No Department"

---

## 🚀 Ready to Use

Both APIs are now ready for integration with your Angular application. All files have been created and the build is successful!
