# User Login System - Complete Documentation

## 📋 Project Overview

This is a **secure user authentication and authorization system** built with .NET 10 and C# 14, featuring:
- ✅ Password Security using Salt and Hashing (BCrypt)
- ✅ JWT-based Authentication 
- ✅ Role-Based Access Control (RBAC)
- ✅ Permission-Based Authorization
- ✅ Refresh Token Management

---

## 🏗️ Architecture Overview

### Project Structure:
```
Solution/
├── Domain/                 # Entity models
├── Application/           # Business logic & interfaces
├── Infrastructure/        # Data access, services, migrations
└── Api/                   # REST API controllers
```

### Key Technologies:
- **Framework**: ASP.NET Core 10.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core
- **Password Hashing**: BCrypt.Net
- **JWT**: System.IdentityModel.Tokens.Jwt
- **API Documentation**: Swagger/OpenAPI

---

## 🔐 Security Implementation

### 1. **Password Security (5 Points)**

#### How it works:
```csharp
// Using BCrypt for secure hashing with salt
public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

**Features:**
- ✅ **Automatic Salting**: Each password gets a unique salt
- ✅ **One-way Hashing**: Passwords cannot be reversed
- ✅ **Workfactor**: BCrypt uses multiple rounds (12 by default) making brute-force attacks extremely slow
- ✅ **Adaptive**: When BCrypt version changes, it can still verify old hashes

**Example Flow:**
```
User Input: "MyPassword123"
    ↓
BCrypt Hash: "$2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUe"
    ↓
Store in DB: (Only the hash is stored, never the password)
```

---

### 2. **Authentication Mechanism (5 Points)**

#### JWT (JSON Web Tokens) Flow:

```
User Registration/Login
    ↓
Validate Credentials
    ↓
Generate JWT (Access Token) + Refresh Token
    ↓
Return to Client
    ↓
Client includes JWT in Authorization header for requests
```

#### JWT Structure:
```
Header.Payload.Signature

Example decoded payload:
{
  "sub": "user-id-uuid",
  "email": "user@test.com",
  "username": "user123",
  "roles": ["Admin", "Manager"],
  "permissions": ["read", "write", "delete"],
  "iat": 1704067200,
  "exp": 1704068100  (15 minutes from now)
}
```

#### Token Management:
```csharp
public class AuthService
{
    // Access Token: Short-lived (15 minutes)
    var accessToken = _jwtService.GenerateAccessToken(user);
    
    // Refresh Token: Long-lived (7 days)
    var refreshToken = _jwtService.GenerateRefreshToken();
}
```

**Flow Diagram:**
```
1. User Logs In
   POST /api/auth/login
   → Returns: AccessToken (15 min) + RefreshToken (7 days)

2. User Makes Request
   GET /api/test/protected
   Header: "Authorization: Bearer <AccessToken>"
   → ✅ Success (if valid)

3. Access Token Expires
   → ❌ 401 Unauthorized

4. Refresh Token
   POST /api/auth/refresh?refreshToken=<refreshToken>
   → Returns: New AccessToken + New RefreshToken
   → Old RefreshToken is revoked

5. Revoked Refresh Token
   → ❌ Cannot reuse old refresh token
```

---

### 3. **Authorization System - RBAC (5 Points)**

#### Role-Based Access Control (RBAC):

```
Users → Roles → Permissions

Example:
┌─────────────────────────────────────────┐
│ User: admin@test.com                    │
│   └─ Role: Admin                        │
│       ├─ Permission: read               │
│       ├─ Permission: write              │
│       ├─ Permission: delete             │
│       └─ Permission: manage_users       │
└─────────────────────────────────────────┘
```

#### Pre-configured Roles:
```csharp
// In database after migration:
1. Admin Role
   - read ✅
   - write ✅
   - delete ✅
   - manage_users ✅

2. Manager Role
   - read ✅
   - write ✅
   - delete ❌
   - manage_users ✅

3. User Role
   - read ✅
   - write ❌
   - delete ❌
   - manage_users ❌
```

#### Access Control Implementation:

```csharp
// Method 1: Role-based
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly() { }

// Method 2: Policy-based (Multiple roles)
[Authorize(Roles = "Admin,Manager")]
public IActionResult Management() { }

