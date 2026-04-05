# Web-Based Resident and Certification Management System for Barangay Langkiwa

## Project Overview
This project is a web-based solution designed to automate resident profiling and the issuance of certifications for Barangay Langkiwa. It was developed to transition the barangay from inefficient manual and spreadsheet-based recording to a modern, centralized digital platform. The system aims to centralize records, reduce manual workloads, and significantly enhance data integrity and security.

## Key Features
* **Resident Profiling and Management:** Allows for the creation, updating, and real-time searching of resident demographic and socio-economic records within a unified platform.
* **Automated Certification Issuance:** Automates the generation of official standard certifications, such as Barangay Clearances and Certificates of Indigency, using stored resident data. The system also provides a live document preview before printing.
* **Centralized Database:** Utilizes a secure relational database to consolidate all resident records, eliminating data fragmentation and preventing redundancy.
* **Role-Based Access Control (RBAC):** Integrates security features to limit encoding privileges exclusively to authorized personnel.
* **Audit Logging:** Maintains comprehensive system audit logs to record every significant database change or user action, ensuring accountability.
* **Automated Reporting:** Generates official operational management reports, such as monthly resident summaries, which can be exported to Excel or PDF for LGU submission.

## Technologies Used
* **Frontend:** Built using ASP.NET Core MVC (Razor Views), integrated with HTML5, CSS3, JavaScript, and Bootstrap 5 to create a responsive and modern user interface.
* **Backend:** Developed in C# utilizing the ASP.NET Core framework.
* **Database:** Powered by MSSQL and managed via Entity Framework Core.

## User Roles
* **Barangay Staff (Encoder):** Responsible for daily data encoding, registering profiles from paper forms, updating resident statuses, and processing walk-in certificate requests.
* **Barangay Secretary (Admin):** Acts as the system administrator with full access to manage user accounts, review audit trails, approve restricted documents, and generate statistical reports for the Local Government Unit.

## Developers
* Rance Aaron D. Capiral
* Jean Andrei S. Maquinto
* Ryzel Clifford C. Realon
