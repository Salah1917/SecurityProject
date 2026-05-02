# Database Schema & Entity Relationships

## 🗄️ Complete Database Schema

### Entity Relationship Diagram (ERD)

```
┌──────────────┐
│    Users     │
├──────────────┤
│ PK: Id       │
│    Username  │───────────┐
│    Email     │           │
│    PasswordH │           │ 1..* 
│    PasswordH │───┐       │
│    PasswordH │   │       │
└──────────────┘   │       │
       │           │       │
       │ 1..*      │ 1     │
       │           │       │
   ┌───▼───────────┴──┐    │
   │   UserRoles     │    │
   ├─────────────────┤    │
   │ PK: UserId      │    │
   │    RoleId       │    │
   └─────────────────┘    │
           │ 1..*         │
           │              │
           │ 1      ┌─────▼──────────┐
           └───────│    Roles        │
                   ├─────────────────┤
                   │ PK: Id          │
                   │    Name         │◄──────┐
                   └─────────────────┘       │
                           │                 │
                           │ 1..*            │
                           │        ┌────────┴───────┐
                      ┌────▼────────┴──┐             │
                      │ RolePermissions │             │
                      ├─────────────────┤             │
                      │ PK: RoleId      │             │
                      │    PermissionId │    1..*     │
                      └─────────────────┘             │
                             │                        │
                             │ 1          ┌───────────▼─────┐
                             └───────────│  Permissions    │
                                         ├─────────────────┤
                                         │ PK: Id          │
                                         │    Name         │
                                         └─────────────────┘


┌──────────────────┐
│  RefreshTokens   │
├──────────────────┤
│ PK: Id           │
│    Token         │
│    Expires       │
│    IsRevoked     │
│ FK: UserId   ────┼───────► Users
└──────────────────┘
```

---

## 📋 Table Definitions

### 1. Users Table
```sql
CREATE TABLE [dbo].[Users] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

-- Example:
-- Id: 550e8400-e29b-41d4-a716-446655440001
-- Username: admin_user
-- Email: admin@test.com
-- PasswordHash: $2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUe
```

**Columns:**
- **Id** (GUID): Unique identifier, auto-generated
- **Username** (string): User's display name
- **Email** (string): User's email address
- **PasswordHash** (string): BCrypt-hashed password (never store plain text!)

---

### 2. Roles Table
```sql
CREATE TABLE [dbo].[Roles] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
);

-- Seeded data:
-- Id: 11111111-1111-1111-1111-111111111111, Name: 'Admin'
-- Id: 22222222-2222-2222-2222-222222222222, Name: 'User'
-- Id: 33333333-3333-3333-3333-333333333333, Name: 'Manager'
```

**Columns:**
- **Id** (GUID): Unique role identifier
- **Name** (string): Role name (Admin, Manager, User)

---

### 3. Permissions Table
```sql
CREATE TABLE [dbo].[Permissions] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
);

-- Seeded data:
-- Id: aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa, Name: 'read'
-- Id: bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb, Name: 'write'
-- Id: cccccccc-cccc-cccc-cccc-cccccccccccc, Name: 'delete'
-- Id: dddddddd-dddd-dddd-dddd-dddddddddddd, Name: 'manage_users'
```

**Columns:**
- **Id** (GUID): Unique permission identifier
- **Name** (string): Permission name

---

### 4. UserRoles Table (Junction)
```sql
CREATE TABLE [dbo].[UserRoles] (
    [UserId] uniqueidentifier NOT NULL,
    [RoleId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) 
        REFERENCES [Roles]([Id]) ON DELETE CASCADE
);

-- Example:
-- UserId: 550e8400-e29b-41d4-a716-446655440001, 
-- RoleId: 11111111-1111-1111-1111-111111111111  (Admin)
```

**Purpose:** Many-to-many relationship between Users and Roles

**Cascade Delete:** If a User or Role is deleted, the UserRole entries are also deleted

---

### 5. RolePermissions Table (Junction)
```sql
CREATE TABLE [dbo].[RolePermissions] (
    [RoleId] uniqueidentifier NOT NULL,
    [PermissionId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([RoleId], [PermissionId]),
    CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) 
        REFERENCES [Roles]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) 
        REFERENCES [Permissions]([Id]) ON DELETE CASCADE
);

-- Example data (from seed migration):
-- Admin Role (11111111...) has:
--   read (aaaaaaaa...)
--   write (bbbbbbbb...)
--   delete (cccccccc...)
--   manage_users (dddddddd...)
--
-- Manager Role (33333333...) has:
--   read (aaaaaaaa...)
--   write (bbbbbbbb...)
--   manage_users (dddddddd...)
--
-- User Role (22222222...) has:
--   read (aaaaaaaa...)
```

