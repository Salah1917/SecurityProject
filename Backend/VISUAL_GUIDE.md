# 🎯 Visual Guide - How Everything Works

## 1️⃣ User Registration Flow

```
┌─────────────────────────────────────────────────────────────┐
│ User Submits Registration Form                              │
│ - Username: john_doe                                        │
│ - Email: john@example.com                                   │
│ - Password: MySecurePass123!                                │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────▼───────────┐
         │  AuthController       │
         │  POST /auth/register  │
         └───────────┬───────────┘
                     │
         ┌───────────▼─────────────┐
         │  AuthService.Register   │
         │  - Create User object   │
         │  - Hash password        │
         └───────────┬─────────────┘
                     │
         ┌───────────▼────────────────────┐
         │  PasswordHasher.Hash()         │
         │  Input: MySecurePass123!       │
         │  Uses: BCrypt (12 rounds)      │
         │  Output: $2a$12$R9h/cIPz...   │
         └───────────┬────────────────────┘
                     │
         ┌───────────▼──────────────────────┐
         │  UserRepository.AddAsync()       │
         │  Stores in Users table:          │
         │  - Id: new GUID                  │
         │  - Username: john_doe           │
         │  - Email: john@example.com       │
         │  - PasswordHash: $2a$12$...     │
         └───────────┬──────────────────────┘
                     │
         ┌───────────▼──────────────────┐
         │  Assign Default Role         │
         │  - Get "User" role           │
         │  - Create UserRole entry     │
         │  - User now has read perm    │
         └───────────┬──────────────────┘
                     │
         ┌───────────▼─────────────────────┐
         │  JwtService.GenerateToken()     │
         │  - Load roles from DB           │
         │  - Load permissions from DB     │
         │  - Create JWT with claims       │
         │  - Sign with secret key         │
         └───────────┬─────────────────────┘
                     │
         ┌───────────▼──────────────────────┐
         │  Create Refresh Token           │
         │  - Generate random GUID         │
         │  - Set expiry: +7 days          │
         │  - Store in RefreshTokens table │
         │  - Mark as not revoked          │
         └───────────┬──────────────────────┘
                     │
┌────────────────────▼──────────────────────────┐
│ Return to Client:                             │
│ {                                              │
│   "accessToken": "eyJhbGc...",              │
│   "refreshToken": "a1b2c3d4-..."            │
│ }                                              │
└────────────────────────────────────────────────┘
```

---

## 2️⃣ Login Flow

```
┌──────────────────────────────────────┐
│ User Submits Login Form              │
│ - Email: john@example.com            │
│ - Password: MySecurePass123!         │
└──────────────┬───────────────────────┘
               │
       ┌───────▼─────────────┐
       │  AuthController     │
       │  POST /auth/login   │
       └───────┬─────────────┘
               │
       ┌───────▼──────────────────────┐
       │  AuthService.Login()         │
       │  - Get user by email         │
       └───────┬──────────────────────┘
               │
       ┌───────▼──────────────────────────┐
       │  Query Users table:              │
       │  SELECT * FROM Users             │
       │  WHERE Email = 'john@...'        │
       │  Include: Roles, Permissions     │
       └───────┬──────────────────────────┘
               │
       ┌───────▼────────────────────────────────┐
       │  PasswordHasher.Verify()               │
       │  Input Password: MySecurePass123!      │
       │  Stored Hash: $2a$12$...              │
       │  Verify(input, hash) → TRUE/FALSE     │
       └───────┬────────────────────────────────┘
               │
       ┌───────▼─────────────┐
       │  If valid:          │
       │  Generate tokens    │
       │  (same as register) │
       └───────┬─────────────┘
               │
┌──────────────▼──────────────────┐
│ Return to Client:               │
│ {                                │
│   "accessToken": "eyJhbGc...",  │
│   "refreshToken": "x9y8z7w6-..." │
│ }                                │
└──────────────────────────────────┘

If invalid:
┌──────────────────────────────────┐
│ Return 401 Unauthorized:         │
│ {                                │
│   "message": "Invalid email..." │
│ }                                │
└──────────────────────────────────┘
```

---

## 3️⃣ Protected Endpoint Access Flow

