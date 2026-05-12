# Network Security — RBAC System
 
A modern **Role-Based Access Control (RBAC)** system built with a .NET 10 backend, Vanilla JavaScript frontend, and SQL Server — fully containerized with Docker.
 
---
 
## Prerequisites
 
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
---
 
## Quick Start
 
```powershell
cd SecurityProject
docker-compose up --build
```
 
Once running, open your browser:
 
| Service | URL |
| :--- | :--- |
| Frontend | `http://localhost:5000` |
| Swagger API Docs | `http://localhost:5000/swagger` |
 
---
 
## Sharing Without Source Code
 
Just send someone the `docker-compose.yml` file. They can run the full stack without any source code — Docker will pull the pre-built image from Docker Hub (`sirlight35/my-networksecurity-app:v1`) automatically.
 
```powershell
docker-compose up
```
 
---
 
## Features
 
- **Unified Container** — A single Docker image serves both the .NET API and the static frontend.
- **Automated Database** — SQL Server runs in a companion container with automatic migrations and seeding.
- **JWT Authentication** — Secure token-based auth with Access and Refresh tokens.
- **Dynamic RBAC** — Choose your role at registration and see permissions update in real time.
- **Transient Resiliency** — Backend waits for the database and retries connections automatically.
---
 
## Roles & Permissions
 
| Role | Permissions |
| :--- | :--- |
| **Admin** | `read`, `write`, `delete`, `manage_users` |
| **Manager** | `read`, `write` |
| **User** | `read` |
 
---
 
## Project Structure
 
```
SecurityProject/
├── Backend/          # .NET 10 Web API — Clean Architecture (Domain, Application, Infrastructure, Api)
├── Frontend/         # Vanilla JS frontend with Glassmorphism UI
├── Dockerfile        # Multi-stage production build
└── docker-compose.yml
```
 
---
 
## Testing the System
 
1. **Register** — Create an account and pick a role (e.g., Manager).
2. **Login** — Sign in with your credentials.
3. **Dashboard** — View your permissions and try the System Actions buttons.
   - A **User** hitting "Admin Portal" will receive `403 Forbidden`.
   - An **Admin** can call all actions successfully.
---
 
## Troubleshooting
 
| Issue | Fix |
| :--- | :--- |
| Docker daemon error | Make sure Docker Desktop is running before executing any `docker-compose` command. |
| Database not ready on first run | The app uses health checks to wait for SQL Server. If it fails, re-run `docker-compose up`. |
| Port conflict on `5000` or `1433` | Edit the port mappings in `docker-compose.yml`. |
