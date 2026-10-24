# 🚀 BeeMart POS - Quickstart & Deployment Guide

This guide describes how to run and verify the **BeeMart Point of Sale (POS)** application on any machine.

---

## 1. Prerequisites

- **.NET 8 SDK** (or later, .NET 10 SDK with .NET 8 targeting support).
- Any standard web browser (Chrome, Edge, Firefox, Safari).
- *(Optional)* Microsoft SQL Server 2019/2022 or LocalDB if using SQL Server provider instead of the default SQLite.

---

## 2. Default Test Accounts

The system automatically initializes and seeds the database with the required test accounts upon first startup:

| Username | Default Password | Role | User ID (5 Digits) | Description |
|---|---|---|---|---|
| `steven` | `password` | **Kasir (Cashier)** | `76985` | Primary test cashier matching PDF specification |
| `maria` | `password` | **Kasir (Cashier)** | `66859` | Secondary test cashier |
| `budi` | `password` | **Kasir (Cashier)** | `78886` | Branch cashier |
| `admin` | `password` | **Admin** | `55794` | System Administrator (can view all cashiers) |
| `superadmin`| `password` | **Admin** | `55356` | Head Office Supervisor |

---

## 3. How to Run the Application

### Option A: Running with .NET CLI (Recommended)

Open two terminal windows:

#### Terminal 1: Start Backend API (Port 5000)
```powershell
dotnet run --project Backend/BeeMart.Api/BeeMart.Api.csproj --urls "http://localhost:5000"
```
*API Swagger documentation will be available at: [http://localhost:5000/swagger](http://localhost:5000/swagger)*

#### Terminal 2: Start Frontend WebClient (Port 5100)
```powershell
dotnet run --project Frontend/BeeMart.Web/BeeMart.Web.csproj --urls "http://localhost:5100"
```
*Open your browser and navigate to: [http://localhost:5100](http://localhost:5100)*

---

## 4. Running the Test Suite

The solution comes with an automated unit and integration test suite in `Tests/BeeMart.Tests`:

```powershell
dotnet test
```

All test cases (invoice number formatting, stock validation, cashier role isolation, authentication errors) will execute automatically.

---

## 5. Database Configuration: SQLite vs Microsoft SQL Server

By default, the application runs using **SQLite** (`beemart.db`), enabling instant out-of-the-box execution without any external database server setup.

### Switching to Microsoft SQL Server:
1. Open `Backend/BeeMart.Api/appsettings.json`.
2. Change `"DatabaseProvider"` to `"SqlServer"`:
```json
{
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "SqlServerConnection": "Server=localhost;Database=BeeMartDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```
3. Execute the provided SQL script:
   - File location: `Database/BeeMart_DDL_DML.sql`
   - Run via SQL Server Management Studio (SSMS), Azure Data Studio, or `sqlcmd`:
```powershell
sqlcmd -S localhost -E -i Database/BeeMart_DDL_DML.sql
```