```
┌────────────────────────────────────────┐
│ Client Makes Request:                  │
│ GET /api/test/protected                │
│ Authorization: Bearer eyJhbGc...       │
└────────────┬───────────────────────────┘
             │
    ┌────────▼──────────────────┐
    │  JwtBearerHandler         │
    │  - Extract token from     │
    │    Authorization header   │
    │  - Decode JWT payload     │
    │  - Validate signature     │
    │  - Check expiration       │
    │  - Validate issuer        │
    │  - Validate audience      │
    └────────┬──────────────────┘
             │
    ┌────────▼──────────────────────┐
    │  If Valid:                     │
    │  Create ClaimsPrincipal       │
    │  With claims:                  │
    │  - sub (userId)                │
    │  - email                       │
    │  - username                    │
    │  - roles: [User]               │
    │  - permissions: [read]         │
    └────────┬──────────────────────┘
             │
    ┌────────▼─────────────────────────┐
    │ AuthorizationMiddleware          │
    │ Check [Authorize] attribute      │
    │ - Required roles?                │
    │ - Required policies?             │
    │ - Required permissions?          │
    └────────┬─────────────────────────┘
             │
    ┌────────▼─────────────────────────────┐
    │ Request allowed!                     │
    │ Route to TestController.Protected()  │
    └────────┬─────────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│ Return to Client:                       │
│ {                                        │
│   "message": "You are authenticated"  │
│ }                                        │
└─────────────────────────────────────────┘
```

---

## 4️⃣ Permission-Based Access Flow

```
┌────────────────────────────────────────┐
│ Client Makes Request:                  │
│ POST /api/test/write-data              │
│ Authorization: Bearer <token>          │
│ (User has only "read" permission)      │
└────────────┬───────────────────────────┘
             │
    ┌────────▼──────────────────┐
    │  JwtBearerHandler         │
    │  - Validate token         │
    │  - Create ClaimsPrincipal │
    │  - Add claims including   │
    │    permissions: [read]    │
    └────────┬──────────────────┘
             │
    ┌────────▼──────────────────────────────┐
    │ AuthorizationMiddleware               │
    │ Check [Authorize(Policy="CanWrite")]  │
    │ - Required permission: "write"        │
    │ - User has: "read"                    │
    │ - Policy requires claim: "write"      │
    │ - User's claims: only has "read"      │
    └────────┬──────────────────────────────┘
             │
┌────────────▼──────────────────────────────┐
│ BLOCKED - 403 Forbidden                   │
│ {                                          │
│   "statusCode": 403,                      │
│   "message": "User is not authorized..."  │
│ }                                          │
└───────────────────────────────────────────┘

Same endpoint WITH Admin token (has "write"):
┌────────────────────────────────────────┐
│ Admin user has permissions:             │
│ permissions: [read, write, delete,      │
│              manage_users]              │
└────────────┬───────────────────────────┘
             │
    ┌────────▼──────────────────────────┐
    │ AuthorizationMiddleware          │
    │ - Required permission: "write"   │
    │ - User has: "write" ✓            │
    │ - Policy matched!                │
    └────────┬──────────────────────────┘
             │
┌────────────▼────────────────────────────┐
│ SUCCESS - 200 OK                         │
│ {                                        │
│   "message": "You have write perm...",  │
│   "savedData": { ... }                   │
│ }                                        │
└────────────────────────────────────────┘
```

---

## 5️⃣ Token Refresh Flow