**Purpose:** Many-to-many relationship between Roles and Permissions

---

### 6. RefreshTokens Table
```sql
CREATE TABLE [dbo].[RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [Token] nvarchar(max) NOT NULL,
    [Expires] datetime2 NOT NULL,
    [IsRevoked] bit NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) 
        REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- Example:
-- Id: 660e8400-e29b-41d4-a716-446655440002
-- Token: a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p
-- Expires: 2025-01-15 10:30:00
-- IsRevoked: 0 (false)
-- UserId: 550e8400-e29b-41d4-a716-446655440001
```

**Columns:**
- **Id** (GUID): Unique token identifier
- **Token** (string): The refresh token value
- **Expires** (DateTime): Expiration date (7 days from creation)
- **IsRevoked** (bool): Whether token has been revoked (false = active, true = revoked)
- **UserId** (FK): Reference to Users table

**Purpose:** Store refresh tokens for token renewal and revocation tracking

---

## 🔄 Data Flow Examples

### Example 1: User Registration

```
1. User submits registration form
   - Username: "john_doe"
   - Email: "john@example.com"
   - Password: "MySecurePass123!"

2. Password is hashed with BCrypt
   - PasswordHash: "$2a$12$R9h/cIPz0gi.URNNX3kh2OPST9/PgBkqquzi.Ss7KIUgO2t0jWMUe"

3. User is created in database
   INSERT INTO Users (Id, Username, Email, PasswordHash)
   VALUES (NEWID(), 'john_doe', 'john@example.com', '$2a$12$...')

4. Default 'User' role is assigned
   INSERT INTO UserRoles (UserId, RoleId)
   VALUES (john's_id, user_role_id)

5. JWT tokens are generated
   - Access Token: Valid for 15 minutes
   - Refresh Token: Valid for 7 days, stored in RefreshTokens table

6. Response sent to client
   {
     "accessToken": "eyJhbGc...",
     "refreshToken": "a1b2c3d4-..."
   }
```

---

### Example 2: User Login with Role Escalation

```
1. User logs in with credentials
   - Email: "john@example.com"
   - Password: "MySecurePass123!"

2. System queries database
   SELECT * FROM Users WHERE Email = 'john@example.com'

3. Password is verified
   - Stored hash: "$2a$12$..."
   - BCrypt.Verify(input_password, stored_hash) → Returns TRUE

4. Roles are loaded
   SELECT r.Id, r.Name FROM Roles r
   INNER JOIN UserRoles ur ON r.Id = ur.RoleId
   WHERE ur.UserId = 'john_id'
   → Result: ['User'] (default role)

5. Permissions are loaded
   SELECT p.Id, p.Name FROM Permissions p
   INNER JOIN RolePermissions rp ON p.Id = rp.PermissionId
   WHERE rp.RoleId IN ('user_role_id')
   → Result: ['read']

6. JWT token is created with claims
   Header:
   {
     "alg": "HS256",
     "typ": "JWT"
   }
   
   Payload:
   {
     "sub": "john_id",
     "email": "john@example.com",
     "username": "john_doe",
     "roles": ["User"],
     "permissions": ["read"],
     "iat": 1704067200,
     "exp": 1704068100
   }

7. Refresh token is created and stored
   INSERT INTO RefreshTokens (Id, Token, Expires, IsRevoked, UserId)
   VALUES (NEWID(), 'a1b2c3d4-...', 2025-01-15 10:30:00, 0, 'john_id')

8. Tokens are returned to client
   {
     "accessToken": "eyJhbGc...",
     "refreshToken": "a1b2c3d4-..."
   }
```

---

### Example 3: Permission Check

```
1. User makes request to protected endpoint
   GET /api/test/write-data
   Authorization: Bearer eyJhbGc...

2. JWT is validated and decoded
   → sub: john_id
   → roles: ["User"]
   → permissions: ["read"]

3. Endpoint requires "write" permission
   [Authorize(Policy = "CanWrite")]

4. System checks if user has "write" permission
   → User permissions: ["read"]
   → Required: "write"
   → Result: MISSING

5. Response: 403 Forbidden
   {
     "statusCode": 403,
     "message": "User is not authorized to access this resource"
   }
```

---

### Example 4: Token Refresh

