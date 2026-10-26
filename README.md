# Network Security — Role-Based Access Control (RBAC) System

A modern **Role-Based Access Control (RBAC)** implementation built with a **.NET 10** backend, **Vanilla JavaScript** frontend, and **Microsoft SQL Server**, fully containerized using Docker.

---

## Architecture Overview

```
SecurityProject/
├── Backend/              # .NET 10 Web API (Clean Architecture: Domain, App, Infra, Api)
├── Frontend/             # Vanilla JavaScript client (Glassmorphism UI)
├── Dockerfile            # Multi-stage production container build
└── docker-compose.yml    # Service orchestration (App + SQL Server)
```

---

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v20.10+ recommended) installed and running.

---

## Getting Started

### Option 1: Run with Source Code (Local Build)

Clone the repository and build the containers locally:

```bash
# Navigate to project directory
cd SecurityProject

# Build and start services in the foreground
docker-compose up --build
```

### Option 2: Run Without Source Code (Docker Hub)

To distribute or run without cloning the full repository, share only `docker-compose.yml`. Docker pulls the pre-built image (`sirlight35/my-networksecurity-app:v1`) directly from Docker Hub:

```bash
docker-compose up
```

---

## Service Endpoints

Once the containers are up and healthy, access the services:

| Service | URL | Notes |
| :--- | :--- | :--- |
| **Frontend Dashboard** | `http://localhost:5000` | UI with interactive RBAC demo |
| **Swagger API Docs** | `http://localhost:5000/swagger` | Interactive OpenAPI documentation |
| **SQL Server** | `localhost:1433` | Companion database instance |

---

## Core Features

- **Single Container Deployment** — Hosts the .NET Web API and serves static frontend assets from one container.
- **Automated DB Migrations** — SQL Server spins up with automatic schema creation, migrations, and initial seed data.
- **JWT Authentication** — Stateless token authentication featuring short-lived Access Tokens paired with Refresh Tokens.
- **Granular RBAC** — Role assignment at registration with dynamic permission resolution on protected routes.
- **Transient Fault Handling** — Built-in retry policies and health checks ensuring the API waits until the database is ready.

---

## Roles & Access Matrix

| Role | Permissions | Available Actions |
| :--- | :--- | :--- |
| **Admin** | `read`, `write`, `delete`, `manage_users` | Full system access, User Management, Admin Portal |
| **Manager** | `read`, `write` | Create and modify records; no user deletion or administration |
| **User** | `read` | Read-only access across standard endpoints |

---

## Verification & Testing Guide

1. **Register** — Create an account via the frontend and select an initial role (e.g., `User` or `Manager`).
2. **Authenticate** — Log in to receive your JWT token pair.
3. **Inspect Permissions** — View active permissions displayed on the user dashboard.
4. **Test Authorization Boundaries**:
   - Access **General Read** endpoints (allowed for all roles).
   - Trigger **Admin Portal** or **User Deletion** while logged in as `User` or `Manager` $\rightarrow$ verifies `403 Forbidden`.
   - Log in as `Admin` $\rightarrow$ verifies `200 OK` across all system actions.

---

## Troubleshooting

| Issue | Root Cause | Resolution |
| :--- | :--- | :--- |
| `Cannot connect to the Docker daemon` | Docker service is stopped | Start Docker Desktop and verify the daemon is running before executing commands. |
| `SqlException / Connection Refused` | First-boot DB init latency | SQL Server initialization can take 15–30s on cold starts. Let the container health check cycle or run `docker-compose down && docker-compose up`. |
| `Bind for 0.0.0.0:5000 failed` | Port `5000` or `1433` is occupied | Change the host port mapping in `docker-compose.yml` (e.g., `"5050:5000"`). |
