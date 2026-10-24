# 🐝 BeeMart - Point of Sale (POS) System

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET 8](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API_%26_Razor_Pages-purple.svg)](https://dotnet.microsoft.com/apps/aspnet)
[![EF Core](https://img.shields.io/badge/Entity_Framework_Core-8.0-green.svg)](https://docs.microsoft.com/ef/core/)

**BeeMart** is an integrated Point of Sale (POS) web application designed and built for the **Binus IT Bootcamp 2026/2027**. The solution strictly adheres to the **Separation of Concerns (SoC)** architectural pattern, separating the presentation layer from business logic and data persistence.

---

## 🌟 Key Highlights & Requirements Fulfillment

- **Separation of Concerns**: The frontend (`BeeMart.Web`) acts 100% as a data consumer and makes zero direct connections to the database. All state and operations are processed via RESTful calls to the backend (`BeeMart.Api`).
- **No Bootstrap Constraint**: Built entirely using **pure custom modern CSS** (`site.css`), offering responsive design, custom modals, real-time toasts, and clean retail interfaces without external CSS frameworks.
- **Role-Based Access Control**:
  - **Kasir (Cashier)**: Conducts transactions and can only view their own transaction history.
  - **Admin**: Has oversight over the entire system and can view transaction history across all cashiers.
- **Immutable Invoice Number Format**:
  - Automatically generated according to specification: `[5 digit userID][ddmmyy][3 digit running number]` (e.g., `76985040926001`).
- **Stock Management & Validation**:
  - Real-time product search with up to 10 results.
  - Interactive modal dialog for item selection.
  - Exact specification validation: if quantity exceeds remaining inventory, user receives `"Jumlah barang yang dimasukkan melebihi sisa stok."`
  - Automatic stock deduction upon transaction completion.
- **Transaction History & Pagination**:
  - Mandatory date filter picker.
  - Dynamic sub-header reflecting role (`Pencarian Data Transaksi (Hanya Kasir: <username>)` vs `Pencarian Data Transaksi (Menampilkan Semua Kasir)`).
  - Chronologically sorted **ascending (ASC)** by Date and Time.
  - Paginated at a maximum of 20 items per page.
- **Indonesian Localization (I18N)**:
  - Formatted throughout in Indonesian Rupiah (IDR, e.g., `Rp 89.000`).
  - Localized date/time display and real-time ticking header clock.

---

## 📐 Architecture & Documentation

Comprehensive architectural blueprints and system specifications are located in the [`docs-architect/`](./docs-architect) directory:

- [**System Architecture & C4 Diagrams**](./docs-architect/ARCHITECTURE.md)
- [**Database ERD & Data Dictionary**](./docs-architect/DATABASE_ERD.md)
- [**RESTful API Specification (v1)**](./docs-architect/API_SPECIFICATION.md)
- [**Deployment & Run Guide**](./docs-architect/DEPLOYMENT_AND_RUN_GUIDE.md)

---

## 🏗️ Project Structure

```text
beemart/
├── Backend/
│   └── BeeMart.Api/             # ASP.NET Core 8 Web API (Business Logic, EF Core)
│       ├── Controllers/         # Versioned API controllers (/api/v1/...)
│       ├── Data/                # DbContext, Entities, and DbInitializer
│       ├── DTOs/                # Request and Response transfer models
│       ├── Services/            # AuthService, TransactionService, ProductService
│       └── appsettings.json     # Connection strings (SQLite & SQL Server)
├── Frontend/
│   └── BeeMart.Web/             # ASP.NET Core 8 Razor Pages (Presentation Layer)
│       ├── Pages/               # Login, Register, Transaction, History
│       ├── wwwroot/
│       │   ├── css/site.css     # Pure custom modern CSS (No Bootstrap)
│       │   └── js/site.js       # Client state, API caller, Realtime clock
│       └── appsettings.json     # API BaseUrl configuration
├── Tests/
│   └── BeeMart.Tests/           # Automated test suite (xUnit, EF Core InMemory/SQLite)
│       ├── AuthServiceTests.cs
│       ├── InvoiceNumberTests.cs
│       ├── TransactionServiceTests.cs
│       ├── HistoryRoleFilterTests.cs
│       └── ProductAndCartEdgeCasesTests.cs
├── Database/
│   └── BeeMart_DDL_DML.sql      # Complete SQL Server DDL & DML script
├── docs-architect/              # Architectural diagrams and technical specs
├── BeeMart.sln                  # Visual Studio Solution File
├── LICENSE                      # MIT License
└── README.md
```

---

## ⚡ Quickstart

### 1. Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### 2. Run Backend API
```powershell
dotnet run --project Backend/BeeMart.Api/BeeMart.Api.csproj --urls "http://localhost:5000"
```
*Swagger UI is accessible at [http://localhost:5000/swagger](http://localhost:5000/swagger)*

### 3. Run Frontend WebClient
In a separate terminal:
```powershell
dotnet run --project Frontend/BeeMart.Web/BeeMart.Web.csproj --urls "http://localhost:5100"
```
*Open your browser and navigate to [http://localhost:5100](http://localhost:5100)*

---

## 🔑 Pre-Seeded Test Accounts

| Role | Username | Password | User ID | Capabilities |
|---|---|---|---|---|
| **Kasir (Cashier)** | `steven` | `password` | `76985` | New transactions, personal history view |
| **Kasir (Cashier)** | `maria` | `password` | `66859` | New transactions, personal history view |
| **Kasir (Cashier)** | `budi` | `password` | `78886` | New transactions, personal history view |
| **Admin** | `admin` | `password` | `55794` | View history of **all** cashiers |
| **Admin** | `superadmin`| `password` | `55356` | Master administrative view |

---

## 🧪 Testing

Execute the automated test suite:

```powershell
dotnet test
```

All 28 unit and integration tests will execute and verify invoice generation, concurrency synchronization, stock constraints, role filtering, and error handling.

---

## 📄 License

This project is licensed under the [MIT License](./LICENSE).
