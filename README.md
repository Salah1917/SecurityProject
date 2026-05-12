# Network Security — RBAC System
 
A production-ready **Role-Based Access Control (RBAC)** system demonstrating modern security architecture. Built with a **.NET 10** Clean Architecture backend, a **Vanilla JavaScript** Glassmorphism frontend, and **SQL Server** — all orchestrated with Docker Compose for a one-command startup experience.
 
The system showcases JWT-based authentication with access/refresh token rotation, dynamic permission enforcement at the API layer, and a fully automated database setup with migrations and seed data.
 
---
 
## Tech Stack
 
| Layer | Technology |
| :--- | :--- |
| Backend | .NET 10 Web API — Clean Architecture |
| Frontend | Vanilla JavaScript, HTML/CSS (Glassmorphism UI) |
| Database | SQL Server (containerized) |
| Auth | JWT — Access & Refresh Tokens |
| Infrastructure | Docker, Docker Compose |
| API Docs | Swagger / OpenAPI |
 
---
 
## Prerequisites
 
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
- No .NET SDK, Node.js, or SQL Server installation required — everything runs inside containers.
---
 
## Quick Start
 
Clone or download the project, then run a single command from the project root:
 
```powershell
cd SecurityProject
docker-compose up --build
```
 
Docker will build the application image, start the SQL Server container, wait for it to become healthy, apply database migrations, seed the initial data, and serve the app — all automatically.
 
Once everything is up, open your browser:
 
| Service | URL |
| :--- | :--- |
| Frontend | `http://localhost:5000` |
| Swagger API Docs | `http://localhost:5000/swagger` |
 
> **First run note:** SQL Server can take 15–30 seconds to initialize. The backend is configured to detect this and retry automatically, so no intervention is needed.
 
---
 
## Sharing Without Source Code
 
You don't need to send the full codebase to let someone run this project. Just share the `docker-compose.yml` file — Docker Hub hosts the pre-built image (`sirlight35/my-networksecurity-app:v1`) and it will be pulled automatically.
 
**Steps for the recipient:**
 
1. Place `docker-compose.yml` in an empty folder.
2. Open a terminal in that folder and run:
```powershell
docker-compose up
```
 
3. Visit `http://localhost:5000` — that's it.
---
 
## Features
 
### 🔐 JWT Authentication
Secure login flow using short-lived **Access Tokens** and longer-lived **Refresh Tokens**. The frontend handles token storage and automatic refresh, keeping sessions alive without re-authentication.
 
### 🛡️ Dynamic Role-Based Access Control
Permissions are enforced at the API layer, not just the UI. Each protected endpoint validates the caller's role and returns a proper `403 Forbidden` if access is denied. Role assignments are made at registration and reflected in real time on the dashboard.
 
### 🗄️ Automated Database Management
SQL Server runs in its own container. On startup, EF Core migrations are applied and the database is seeded with roles and permissions automatically — no manual setup required.
 
### 🐳 Single Unified Container
The Dockerfile uses a multi-stage build to compile the .NET API and bundle the static frontend assets into one lean production image. No separate web server needed.
 
### ♻️ Transient Resiliency
The backend uses retry logic with exponential backoff to handle the race condition between the app container and the SQL Server container becoming ready. Startup is fully automated and self-healing.
 
---
 
## Roles & Permissions
 
The database is pre-seeded with three roles and their associated permissions:
 
| Role | `read` | `write` | `delete` | `manage_users` |
| :--- | :---: | :---: | :---: | :---: |
| **Admin** | ✅ | ✅ | ✅ | ✅ |
| **Manager** | ✅ | ✅ | ❌ | ❌ |
| **User** | ✅ | ❌ | ❌ | ❌ |
 
Roles are selected at registration time and cannot be changed from the UI — this is intentional for demonstration purposes, keeping the focus on permission enforcement rather than role management.
 
---
 
## Project Structure
 
```
SecurityProject/
├── Backend/
│   ├── Domain/              # Entities, enums, domain logic
│   ├── Application/         # Use cases, interfaces, DTOs
│   ├── Infrastructure/      # EF Core, repositories, JWT, DB seeding
│   └── Api/                 # Controllers, middleware, Swagger config
│
├── Frontend/
│   ├── index.html           # Entry point
│   ├── app.js               # Auth flow, API calls, dashboard logic
│   └── styles.css           # Glassmorphism UI styles
│
├── Dockerfile               # Multi-stage production build
└── docker-compose.yml       # Orchestrates App + SQL Server containers
```
 
---
 
## Testing the System
 
### 1. Register
Navigate to `http://localhost:5000` and create a new account. Select a role from the dropdown — try **Manager** for a mid-tier permission set.
 
### 2. Login
Sign in with your new credentials. The app will receive a JWT and store it for subsequent API calls.
 
### 3. Explore the Dashboard
Your assigned permissions are displayed on the dashboard. Use the **System Actions** buttons to call protected API endpoints and observe the results:
 
| Action | Admin | Manager | User |
| :--- | :---: | :---: | :---: |
| Read Data | ✅ | ✅ | ✅ |
| Write Data | ✅ | ✅ | ❌ `403` |
| Delete Data | ✅ | ❌ `403` | ❌ `403` |
| Admin Portal | ✅ | ❌ `403` | ❌ `403` |
 
### 4. Inspect the API
Visit `http://localhost:5000/swagger` to explore and test all endpoints directly, including authorization flows.
 
---
 
## Troubleshooting
 
| Symptom | Cause | Solution |
| :--- | :--- | :--- |
| `Docker daemon is not running` | Docker Desktop isn't started | Launch Docker Desktop and wait for it to fully initialize before running `docker-compose`. |
| App crashes immediately on first run | SQL Server isn't ready yet | Run `docker-compose up` again — the retry logic will handle it on the second attempt. |
| `bind: address already in use` on port `5000` | Another process is using the port | Change the host port in `docker-compose.yml`, e.g. `"5001:5000"`. |
| `bind: address already in use` on port `1433` | A local SQL Server instance is running | Stop the local instance or remap the DB port in `docker-compose.yml`. |
| Frontend shows blank page | Browser cache issue | Hard refresh with `Ctrl+Shift+R` (or `Cmd+Shift+R` on Mac). |
