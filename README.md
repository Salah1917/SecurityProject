# Network Security - RBAC System

A modern Role-Based Access Control (RBAC) system built with a **.NET 10** backend, **Vanilla JavaScript** frontend, and **SQL Server**, all containerized using Docker.

## 🚀 Quick Start (Docker)

This project is fully containerized. You only need Docker Desktop installed to run the entire stack.

### Prerequisites
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

### Installation
1.  Open your terminal (PowerShell, CMD, or Bash).
2.  Navigate to the project root directory:
    ```powershell
    cd SecurityProject
    ```
3.  Run the following command to build and start the system:
    ```powershell
    docker-compose up --build
    ```

## 🌍 Sharing with Others (No Source Code Needed)

If you want to share this project with someone without sending them all the source code, you can just send them the `docker-compose.yml` file:

1.  **Send them `docker-compose.yml`**.
2.  They put it in a folder and run:
    ```powershell
    docker-compose up
    ```
3.  Docker will automatically pull the pre-built image from **Docker Hub** (`sirlight35/my-networksecurity-app:v1`).

### Accessing the App
*   **Frontend**: `http://localhost:5000`
*   **Swagger API Documentation**: `http://localhost:5000/swagger`

---

## 🛠 Features

*   **Unified Container**: A single Docker image serves both the .NET API and the static Frontend.
*   **Automated Database**: SQL Server runs in a companion container with automatic migrations and seeding.
*   **JWT Authentication**: Secure token-based auth with Access and Refresh tokens.
*   **Dynamic RBAC**: Choose your role during registration (Admin, Manager, User) and see your permissions update in real-time.
*   **Transient Resiliency**: Backend is configured to wait for the database and retry connections automatically.

---

## 🔐 Roles & Permissions

The system comes pre-seeded with the following access levels:

| Role | Permissions |
| :--- | :--- |
| **Admin** | `read`, `write`, `delete`, `manage_users` |
| **Manager** | `read`, `write` |
| **User** | `read` |

---

## 📂 Project Structure

*   `/Backend`: .NET 10 Web API with Clean Architecture (Domain, Application, Infrastructure, Api).
*   `/Frontend`: Modern Vanilla JS frontend with Glassmorphism UI.
*   `Dockerfile`: Multi-stage build for production-ready deployment.
*   `docker-compose.yml`: Orchestrates the App and SQL Server containers.

---

## 🧪 Testing the System

1.  **Register**: Create a new account and select a role (e.g., **Manager**).
2.  **Login**: Sign in with your credentials.
3.  **Dashboard**: View your assigned permissions and try calling the **System Actions** buttons.
    *   If you are a **User**, the "Admin Portal" action will return a `403 Forbidden` status.
    *   If you are an **Admin**, all actions will succeed.

---

## 📝 Troubleshooting

*   **Docker Daemon Error**: Ensure Docker Desktop is running before executing `docker-compose`.
*   **Database Not Ready**: The app uses health checks to wait for SQL Server. If the first run fails, simply run `docker-compose up` again.
*   **Port Conflicts**: If port `5000` or `1433` is already in use, you can change them in `docker-compose.yml`.
