## Context
This is an ASP.NET Core MVC application designed to automate resident registration and digitize certificate issuance for a Philippine Barangay. It is hosted on MonsterASP.NET and serves purely as a portfolio showcase piece. **Crucially, no external cloud or physical database (SQL, Supabase) will be used.** All data persistence must continue to rely on the existing `InMemoryDataStore`.

**Goal:** Complete the missing interfaces, tighten security/validation, and elevate the UI/UX so the application is fully functional and presentable as a professional portfolio demonstration. All core workflows (especially certificate generation and downloading) must execute flawlessly using the mocked data to prove the system's logic to viewers.

## Implementation Tasks

### 1. Model Validation & Data Integrity
**Objective:** Ensure all incoming data is strictly validated before hitting the controllers, proving to viewers that the system handles edge cases.
*   **Task:** Add Data Annotations to domain models.
    *   `Resident.cs`: Apply `[Required]` to `FirstName`, `LastName`, `Birthdate`, `Gender`, `CivilStatus`, `Address`, and `Purok`. Apply `[Phone]` to `ContactNo` and `[EmailAddress]` to `Email`.
    *   `CertRequest.cs`: Apply `[Required]` to `ResidentId`, `CertificateType`, and `Purpose`.
*   **Acceptance Criteria:** Submitting empty or invalidly formatted data in `Residents/Register.cshtml` or `Certificates/Create.cshtml` triggers client-side and server-side validation error messages without crashing.

### 2. Global Exception Handling & Security
**Objective:** Prevent unhandled exceptions from exposing stack traces and standardize authorization.
*   **Task A (Exception Handling):** Ensure `Program.cs` global exception middleware is correctly configured. Wrap critical operations (e.g., file generation in `CertificatesController.Approve`, record deletion in `ResidentsController.DeleteConfirmed`) in `try-catch` blocks, logging errors and redirecting to a friendly error view.
*   **Task B (Authorization):** Replace the manual role string checking inside `BaseController` and `ReportsController` with ASP.NET Core standard `[Authorize]` attributes (e.g., `[Authorize(Roles = "Barangay Captain, Barangay Secretary")]`).
*   **Acceptance Criteria:** Generating a certificate with a missing `.docx` template does not crash the app but displays a handled error message. Unauthorized users navigating directly to `/Reports` are automatically redirected to the Access Denied view.

### 3. UI/UX Polish: Pagination & Toast Notifications
**Objective:** Improve the user experience for managing large datasets and receiving system feedback, crucial for a portfolio UI demonstration.
*   **Task A (Toast Notifications):** Implement a global Toast notification system (using Bootstrap Toasts or a library like Toastr). Refactor controllers to pass success/error messages via `TempData` (e.g., `TempData["Success"] = "Resident added."`) and render these in `_Layout.cshtml`.
*   **Task B (Pagination):** Implement server-side pagination for `ResidentsController.Index` and `CertificatesController.Index`. The views should display a maximum of 10-20 records per page with working "Next" and "Previous" controls.
*   **Acceptance Criteria:** Registering a resident or approving a certificate displays a green success toast. The Resident Index view no longer loads the entire `InMemoryDataStore.Residents` list at once.

### 4. New Feature: Resident Request Portal
**Objective:** Close the gap in the certificate workflow by allowing a resident to initiate a request, demonstrating a complete end-to-end user journey.
*   **Task:** Create a new controller/view set (`PortalController`) accessible without authentication.
    *   It should feature a simple form where a user inputs their Resident ID or Name, selects a `CertificateType`, and inputs a `Purpose`.
    *   Submitting this form creates a new `CertRequest` in the `InMemoryDataStore` with a status of "Pending".
*   **Acceptance Criteria:** An unauthenticated user can submit a request via the portal. That request immediately appears in the Reports Dashboard (`GetChartData` recent items) for an admin to process.

### 5. Document Generation Workflow Optimization (MonsterASP.NET Compatibility)
**Objective:** Ensure the core `.docx` certificate generation and download functionality works flawlessly in the hosted MonsterASP.NET environment.
*   **Task:** Review the file I/O operations in `CertificatesController.Approve`.
    *   Ensure the pathing logic for reading templates from `wwwroot/templates` and saving to `wwwroot/generated` uses robust relative pathing (`IWebHostEnvironment.WebRootPath`) rather than hardcoded directory structures that might break on MonsterASP.NET's IIS servers.
    *   Verify the byte array return for the file download successfully triggers the browser prompt.
*   **Acceptance Criteria:** An admin can successfully click "Approve" on a pending certificate request, which manipulates the Word template and prompts a successful `.docx` download for the user.

## Constraints & Rules for Jules
*   **STRICT MOCK DATA ENFORCEMENT:** Do NOT install Entity Framework Core or attempt to connect to any SQL, NoSQL, or Cloud database. This project is strictly a UI/UX logic showcase. You MUST continue using and expanding the `InMemoryDataStore.cs` pattern for all data persistence.
*   **Template Engine:** Do not modify the `mammoth.js` client-side implementation in `Certificates/Create.cshtml` unless it is explicitly broken by validation changes.
*   **Styling:** Adhere to the existing Bootstrap 5 utility classes and styling conventions present in the views.
''')
