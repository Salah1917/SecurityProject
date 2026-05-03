# Quick Start Guide 🚀

## 1️⃣ Setup Database

### Step 1: Configure Connection String
Edit `Api\appsettings.json`:
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

### Step 2: Apply Migrations
```powershell
# From Visual Studio Package Manager Console or Terminal
cd Infrastructure
dotnet ef database update
```

This creates:
- ✅ All database tables
- ✅ Seeds 3 roles: Admin, Manager, User
- ✅ Seeds 4 permissions: read, write, delete, manage_users
- ✅ Maps permissions to roles

---

## 2️⃣ Run the API

```powershell
cd Api
dotnet run
```

API is now running at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

Swagger/OpenAPI docs: `http://localhost:5000/swagger`

---

## 3️⃣ Test the System

### Using REST Client in Visual Studio:

1. Open `Api\ApiTesting.http`
2. Register a new user:
   ```http
   POST http://localhost:5000/api/auth/register
   Content-Type: application/json

   {
     "username": "admin_user",
     "email": "admin@test.com",
     "password": "SecurePassword123!"
   }
   ```
   Copy the `accessToken` from response

3. Test protected endpoint:
   ```http
   GET http://localhost:5000/api/test/protected
   Authorization: Bearer <PASTE_TOKEN_HERE>
   ```

4. Get current user info:
   ```http
   GET http://localhost:5000/api/test/me
   Authorization: Bearer <PASTE_TOKEN_HERE>
   ```

---

## 📊 Understanding the Response

### Login/Register Response:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI...",
  "refreshToken": "a1b2c3d4-e5f6-4g7h-8i9j-0k1l2m3n4o5p"
}
```

- **accessToken**: Use this in `Authorization: Bearer <token>` header (expires in 15 min)
- **refreshToken**: Use to get new tokens when expired (expires in 7 days)

### User Info Response:
```json
{
  "userId": "uuid",
  "email": "admin@test.com",
  "username": "admin_user",
  "roles": ["User"],  // Initially everyone gets User role
  "permissions": ["read"]  // User role has read permission
}
```

---

## 🔑 Key Roles & Permissions

### Default Setup After Migration:

```
Admin Role
├── read
├── write
├── delete
└── manage_users

Manager Role
├── read
├── write
└── manage_users

User Role
└── read
```

---

## 🛠️ Assign Roles (Admin Only)

```http
POST http://localhost:5000/api/admin/assign-role
Authorization: Bearer <ADMIN_TOKEN>
Content-Type: application/json

{
  "userId": "user-uuid",
  "roleId": "11111111-1111-1111-1111-111111111111"  // Admin role UUID
}
```

**Role UUIDs:**
- Admin: `11111111-1111-1111-1111-111111111111`
- User: `22222222-2222-2222-2222-222222222222`
- Manager: `33333333-3333-3333-3333-333333333333`

---

## ✅ Test Different Permission Levels

### For Admin (has all permissions):
```http
POST http://localhost:5000/api/test/write-data
Authorization: Bearer <ADMIN_TOKEN>
→ ✅ Success (has write permission)

DELETE http://localhost:5000/api/test/delete-data/1
Authorization: Bearer <ADMIN_TOKEN>
→ ✅ Success (has delete permission)
```

### For User (only has read):
```http
GET http://localhost:5000/api/test/read-data
Authorization: Bearer <USER_TOKEN>
→ ✅ Success (has read permission)

POST http://localhost:5000/api/test/write-data
Authorization: Bearer <USER_TOKEN>
→ ❌ 403 Forbidden (no write permission)

DELETE http://localhost:5000/api/test/delete-data/1
Authorization: Bearer <USER_TOKEN>
→ ❌ 403 Forbidden (no delete permission)
```

---

## 🔄 Refresh Token Flow

### When Access Token Expires:

```http
POST http://localhost:5000/api/auth/refresh?refreshToken=<YOUR_REFRESH_TOKEN>
```

Response:
```json
{
  "accessToken": "new-token-valid-15-min",
  "refreshToken": "new-refresh-token-valid-7-days"
}
```

**Note:** Old refresh token is automatically revoked

---

## 🐛 Troubleshooting

### Issue: "Database connection failed"
**Solution:** Check `ConnectionString` in `appsettings.json`
- Verify SQL Server is running
- Check server name (`.` = local)
- Ensure database exists or use `Trusted_Connection=true`

### Issue: "Invalid refresh token"
**Solution:**
- Use refresh token from latest login
- Don't reuse old refresh tokens (they get revoked)
- Check token hasn't expired (7 days max)

### Issue: "401 Unauthorized"
**Solution:**
- Token is missing or malformed
- Check `Authorization: Bearer <token>` format
- Ensure header name is exactly "Authorization"
- Verify token hasn't expired

### Issue: "403 Forbidden"
**Solution:**
- User doesn't have required permission/role
- Use admin endpoints with admin account
- Check endpoint's `[Authorize(Policy = "...")]` requirement

---

## 📚 Architecture Overview

```
┌─────────────────┐
│  REST Client    │
│  (Postman, etc) │
└────────┬────────┘
         │
    POST /auth/login
         │
    ┌────▼────────────────┐
    │   API Controllers   │
    │  [AuthController]   │
    │  [TestController]   │
    │  [AdminController]  │
    └────┬────────────────┘
         │
    ┌────▼────────────────────┐
    │  Application Services   │
    │  [AuthService]          │
    │  [JwtService]           │
    │  [PasswordHasher]       │
    └────┬────────────────────┘
         │
    ┌────▼────────────────────┐
    │  Infrastructure         │
    │  [UserRepository]       │
    │  [AppDbContext]         │
    └────┬────────────────────┘
         │
    ┌────▼────────────────────┐
    │  SQL Server Database    │
    │  [Users Table]          │
    │  [Roles Table]          │
    │  [Permissions Table]    │
    │  [UserRoles Table]      │
    │  [RefreshTokens Table]  │
    └─────────────────────────┘
```

---

## ✨ Features Implemented

✅ **Password Security**
- BCrypt hashing with salt
- Secure password verification
- Protected against brute-force

✅ **Authentication**
- JWT tokens (15 min expiry)
- Refresh tokens (7 day expiry)
- Token revocation

✅ **Authorization**
- Role-based access control (RBAC)
- Permission-based access control (PBAC)
- Multi-level security

✅ **User Management**
- Register new users
- Login with credentials
- Assign/remove roles
- View user permissions

✅ **Admin Features**
- Assign roles to users
- Manage permissions
- View all users
- View all roles

---

## 🎯 What's Working

✅ All password security requirements  
✅ All authentication mechanisms  
✅ All authorization/RBAC requirements  
✅ Permission management system  
✅ Error handling  
✅ Global exception middleware  

---

## 📞 Need Help?

1. Read `DOCUMENTATION.md` for detailed explanations
2. Check `Api\ApiTesting.http` for all endpoint examples
3. Review database schema in migrations
4. Check error messages in API responses
5. Verify `appsettings.json` configuration

Happy coding! 🚀