// Method 3: Policy-based (Custom claim)
[Authorize(Policy = "AdminOnly")]
public IActionResult SecureAdmin() { }
```

---

### 4. **Permission Management (Bonus)**

#### Permission System:
```csharp
public class Permission
{
    public Guid Id { get; set; }
    public string Name { get; set; }  // "read", "write", "delete", "manage_users"
    public List<RolePermission> RolePermissions { get; set; }
}

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; }
    
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; }
}
```

#### Permission-Based Endpoint Protection:
```csharp
// Only users with "read" permission can access
[Authorize(Policy = "CanRead")]
[HttpGet("read-data")]
public IActionResult ReadData() { }

// Only users with "write" permission can access
[Authorize(Policy = "CanWrite")]
[HttpPost("write-data")]
public IActionResult WriteData() { }

// Only users with "delete" permission can access
[Authorize(Policy = "CanDelete")]
[HttpDelete("delete-data/{id}")]
public IActionResult DeleteData(string id) { }

// Only users with "manage_users" permission can access
[Authorize(Policy = "CanManageUsers")]
[HttpGet("manage-users")]
public IActionResult ManageUsers() { }
```

---

## 📊 Database Schema

### Core Tables:

```sql
-- Users table
Users
├── Id (GUID)
├── Username (string)
├── Email (string)
├── PasswordHash (string)  [BCrypt hashed]
└── RefreshTokens (FK)

-- Roles table
Roles
├── Id (GUID)
└── Name (string)  ["Admin", "Manager", "User"]

-- Permissions table
Permissions
├── Id (GUID)
└── Name (string)  ["read", "write", "delete", "manage_users"]

-- UserRoles (Many-to-Many)
UserRoles
├── UserId (FK)
└── RoleId (FK)

-- RolePermissions (Many-to-Many)
RolePermissions
├── RoleId (FK)
└── PermissionId (FK)

-- RefreshTokens table
RefreshTokens
├── Id (GUID)
├── Token (string)
├── Expires (DateTime)
├── IsRevoked (bool)
└── UserId (FK)
```

---

## 🚀 Setup & Configuration

### 1. Database Configuration

Edit `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=UserLoginDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long!!!",
    "Issuer": "YourApp",
    "Audience": "YourAppUsers"
  }
}
```

### 2. Apply Migrations

```powershell
cd Infrastructure
dotnet ef database update
```

This will:
- ✅ Create all tables
- ✅ Seed Admin, Manager, User roles
- ✅ Seed permissions (read, write, delete, manage_users)
- ✅ Create role-permission mappings

### 3. Run the API

```powershell
cd Api
dotnet run
```

API runs at: `https://localhost:5001` or `http://localhost:5000`

---

## 🧪 Testing the System

### Using the REST Client (Api\ApiTesting.http):

#### Step 1: Register Users
```http
POST http://localhost:5000/api/auth/register
Content-Type: application/json

{
  "username": "admin_user",
  "email": "admin@test.com",
  "password": "SecurePassword123!"
}
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p"
}
```

#### Step 2: Update User Role (Admin only)

```http
POST http://localhost:5000/api/admin/assign-role
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "userId": "user-uuid-here",
  "roleId": "11111111-1111-1111-1111-111111111111"  // Admin role
}
```

#### Step 3: Test Permission-Based Access

```http
# This works (Admin has read permission)
GET http://localhost:5000/api/test/read-data
Authorization: Bearer <ADMIN_TOKEN>

# This works (Admin has write permission)
POST http://localhost:5000/api/test/write-data
Authorization: Bearer <ADMIN_TOKEN>

# This FAILS (Regular User doesn't have write permission)
POST http://localhost:5000/api/test/write-data
Authorization: Bearer <USER_TOKEN>
```

#### Step 4: Test Token Refresh

```http
POST http://localhost:5000/api/auth/refresh?refreshToken=<REFRESH_TOKEN>
```

Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "new-refresh-token-guid"
}
```

---

## 🔍 Key Endpoints

### Authentication Endpoints:
```
POST   /api/auth/register         Register new user
POST   /api/auth/login            Login user
POST   /api/auth/refresh           Refresh access token
```

### Test Endpoints (For Demo):
```
GET    /api/test/public           Public (no auth)
GET    /api/test/protected        Protected (any authenticated user)
GET    /api/test/me               Get current user info
GET    /api/test/admin            Admin only
GET    /api/test/management       Admin or Manager
GET    /api/test/read-data        Has "read" permission
POST   /api/test/write-data       Has "write" permission
DELETE /api/test/delete-data/{id} Has "delete" permission
GET    /api/test/manage-users     Has "manage_users" permission
```

### Admin Endpoints:
```
POST   /api/admin/assign-role     Assign role to user (Admin only)
POST   /api/admin/remove-role     Remove role from user (Admin only)
GET    /api/admin/users           Get all users (Admin only)
GET    /api/admin/roles           Get all roles (Admin only)
GET    /api/admin/permissions     Get all permissions (Admin only)
```

---

## ⚙️ How Authorization Works

### Request Processing Flow:

```
1. Client sends request with JWT in Authorization header
   GET /api/test/admin
   Authorization: Bearer eyJhbGc...