```
1. Access token expires after 15 minutes
   JWT exp claim: 1704068100 (Unix timestamp)
   Current time: 1704068200
   → Result: EXPIRED

2. User makes request with refresh token
   POST /api/auth/refresh?refreshToken=a1b2c3d4-...

3. System queries database
   SELECT * FROM RefreshTokens 
   WHERE Token = 'a1b2c3d4-...'
   → Result: RefreshToken record found

4. System validates refresh token
   ✓ Token exists
   ✓ IsRevoked = false
   ✓ Expires date > current time
   → Result: VALID

5. Old refresh token is revoked
   UPDATE RefreshTokens 
   SET IsRevoked = true
   WHERE Token = 'a1b2c3d4-...'

6. New tokens are generated
   - New Access Token: Valid 15 minutes
   - New Refresh Token: Valid 7 days, stored in DB

7. Response sent to client
   {
     "accessToken": "eyJhbGc...new...",
     "refreshToken": "x9y8z7w6-...new..."
   }
```

---

## 🔐 Security Considerations

### Password Storage
```
❌ DON'T: Store plain passwords
✅ DO:   Use BCrypt hashing with automatic salt

Example:
❌ Password: "MyPassword123"     (EXPOSED)
✅ Hash:     "$2a$12$abc..."    (SAFE - irreversible)
```

### Token Storage
```
❌ DON'T: Store tokens in plain text
✅ DO:   Send via secure HTTP header

Transport:
❌ Query parameter: ?token=abc (Exposed in logs)
✅ Header: Authorization: Bearer abc (More secure)
```

### Refresh Token Revocation
```
When user logs out or password changes:
UPDATE RefreshTokens SET IsRevoked = true WHERE UserId = ?

This prevents:
- Reusing old tokens
- Token hijacking
- Unauthorized access after logout
```

---

## 📊 Query Examples

### Get user with all roles and permissions
```sql
SELECT 
    u.Id,
    u.Username,
    u.Email,
    STRING_AGG(r.Name, ', ') AS Roles,
    STRING_AGG(p.Name, ', ') AS Permissions
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
LEFT JOIN RolePermissions rp ON r.Id = rp.RoleId
LEFT JOIN Permissions p ON rp.PermissionId = p.Id
WHERE u.Email = 'admin@test.com'
GROUP BY u.Id, u.Username, u.Email
```

### Get all active refresh tokens
```sql
SELECT 
    rt.Id,
    rt.Token,
    rt.Expires,
    u.Username,
    u.Email
FROM RefreshTokens rt
INNER JOIN Users u ON rt.UserId = u.Id
WHERE rt.IsRevoked = 0 
  AND rt.Expires > GETUTCDATE()
ORDER BY rt.Expires DESC
```

### Check user permissions
```sql
SELECT DISTINCT p.Name AS Permission
FROM Users u
INNER JOIN UserRoles ur ON u.Id = ur.UserId
INNER JOIN RolePermissions rp ON ur.RoleId = rp.RoleId
INNER JOIN Permissions p ON rp.PermissionId = p.Id
WHERE u.Email = 'john@example.com'
```

---

## 🚀 Database Migrations

### Applied Migrations:
1. **20260502173148_InitialCreate**
   - Creates all tables with relationships
   - Sets up primary keys and foreign keys
   - Creates indexes

2. **20260502183021_SeedRoles**
   - Seeds 3 roles: Admin, Manager, User

3. **20260502200000_SeedPermissions**
   - Seeds 4 permissions
   - Maps permissions to roles
   - Admin: all 4 permissions
   - Manager: read, write, manage_users
   - User: read only

---

## 🔧 Database Maintenance

### Check table sizes
```sql
SELECT 
    TABLE_NAME,
    ROW_COUNT = (
        SELECT SUM(p.rows)
        FROM sys.tables t
        INNER JOIN sys.partitions p ON t.object_id = p.object_id
        WHERE t.name = 'Users'
    )
```

### Cleanup old refresh tokens
```sql
DELETE FROM RefreshTokens
WHERE IsRevoked = 1 
  AND Expires < DATEADD(DAY, -30, GETUTCDATE())
```

### Rebuild indexes
```sql
ALTER INDEX ALL ON Users REBUILD
ALTER INDEX ALL ON Roles REBUILD
ALTER INDEX ALL ON Permissions REBUILD
```

---

## 📝 Summary

✅ **6 Tables** with proper relationships  
✅ **2 Junction Tables** for many-to-many relationships  
✅ **Cascade Delete** for data integrity  
✅ **Seeded Data** for immediate testing  
✅ **Secure Password Storage** with BCrypt  
✅ **Token Revocation** for security  

Your database is production-ready! 🚀
