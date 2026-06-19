# Web Application Report
## National Institute for Governance & Sustainable Development
### المعهد القومي للحوكمة والتنمية المستدامة

A role-based web application for managing dynamic reports, built with ASP.NET Core 8 MVC, Entity Framework Core, and ASP.NET Core Identity.

---

## Table of Contents

1. [Requirements](#requirements)
2. [Installation](#installation)
3. [Database Configuration](#database-configuration)
4. [Running the Application](#running-the-application)
5. [Default Login](#default-login)
6. [Roles & Permissions](#roles--permissions)
7. [Features](#features)
8. [Project Structure](#project-structure)
9. [Troubleshooting](#troubleshooting)

---

## Requirements

### Software

| Requirement | Version | Download |
|---|---|---|
| .NET SDK | 8.0 or later | https://dotnet.microsoft.com/download/dotnet/8 |
| SQL Server | 2017 or later | https://www.microsoft.com/en-us/sql-server/sql-server-downloads |
| Visual Studio | 2022 (v17.8+) | https://visualstudio.microsoft.com/ |
| Visual Studio Workload | **ASP.NET and web development** | Select during VS install |

> **Note:** SQL Server Express is free and sufficient for development.

### NuGet Packages (auto-restored)

The following packages are listed in the `.csproj` and will be restored automatically on build:

- `Microsoft.EntityFrameworkCore.SqlServer` 8.x
- `Microsoft.EntityFrameworkCore.Tools` 8.x
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.x
- `ReportViewerCore.NETCore` 15.1.33
- `Microsoft.Windows.Compatibility` 10.x
- `System.Drawing.Common` 10.x

---

## Installation

### 1. Install .NET 8 SDK

Download and install from:
```
https://dotnet.microsoft.com/download/dotnet/8
```

Verify the installation:
```bash
dotnet --version
# Should print: 8.x.x
```

### 2. Install SQL Server

- **Development:** Download SQL Server Express (free)
- **Production:** SQL Server 2017 or later (Standard / Enterprise)

Make sure SQL Server is running and TCP/IP is enabled on port 1433.

### 3. Open the Project

Open `WebApplicationReport.sln` in Visual Studio 2022, or restore from the CLI:
```bash
cd WebApplicationReport
dotnet restore
```

---

## Database Configuration

Open `WebApplicationReport/appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=WebApplicationReportDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

### Connection String Parameters

| Parameter | Description | Example |
|---|---|---|
| `Server` | SQL Server host name or IP | `192.168.3.35` / `localhost` / `.\SQLEXPRESS` |
| `Database` | Database name (created automatically on first run) | `WebApplicationReportDb` |
| `User Id` | SQL Server login username | `sa` |
| `Password` | SQL Server login password | `P@ssw0rd` |
| `TrustServerCertificate` | Skip SSL certificate check (recommended for local/LAN) | `True` |

### Windows Authentication (alternative to SQL login)

```json
"DefaultConnection": "Server=localhost;Database=WebApplicationReportDb;Trusted_Connection=True;TrustServerCertificate=True"
```

### Apply Database Migrations

The application **automatically applies migrations and seeds data** on first startup.

To apply migrations manually:

**Package Manager Console (Visual Studio):**
```powershell
Update-Database
```

**.NET CLI:**
```bash
cd WebApplicationReport
dotnet ef database update
```

> All tables, roles (Admin, Editor, DataEntry), and the default admin user are created automatically.

---

## Running the Application

### From Visual Studio

Press **F5** or click the **Run** button (IIS Express or Kestrel profile).

### From the CLI

```bash
cd WebApplicationReport
dotnet run
```

Open your browser at the URL shown in the terminal (e.g. `https://localhost:7xxx`).

---

## Default Login

A default administrator account is created automatically on first run:

| Field | Value |
|---|---|
| Email | `admin@admin.com` |
| Password | `Admin@123` |

> **Important:** Change this password after first login in a production environment.

---

## Roles & Permissions

| Action | Admin | DataEntry | Editor |
|---|---|---|---|
| View reports list | Yes | Yes | Yes |
| View report entries | Yes | Yes | Yes |
| Create report (define structure) | Yes | No | No |
| Add entry | Yes | Yes | No |
| Edit entry | Yes | No | Yes |
| Delete entry (soft) | Yes | No | Yes |
| PDF viewer / Export PDF | Yes | Yes | No |
| Export Excel | Yes | Yes | No |
| User Management | Yes | No | No |
| Delete / Restore users (soft) | Yes | No | No |

### Important Notes

- **Soft Delete** — Users and entries are flagged as deleted, not removed from the database.
- **Deleted users cannot log in**, even with the correct password.
- Deleted users can be **restored** by an Admin from the Users Management page.

---

## Features

- **Dynamic Reports** — Admin creates reports with custom fields: Text, Number, Date, Dropdown.
- **RDLC Report Viewer** — Embedded PDF viewer rendered dynamically from report data with the institute logo.
- **PDF & Excel Export** — One-click export using `ReportViewerCore.NETCore`.
- **Soft Delete** — Users and report entries are never permanently removed.
- **Localization** — Full Arabic (RTL) and English support, switchable via a language button in the navbar.
- **Role-Based Access** — Every page and action is protected by role authorization at both the controller and view level.
- **Custom Sign-In** — Deleted users are blocked at the `SignInManager` level.

---

## Project Structure

```
WebApplicationReport/
├── Areas/
│   └── Identity/Pages/Account/   # Login, Logout, Register (customized)
├── Controllers/
│   ├── AdminController.cs         # User management (CRUD + soft delete)
│   ├── ReportController.cs        # Report & entry management
│   ├── HomeController.cs
│   └── LanguageController.cs      # Language switcher (EN / AR)
├── Data/
│   ├── ApplicationDbContext.cs    # EF Core DbContext (IdentityDbContext<ApplicationUser>)
│   ├── DbInitializer.cs           # Seeds roles and default admin on startup
│   └── Migrations/                # EF Core migration files
├── Models/
│   ├── Domain/
│   │   ├── ApplicationUser.cs     # Extends IdentityUser with IsDeleted
│   │   ├── Report.cs
│   │   ├── ReportField.cs
│   │   ├── ReportEntry.cs         # Includes IsDeleted for soft delete
│   │   ├── ReportEntryValue.cs
│   │   └── FieldType.cs           # Enum: Text, Number, Date, Dropdown
│   └── ViewModels/                # Form models for controllers
├── Resources/                     # .resx localization files (EN + AR)
├── Services/
│   ├── ApplicationSignInManager.cs  # Blocks deleted users from logging in
│   ├── RdlcService.cs               # Renders PDF / Excel using LocalReport
│   └── RdlcReportBuilder.cs         # Dynamically generates RDLC XML
├── Views/
│   ├── Admin/         # Users, AddUser, EditUser, AssignRole
│   ├── Report/        # Index, Create, DetailsEntry, AddEntry, EditEntry
│   ├── Home/          # Landing page with navigation cards
│   └── Shared/        # _Layout, _LoginPartial
└── wwwroot/
    └── images/
        └── logo.png   # Institute logo (place your file here)
```

---

## Logo Setup

Place the institute logo at:
```
WebApplicationReport/wwwroot/images/logo.png
```

The logo appears in:
- Navbar (top-left corner)
- Home page hero section
- PDF/Excel report header (embedded as base64 in RDLC)

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Cannot connect to SQL Server | Verify the `Server` value in `appsettings.json`. Confirm SQL Server is running and port 1433 is accessible. |
| `Login failed for user 'sa'` | Enable SQL Server Authentication mode and verify the password. |
| `TrustServerCertificate` error | Add `TrustServerCertificate=True` to the connection string. |
| PDF/Excel export fails | The RDLC renderer requires Windows OS. Ensure `Microsoft.Windows.Compatibility` package is installed. |
| Logo not showing | Copy your logo PNG to `wwwroot/images/logo.png`. |
| Migration errors on startup | Run `dotnet ef database update` from inside the `WebApplicationReport/` folder. |
| User cannot log in | The account may be soft-deleted. An Admin can restore it from the Users Management page. |
| `dotnet ef` command not found | Install the EF tools: `dotnet tool install --global dotnet-ef` |