2. JwtBearerHandler extracts and validates the token
   ├─ Checks signature
   ├─ Validates expiration
   ├─ Validates issuer & audience
   └─ Creates ClaimsPrincipal from payload

3. AuthorizationMiddleware checks [Authorize] attributes
   ├─ Required role check
   ├─ Required policy check
   └─ Custom claim verification

4. Result:
   ✅ If authorized → Request proceeds to controller
   ❌ If unauthorized → 403 Forbidden
   ❌ If unauthenticated → 401 Unauthorized
```

### Authorization Attributes:

```csharp
// Role-based
[Authorize(Roles = "Admin")]              // Single role
[Authorize(Roles = "Admin,Manager")]      // Multiple roles

// Policy-based
[Authorize(Policy = "CanRead")]           // Custom policy

// All authenticated users
[Authorize]                                // Any logged-in user

// Public (no authentication)
// (No attribute needed)
```

---

## 🐛 Error Handling

### Global Exception Handler:

```csharp
// All exceptions are caught and returned as JSON
{
  "statusCode": 500,
  "message": "An error occurred",
  "type": "InvalidOperationException"
}
```

### Common Error Responses:

```json
// 401 Unauthorized - Invalid token or expired
{
  "statusCode": 401,
  "message": "Invalid refresh token",
  "type": "UnauthorizedAccessException"
}

// 403 Forbidden - Insufficient permissions
{
  "statusCode": 403,
  "message": "User is not authorized to access this resource"
}

// 400 Bad Request - Invalid input
{
  "statusCode": 400,
  "message": "User with this email already exists",
  "type": "InvalidOperationException"
}
```

---

## 📈 Security Best Practices Implemented

✅ **Password Security**
- BCrypt with automatic salt generation
- Multiple hashing rounds (12)
- One-way hashing (irreversible)

✅ **JWT Security**
- Signature verification
- Expiration validation
- Issuer & audience validation
- Secure token storage in JWT claims

✅ **Token Management**
- Short-lived access tokens (15 minutes)
- Long-lived refresh tokens (7 days) stored in database
- Token revocation (old refresh tokens can't be reused)

✅ **Authorization**
- Role-based access control
- Permission-based access control
- Multi-level security checks

✅ **Error Handling**
- Global exception handling
- Secure error messages (don't expose sensitive info)
- Detailed logging capability

---

## 📝 Summary of Points Coverage

### Requirement Points:

1. **Password Security using Salt and Hashing (5 points)** ✅
   - Using BCrypt with automatic salt generation
   - Secure password verification
   - Protection against brute-force attacks

2. **Authentication Mechanism (5 points)** ✅
   - JWT-based authentication
   - Access tokens with expiration
   - Refresh token flow
   - Token storage in secure HTTP headers

3. **Authorization System - Role-Based Access Control (5 points)** ✅
   - Three pre-configured roles (Admin, Manager, User)
   - Role-based endpoint protection
   - Policy-based authorization

4. **User Permissions Management (Bonus)** ✅
   - Permission model with 4 permissions
   - Permission-based endpoint protection
   - Dynamic role assignment
   - Admin panel for user management

---

## 🎯 Next Steps for Enhancement

1. **Add logging** - Log all authentication/authorization events
2. **Add email verification** - Send confirmation email on registration
3. **Add password reset** - Secure password recovery flow
4. **Add 2FA** - Two-factor authentication
5. **Add OAuth2** - Google/GitHub login integration
6. **Add auditing** - Track who accessed what and when
7. **Add API key** - Alternative authentication method
8. **Add rate limiting** - Prevent brute-force attacks

---

## 📞 Support

For issues or questions:
1. Check the test file: `Api\ApiTesting.http`
2. Review error messages in response
3. Check database connections in `appsettings.json`
4. Ensure JWT configuration is correct
5. Verify user exists and has correct role assigned

Happy coding! 🚀