```
┌──────────────────────────────────┐
│ Access token expired (15 min)    │
│ Client has:                      │
│ - Expired Access Token           │
│ - Valid Refresh Token            │
└──────────────┬───────────────────┘
               │
       ┌───────▼──────────────────────┐
       │ AuthController.Refresh()     │
       │ POST /auth/refresh           │
       │ ?refreshToken=<guid>         │
       └───────┬──────────────────────┘
               │
       ┌───────▼──────────────────────┐
       │ UserRepository.GetRefresh    │
       │ TokenAsync()                 │
       │ Query database:              │
       │ SELECT * FROM RefreshTokens  │
       │ WHERE Token = '<guid>'       │
       └───────┬──────────────────────┘
               │
       ┌───────▼────────────────────────────┐
       │ AuthService.RefreshToken()         │
       │ Validate refresh token:            │
       │ ✓ Token found                      │
       │ ✓ IsRevoked = false                │
       │ ✓ Expires > now                    │
       └───────┬────────────────────────────┘
               │
       ┌───────▼────────────────────────────┐
       │ Mark old token as revoked:         │
       │ UPDATE RefreshTokens               │
       │ SET IsRevoked = true               │
       │ WHERE Token = '<old_guid>'         │
       └───────┬────────────────────────────┘
               │
       ┌───────▼────────────────────────┐
       │ Generate new tokens:           │
       │ - New Access Token (15 min)    │
       │ - New Refresh Token (7 days)   │
       │ - Save new token in DB         │
       └───────┬────────────────────────┘
               │
┌──────────────▼────────────────────────────┐
│ Return to Client:                         │
│ {                                          │
│   "accessToken": "new_token_here",       │
│   "refreshToken": "new_refresh_here"     │
│ }                                          │
│                                            │
│ Old refresh token cannot be reused!       │
│ (automatically revoked)                   │
└────────────────────────────────────────────┘
```

---

## 6️⃣ Role Assignment Flow

```
┌──────────────────────────────────┐
│ Admin Makes Request:             │
│ POST /api/admin/assign-role      │
│ Authorization: Bearer <admin_tok> │
│ Body:                             │
│ {                                 │
│   "userId": "john-id",           │
│   "roleId": "admin-role-id"      │
│ }                                 │
└──────────────┬───────────────────┘
               │
       ┌───────▼──────────────────────┐
       │ AdminController              │
       │ Check: User is Admin? ✓      │
       │ [Authorize(Roles="Admin")]   │
       └───────┬──────────────────────┘
               │
       ┌───────▼────────────────────────┐
       │ Get user from database:        │
       │ SELECT * FROM Users            │
       │ WHERE Id = 'john-id'           │
       └───────┬────────────────────────┘
               │
       ┌───────▼────────────────────────┐
       │ Get role from database:        │
       │ SELECT * FROM Roles            │
       │ WHERE Id = 'admin-role-id'     │
       └───────┬────────────────────────┘
               │
       ┌───────▼────────────────────┐
       │ Create UserRole entry:     │
       │ INSERT INTO UserRoles      │
       │ (UserId, RoleId)           │
       │ VALUES ('john-id',         │
       │         'admin-role-id')   │
       └───────┬────────────────────┘
               │
┌──────────────▼──────────────────────────┐
│ Success:                                 │
│ {                                        │
│   "message": "Role 'Admin' assigned..."  │
│ }                                        │
│                                          │
│ Next time John logs in:                 │
│ - Will get Admin role in JWT             │
│ - Will get all Admin permissions        │
│ - Will access admin endpoints          │
└──────────────────────────────────────────┘
```

---

## 7️⃣ Database Relationship Diagram

```
┌─────────────────┐         ┌──────────────┐
│     Users       │         │   Roles      │
├─────────────────┤  1..*   ├──────────────┤
│ Id (PK)         │────────▶│ Id (PK)      │
│ Username        │         │ Name: "User" │
│ Email           │         │ Name:"Admin" │
│ PasswordHash    │         │ Name:"Mgr"   │
└─────────────────┘         └──────┬───────┘
        ▲                          │
        │                          │ 1..*
        │                    ┌─────▼──────────────┐
        │                    │ RolePermissions    │
        │                    ├────────────────────┤
        │                    │ RoleId (FK)        │
        │                    │ PermissionId (FK)  │
        │                    └─────┬──────────────┘
        │                          │
        │                          ▼
        │                    ┌──────────────────┐
        │                    │  Permissions     │
        │                    ├──────────────────┤
        │                    │ Id (PK)          │
        │                    │ Name: "read"     │
        │                    │ Name: "write"    │
        │                    │ Name: "delete"   │
        │                    └──────────────────┘
        │
        │ 1..*
    ┌───▼───────────────┐
    │   UserRoles       │
    ├───────────────────┤
    │ UserId (FK) ──────┘
    │ RoleId (FK)
    └───────────────────┘

┌──────────────────────┐
│  RefreshTokens       │
├──────────────────────┤
│ Id (PK)              │
│ Token                │
│ Expires              │
│ IsRevoked            │
│ UserId (FK) ────┐
└──────────────────────┘   Referenced by Users
                        1
```

