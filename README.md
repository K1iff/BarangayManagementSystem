# Barangay Information Management System (Barangay Langkiwa)

[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen)](#scripts--testing)
[![Platform](https://img.shields.io/badge/platform-ASP.NET%20Core%20MVC-blue)](#tech-stack)
[![Hosting](https://img.shields.io/badge/hosting-MonsterASP.NET-orange)](#tech-stack)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> A modern, web-based information management and automated certificate issuance platform designed to digitize resident records, streamline public document workflows, and enhance administrative accountability for Barangay Langkiwa.

```
+------------------------------------------------------------------------------------+
|                                [ DEMO SCREENSHOT ]                                 |
|                                                                                    |
|       +---------------------+   +---------------------+   +---------------------+  |
|       |  Total Residents    |   |  Pending Requests   |   |  Issued Documents   |  |
|       |        1,250        |   |         14          |   |         432         |  |
|       +---------------------+   +---------------------+   +---------------------+  |
|                                                                                    |
|       +------------------------------------+  +----------------------------------+ |
|       |      Monthly Issuance Trends       |  |     Population by Purok          | |
|       |          (Chart.js Bar)            |  |       (Chart.js Doughnut)        | |
|       +------------------------------------+  +----------------------------------+ |
|                                                                                    |
+------------------------------------------------------------------------------------+
```

---

## 1. Project Overview & Notes
Developed as a transition from manual logbooks and fragmented spreadsheets to a centralized digital solution, the Barangay Langkiwa Information Management System delivers a robust administrative hub for local governance. It features an in-memory data store for self-contained portfolio and showcase demonstrations, with zero external database prerequisites.

### User Roles & Access Hierarchy
* **Barangay Staff (Encoder / Clerk):** Responsible for daily data encoding, registering resident demographic records from walk-ins or paper forms, tracking community members, and processing standard certificate issuance requests.
* **Barangay Secretary / Captain (Admin):** Administrative authorities with privileges to review certificate requests, approve restricted documents, inspect system audit activities, oversee user credentials, and generate operational management reports for Local Government Unit (LGU) submissions.
* **Public Residents (Unauthenticated):** Accessible via the public portal to lodge certificate issuance applications with real-time tracking numbers without needing an account.

---

## 2. Key Features

* **Resident Profiling & Record Management:**
  * Centralized management of resident demographic, contact, and socio-economic data with server-side validation annotations (`Resident.cs`).
  * Real-time resident search, filtering, and server-side pagination for efficient browsing across large datasets (`ResidentsController.cs`).
  * Modal/Detail view displaying household background, civil status, and purok assignment.

* **Automated Certificate Issuance & Word Processing:**
  * Dynamic generation of official documents including **Barangay Clearances**, **Certificates of Indigency**, and **Certificates of Residency**.
  * Server-side OpenXML templating (`DocumentFormat.OpenXml`) utilizing customizable Word (`.docx`) templates in `wwwroot/templates` and outputting to `wwwroot/generated`.
  * In-browser live document preview via client-side `mammoth.js` prior to issuance and approval.
  * Robust relative path resolution with fallback (`IWebHostEnvironment.WebRootPath`) ensuring reliable document rendering on MonsterASP.NET Windows/IIS hosting environments.

* **Public Resident Request Portal:**
  * Dedicated unauthenticated entry point (`/Portal`) allowing constituents to request clearances and residency certifications online.
  * Instant request generation tagged with "Pending" status for staff review and administrative approval.

* **Role-Based Access Control (RBAC) & Secure Sessions:**
  * ASP.NET Core Cookie Authentication and session management (`HttpContext.Session`).
  * Role authorization policies supporting Barangay Staff (`Clerk`), Barangay Secretary, and Barangay Captain roles.
  * Quick-login demo presets on the authentication page (`/Accounts/Login`) for rapid evaluation of permission levels.
  * Password hashing and cryptographic verification powered by `BCrypt.Net-Next`.

* **Interactive Analytics & Reporting:**
  * Executive dashboard featuring visual statistics powered by `Chart.js` (`/Reports` and `/Home`).
  * Aggregated reports categorized by period (weekly, monthly, yearly) and by purok demographic metrics.
  * Export-ready summaries for administrative local governance reporting.

* **Comprehensive Audit Trail & System Activity Logging:**
  * Automatic activity logging (`SystemActivity.cs`) recording critical operations, resident additions, profile modifications, and document approvals to maintain accountability.

---

## 3. Tech Stack

| Category | Technology / Dependency | Version | Purpose |
| :--- | :--- | :--- | :--- |
| **Backend / API** | [ASP.NET Core MVC](https://learn.microsoft.com/aspnet/core) | 8.0 (`net8.0`) | Primary web framework and MVC controller architecture |
| **Backend / API** | [.NET SDK](https://dotnet.microsoft.com/) | 8.0+ | Runtime and build environment |
| **Backend / API** | [DocumentFormat.OpenXml](https://www.nuget.org/packages/DocumentFormat.OpenXml) | 3.4.1 | Server-side Word (.docx) certificate generation and manipulation |
| **Frontend / Native UI** | [Razor Views](https://learn.microsoft.com/aspnet/core/mvc/views/razor) | ASP.NET Core 8.0 | Dynamic server-rendered UI markup |
| **Frontend / Native UI** | [Bootstrap](https://getbootstrap.com/) | 5.3.x | Responsive styling, modern layout grids, and utility classes |
| **Frontend / Native UI** | [Bootstrap Icons](https://icons.getbootstrap.com/) | 1.11.x | UI icon set across navigation, actions, and cards |
| **Frontend / Native UI** | [jQuery](https://jquery.com/) | 3.7.x (incl. slim) | DOM utilities and event handling |
| **Frontend / Native UI** | [jQuery Validation](https://jqueryvalidation.org/) & Unobtrusive | 1.19.x / 3.2.x | Client-side form validation matching C# model annotations |
| **Frontend / Native UI** | [Mammoth.js](https://github.com/mwilliamson/mammoth.js) | 1.6.0 | Client-side .docx preview and HTML rendering engine |
| **Frontend / Native UI** | [Chart.js](https://www.chartjs.org/) | 4.x | Data visualization for resident demographics and issuance trends |
| **Database & Auth** | `InMemoryDataStore` | Built-in C# Data Store | High-performance in-memory repository for self-contained showcase |
| **Database & Auth** | [BCrypt.Net-Next](https://www.nuget.org/packages/BCrypt.Net-Next) | 4.0.3 | Secure password hashing for user management |
| **Database & Auth** | ASP.NET Core Cookie Auth & Sessions | 8.0 | Authentication cookie middleware and active session persistence |
| **Hosting & Tools** | [MonsterASP.NET](https://www.monsterasp.net/) | Windows / IIS | Cloud hosting environment for live showcase deployment |
| **Hosting & Tools** | Visual Studio / `dotnet` CLI | Latest | Project orchestration and solution build management |

---

## 4. Getting Started

### Prerequisites
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or higher) installed on your machine.
* [Git](https://git-scm.com/) installed on your machine.
* Modern web browser (Chrome, Edge, Firefox, Safari).

### Installation & Local Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/K1iff/BarangayLangkiwaSystem.git
   cd BarangayLangkiwaSystem
   ```

2. **Restore Dependencies**
   Restore all required NuGet packages for the solution:
   ```bash
   dotnet restore BarangayLangkiwaSystem.slnx
   ```

3. **Run the Application**
   Launch the project locally using the .NET CLI:
   ```bash
   dotnet run --project BarangayLangkiwaSystem
   ```

4. **Access the System**
   Open your browser and navigate to the local endpoint displayed in the terminal:
   * Application URL: `http://localhost:5082` (or `https://localhost:7062`)
   * Public Portal: `http://localhost:5082/Portal`
   * Staff/Admin Login: `http://localhost:5082/Accounts/Login`

### Demo Credentials
Use the quick-login buttons on the login page or enter the credentials below:
* **Barangay Captain:** `captain@langkiwa.gov.ph` / `Password123`
* **Barangay Secretary:** `secretary@langkiwa.gov.ph` / `Password123`
* **Barangay Clerk / Staff:** `clerk@langkiwa.gov.ph` / `Password123`

---

## 5. Environment Variables

The application is configured using ASP.NET Core's configuration hierarchy (`appsettings.json`, `appsettings.Development.json`, and `launchSettings.json`). No external `.env` file is required for standalone in-memory execution. The following environment variables can be set in your terminal or hosting control panel:

```bash
# ASP.NET Core Runtime Environment ("Development", "Staging", or "Production")
ASPNETCORE_ENVIRONMENT=Development

# Host binding URLs (HTTP & HTTPS)
ASPNETCORE_URLS=http://localhost:5082;https://localhost:7062

# Allowed Hosts for HTTP Host header validation
AllowedHosts=*

# Logging Level Configurations
Logging__LogLevel__Default=Information
Logging__LogLevel__Microsoft.AspNetCore=Warning
```

When deploying to **MonsterASP.NET** or IIS, environment variables can be specified through the hosting dashboard or mapped inside `web.config`.

---

## 6. Project Structure

```text
BarangayLangkiwaSystem/
├── BarangayLangkiwaSystem.slnx        # Modern .NET solution definition file
├── README.md                          # Repository documentation and architecture guide
├── Requirements.md                    # Detailed portfolio and technical specifications
└── BarangayLangkiwaSystem/            # Main ASP.NET Core MVC web application
    ├── BarangayLangkiwaSystem.csproj  # Project manifest and NuGet dependencies
    ├── Program.cs                     # Application bootstrap, DI, and middleware pipeline
    ├── appsettings.json               # Global application configuration
    ├── appsettings.Development.json   # Development environment settings
    ├── .config/                       # Local dotnet tool manifests (e.g. dotnet-ef)
    ├── Properties/
    │   └── launchSettings.json        # Local server profiles and launch URLs
    ├── Controllers/                   # MVC Controllers
    │   ├── AccountsController.cs      # Authentication, login, and logout workflows
    │   ├── BaseController.cs          # Base authorization and session enforcement
    │   ├── CertificatesController.cs  # Document processing, Word export, and approval
    │   ├── HomeController.cs          # Main landing dashboard and error handling
    │   ├── PortalController.cs        # Unauthenticated public request portal
    │   ├── ReportsController.cs       # Analytics generation and statistical exports
    │   ├── ResidentsController.cs     # Resident records CRUD, search, and pagination
    │   └── UserManagementController.cs# Account administration and staff profile edits
    ├── Data/
    │   └── InMemoryDataStore.cs       # Mocked in-memory persistence and seed data
    ├── Models/                        # Domain entities and validation models
    │   ├── CertificateRequest.cs      # Public and internal clearance requests
    │   ├── CertificationLog.cs        # Archival history of issued documents
    │   ├── CertificationType.cs       # Certificate specifications and fees
    │   ├── DashboardViewModel.cs      # Aggregated metrics for home dashboard
    │   ├── ErrorViewModel.cs          # Exception and error display model
    │   ├── Login.cs                   # Authentication credentials model
    │   ├── PurokReportViewModel.cs    # Demographic grouping model
    │   ├── ReportLog.cs               # Audit representation of administrative reports
    │   ├── Resident.cs                # Resident demographic profile model
    │   ├── SystemActivity.cs          # Action-level audit trail entity
    │   └── UserAccounts.cs            # System user representation and roles
    ├── ViewModels/                    # Composite and presentation view models
    │   ├── AllData.cs                 # Aggregated resident and certification view model
    │   ├── CertificateIssueVM.cs      # Document issuance form view model
    │   └── CertificateLogVM.cs        # Certificate tracking and status view model
    ├── Views/                         # Razor views (.cshtml)
    │   ├── Accounts/                  # Login and credentials views
    │   ├── Certificates/              # Issuance, live preview, and archival logs
    │   ├── Home/                      # Dashboard, privacy, and access denied views
    │   ├── Portal/                    # Resident-facing submission and confirmation
    │   ├── Reports/                   # Weekly/monthly charts and statistical breakdowns
    │   ├── Residents/                 # Resident listing, registration, and editing
    │   ├── Shared/                    # Shared layouts (_Layout, _AuthLayout) and partials
    │   └── UserManagement/            # Staff directory and profile maintenance
    └── wwwroot/                       # Static web assets
        ├── css/                       # Custom application stylesheets
        ├── js/                        # Client-side scripts
        ├── images/                    # Logos, branding graphics, and backgrounds
        ├── lib/                       # Vendor libraries (Bootstrap, jQuery, Validation)
        ├── templates/                 # Word (.docx) templates for certificates
        └── generated/                 # Generated .docx files for download
```

---

## 7. Scripts & Testing

Execute these commands from the repository root directory using the .NET CLI:

### Local Development
```bash
# Run the application with hot reload enabled
dotnet watch --project BarangayLangkiwaSystem

# Standard run
dotnet run --project BarangayLangkiwaSystem
```

### Build & Restore
```bash
# Restore package dependencies
dotnet restore BarangayLangkiwaSystem.slnx

# Compile the solution in Debug mode
dotnet build BarangayLangkiwaSystem.slnx

# Compile the solution in Release mode
dotnet build BarangayLangkiwaSystem.slnx -c Release
```

### Production Publish
```bash
# Publish self-contained or framework-dependent binaries for MonsterASP.NET / IIS
dotnet publish BarangayLangkiwaSystem/BarangayLangkiwaSystem.csproj -c Release -o ./publish
```

### Testing & Validation
```bash
# Run test suite across the solution
dotnet test BarangayLangkiwaSystem.slnx --logger "console;verbosity=detailed"

# Code formatting and code style verification
dotnet format whitespace --verify-no-changes
```

---

## 8. License & Contact

### License
This project is licensed under the [MIT License](LICENSE) - see the LICENSE file for details.

### Original Developers & Contributors
* **Rance Aaron D. Capiral**
* **Jean Andrei S. Maquinto**
* **Ryzel Clifford C. Realon**

For questions, support, or showcase inquiries, please visit the repository issue tracker or contact the maintainers via their official development channels.
