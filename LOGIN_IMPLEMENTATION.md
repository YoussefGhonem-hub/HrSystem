# Login Authentication Implementation

## Overview
The login system authenticates users and generates JWT tokens with comprehensive claims including role information, employee details (job title and direct manager ID).

## Files Created/Modified

### 1. Application Layer

#### LoginCommand.cs (`Application/Auth/Commands/Login/`)
- **Purpose**: Command object for login requests
- **Properties**:
  - `Username` (string): User's username
  - `Password` (string): User's password
- **Response**: `LoginResponse` containing:
  - `AccessToken`: JWT token string
  - `ExpiresAt`: Token expiration timestamp
  - `UserId`: User's unique identifier
  - `FullName`: User's full name
  - `Email`: User's email address
  - `Roles`: List of assigned roles

#### LoginCommandHandler.cs (`Application/Auth/Commands/Login/`)
- **Purpose**: Processes login requests and generates JWT tokens
- **Process**:
  1. Validates username exists via `UserManager`
  2. Checks if user account is active
  3. Verifies password using `UserManager.CheckPasswordAsync`
  4. Retrieves user roles from ASP.NET Identity
  5. **If user is an employee**: Queries employee data including:
     - Employee ID
     - Job Title ID
     - Direct Manager ID
     - Department Name (English or Arabic)
  6. Generates JWT token with all claims
  7. Returns structured response with token and user information

- **Error Handling**:
  - `Error.Unauthorized`: Invalid credentials
  - `Error.Forbidden`: Inactive user account

#### LoginCommandValidator.cs (`Application/Auth/Commands/Login/`)
- **Validation Rules**:
  - Username: Required, max 256 characters
  - Password: Required, minimum 6 characters

### 2. Infrastructure Layer

#### TokenService.cs (`Infrastructure/Identity/`)
- **Interface Updates**: Added optional parameters for employee information:
  - `employeeId` (Guid?)
  - `jobTitleId` (Guid?)
  - `directManagerId` (Guid?)

- **JWT Claims Generated**:

  **Standard Claims**:
  - `sub`: User ID (Subject)
  - `nameid`: User ID (Name Identifier)
  - `Id`: User ID (custom claim)
  - `email`: User email
  - `name`: User full name
  - `preferred_username`: Username
  - `jti`: JWT ID (unique token identifier)

  **Role Claims**:
  - `role`: Individual role claims (one per role)
  - `roles`: JSON array of all roles

  **Department Claim**:
  - `department`: Department name (if available)

  **Employee-Specific Claims** (✅ As Requested):
  - `employee_id`: Employee's unique identifier
  - `job_title_id`: Employee's job title ID
  - `direct_manager_id`: Employee's direct manager ID

### 3. API Layer

#### AuthController.cs (`API/Controllers/`)
- **Endpoint**: `POST /api/auth/login`
- **Authorization**: `[AllowAnonymous]` - Public endpoint
- **Request Body**: 
  ```json
  {
    "username": "string",
    "password": "string"
  }
  ```
- **Response (200 OK)**:
  ```json
  {
    "success": true,
    "message": "Login successful",
    "data": {
      "accessToken": "eyJhbGc...",
      "expiresAt": "2024-01-15T14:30:00Z",
      "userId": "guid",
      "fullName": "John Doe",
      "email": "john.doe@example.com",
      "roles": ["Employee", "Manager"]
    }
  }
  ```

## JWT Token Claims Structure

When an employee logs in, the JWT token contains:

```
{
  "sub": "user-guid",
  "nameid": "user-guid",
  "Id": "user-guid",
  "email": "employee@company.com",
  "name": "Employee Full Name",
  "preferred_username": "employee.username",
  "jti": "unique-token-id",
  "department": "IT Department",
  "employee_id": "employee-guid",           // ✅ Employee ID
  "job_title_id": "jobtitle-guid",          // ✅ Job Title ID
  "direct_manager_id": "manager-guid",      // ✅ Direct Manager ID
  "role": ["Employee", "Manager"],          // ✅ Roles
  "roles": "[\"Employee\",\"Manager\"]",
  "iss": "your-issuer",
  "aud": "your-audience",
  "exp": 1705329000,
  "nbf": 1705325400
}
```

## Usage in Authorization

With these claims in the JWT token, you can:

1. **Check User Roles**: Use `[Authorize(Roles = "Manager")]` or access via `HttpContext.User.IsInRole("Manager")`

2. **Access Employee Information**: Extract claims in controllers or middleware:
   ```csharp
   var employeeId = User.FindFirst("employee_id")?.Value;
   var jobTitleId = User.FindFirst("job_title_id")?.Value;
   var directManagerId = User.FindFirst("direct_manager_id")?.Value;
   ```

3. **Hierarchical Approval**: The validators can check if:
   - User is the direct manager: `currentUserId == leaveRequest.Employee.DirectManagerId`
   - User has HR role: `User.IsInRole("HrManager")`
   - User is the employee's manager: Compare `employee_id` claim with `DirectManagerId` from database

## Security Considerations

- Passwords are hashed and verified via ASP.NET Identity
- Failed login attempts return generic "Invalid username or password" to prevent user enumeration
- Inactive accounts cannot login (returns 403 Forbidden)
- JWT tokens expire based on `JwtSettings.DurationInMinutes`
- Tokens are signed using HMAC-SHA256 algorithm

## Testing the Login Endpoint

**Request**:
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "ahmed.ibrahim",
  "password": "your-password"
}
```

**Success Response**:
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresAt": "2024-01-15T15:30:00Z",
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "fullName": "Ahmed Ibrahim",
    "email": "ahmed.ibrahim@company.com",
    "roles": ["Employee", "CEO"]
  }
}
```

**Error Responses**:
- `401 Unauthorized`: Invalid credentials
- `403 Forbidden`: Account is inactive
- `400 Bad Request`: Validation errors (missing username/password)

## Integration with Existing Features

The JWT token can now be used in:

1. **Leave Approval Workflow**: 
   - Validator checks if logged-in user (from `employee_id` claim) matches `DirectManagerId`
   - Or if user has "HrManager" role

2. **Multi-Branch Operations**:
   - Employee's branch can be queried using `employee_id` from token
   - Branch-specific authorization can be implemented

3. **Department-Based Access**:
   - Department name is included in token claims
   - Can filter data by department without additional queries

## Next Steps

1. Update existing API endpoints to use `[Authorize]` attribute
2. Create helper methods to extract employee information from claims
3. Implement token refresh mechanism (RefreshTokenService already exists)
4. Add audit logging for authentication events
5. Consider implementing rate limiting for login endpoint