---

## 8️⃣ Permission Hierarchy

```
┌─────────────────────────────────────────────────┐
│ ADMIN ROLE                                      │
├─────────────────────────────────────────────────┤
│ Permissions: read, write, delete, manage_users │
│ Can:                                            │
│  • Read all data                                │
│  • Write/modify data                            │
│  • Delete data                                  │
│  • Manage users (assign roles, etc)             │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ MANAGER ROLE                                    │
├─────────────────────────────────────────────────┤
│ Permissions: read, write, manage_users         │
│ Can:                                            │
│  • Read all data                                │
│  • Write/modify data                            │
│  • Manage users (assign roles, etc)             │
│ Cannot:                                         │
│  ✗ Delete data                                  │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│ USER ROLE                                       │
├─────────────────────────────────────────────────┤
│ Permissions: read                               │
│ Can:                                            │
│  • Read data                                    │
│ Cannot:                                         │
│  ✗ Write/modify data                            │
│  ✗ Delete data                                  │
│  ✗ Manage users                                 │
└─────────────────────────────────────────────────┘
```

---

## 9️⃣ Security Layers

```
┌──────────────────────────────────────────────────┐
│ Layer 1: Password Security                      │
│ • Input: "MyPassword123"                        │
│ • BCrypt: 12 rounds, automatic salt             │
│ • Output: $2a$12$R9h/cIPz0gi.URNNX3...        │
│ • Storage: Hashed in database (never plain)    │
│ • Result: ✓ Unrecoverable, secure              │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│ Layer 2: Authentication (JWT)                   │
│ • Token Contains: userId, email, roles, perms   │
│ • Signed With: HMAC-SHA256 secret key          │
│ • Expiry: 15 minutes (access) / 7 days (refresh)│
│ • Validation: Signature, expiration, issuer    │
│ • Transport: Authorization header (secure)      │
│ • Result: ✓ Stateless, tamper-proof            │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│ Layer 3: Authorization (RBAC + PBAC)            │
│ • Check 1: [Authorize] attribute present?      │
│ • Check 2: User authenticated?                  │
│ • Check 3: Required role(s)?                    │
│ • Check 4: Required permission(s)?              │
│ • Check 5: Token not revoked?                   │
│ • Result: ✓ 403 Forbidden or ✓ Allowed         │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│ Layer 4: Error Handling                         │
│ • Catch all exceptions globally                 │
│ • Return secure error messages                  │
│ • No stack traces exposed                       │
│ • Log internally (not to client)                │
│ • Result: ✓ Safe, non-revealing errors         │
└──────────────────────────────────────────────────┘
```

---

## 🔟 Complete Request/Response Example

```
REQUEST:
───────
POST /api/auth/login HTTP/1.1
Host: localhost:5000
Content-Type: application/json

{
  "email": "john@example.com",
  "password": "MySecurePass123!"
}

RESPONSE: 200 OK
──────────────
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.
                  eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDEiLCJl
                  bWFpbCI6ImpvaG5AZXhhbXBsZS5jb20iLCJ1c2VybmFtZSI6ImpvaG5fZG9lIiwicm
                  9sZXMiOlsiVXNlciJdLCJwZXJtaXNzaW9ucyI6WyJyZWFkIl0sImlhdCI6MTcwNDA2
                  NzIwMCwiZXhwIjoxNzA0MDY4MTAwfQ.
                  abc123defghijklmnopqrstuvwxyz",
  "refreshToken": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p"
}

JWT DECODED:
────────────
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "550e8400-e29b-41d4-a716-446655440001",
  "email": "john@example.com",
  "username": "john_doe",
  "roles": ["User"],
  "permissions": ["read"],
  "iat": 1704067200,
  "exp": 1704068100
}

Signature: HMACSHA256(
  base64UrlEncode(header) + "." +
  base64UrlEncode(payload),
  "your-secret-key-here"
)

USING TOKEN:
────────────
GET /api/test/protected HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...

RESPONSE: 200 OK
{
  "message": "You are authenticated",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "email": "john@example.com"
}
```

---

**All flows are secure, efficient, and follow industry best practices! 🔐**
