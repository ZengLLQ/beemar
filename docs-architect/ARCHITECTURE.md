# 🏛️ BeeMart POS - Architecture Documentation

## 1. Overview & Architectural Principles

**BeeMart** is an enterprise Point of Sale (POS) system built using **ASP.NET Core on .NET 8**, designed according to the **Separation of Concerns (SoC)** principle. The system is cleanly split into two distinct tiers:

1. **Frontend (BeeMart.Web)**:
   - Built on ASP.NET Core Razor Pages with **pure custom modern CSS** (strictly zero external CSS framework such as Bootstrap).
   - Functions strictly as a **data consumer** (`Presentation Tier`).
   - Has **zero direct access** to database engines or ORMs.
   - Communicates solely through HTTP/REST calls with JSON payloads to the Backend API.
   - Manages client-side states (active session, shopping cart, realtime clock, and search filters).

2. **Backend (BeeMart.Api)**:
   - High-performance ASP.NET Core Web API with versioned endpoints (`/api/v1/...`).
   - Houses all domain logic, validations, invoice number sequential generation, stock deduction, and persistence via **Entity Framework Core 8**.
   - Dual-database provider support: Portable zero-config SQLite out of the box and Microsoft SQL Server 2019/2022 / LocalDB.
   - Exposes interactive OpenAPI/Swagger documentation.

---

## 2. C4 Architecture Diagrams

### 2.1 System Context Diagram (C4 Level 1)

```mermaid
graph TD
    UserCashier["👤 Cashier / Kasir"] -->|Interacts via Browser| BeeMartWeb["🌐 BeeMart WebClient (UI)"]
    UserAdmin["👤 Administrator"] -->|Interacts via Browser| BeeMartWeb

    BeeMartWeb -->|RESTful HTTP / JSON /api/v1/...| BeeMartApi["⚙️ BeeMart Backend API (.NET 8)"]
    
    BeeMartApi -->|EF Core / ADO.NET| Database[("🗄️ Database (SQL Server / SQLite)")]
```

### 2.2 Container Diagram (C4 Level 2)

```mermaid
graph LR
    subgraph Client Browser
        UI["Modern Custom UI (Pure CSS & JS)"]
        State["Client State (User Session, Cart, Realtime Clock)"]
    end

    subgraph "Frontend Container: BeeMart.Web (Port: 5100/5101)"
        Pages["Razor Pages (/Login, /Register, /Transaction, /History)"]
        StaticAssets["Custom CSS (site.css) & JS (site.js)"]
    end

    subgraph "Backend Container: BeeMart.Api (Port: 5000/5001)"
        Controllers["API Controllers (Auth, Products, PaymentMethods, Transactions)"]
        Services["Domain Services (TransactionService, AuthService, ProductService)"]
        EF["Entity Framework Core 8 DbContext"]
    end

    subgraph "Persistence Tier"
        DB[("BeeMartDb (Tables: MsUser, LtPaymentMethod, MsProduct, TrTransaction, TrTransactionDetail)")]
    end

    UI --> Pages
    Pages --> StaticAssets
    UI -->|HTTP Fetch API| Controllers
    Controllers --> Services
    Services --> EF
    EF --> DB
```

### 2.3 Component Diagram (C4 Level 3)

```mermaid
graph TD
    subgraph "BeeMart.Api Components"
        AC[AuthController] --> AS[AuthService]
        PC[ProductsController] --> PS[ProductService]
        PMC[PaymentMethodsController] --> PMS[PaymentMethodService]
        TC[TransactionsController] --> TS[TransactionService]
        
        AS --> PH[PasswordHasher SHA256]
        AS --> DbContext[(BeeMartDbContext)]
        PS --> DbContext
        PMS --> DbContext
        TS --> DbContext
    end
```

---

## 3. Core Business Flows & Sequence Diagrams

### 3.1 New Transaction & Stock Deduction Flow

```mermaid
sequenceDiagram
    autonumber
    actor Cashier
    participant UI as BeeMart.Web (UI)
    participant API as BeeMart.Api
    participant DB as Database

    Cashier->>UI: Opens /Transaction
    UI->>API: GET /api/v1/transactions/next-invoice?userId=76985
    API->>DB: Query today's invoices with prefix '76985040926'
    DB-->>API: Max suffix = '001'
    API-->>UI: Next Invoice = '76985040926002'
    
    Cashier->>UI: Types "Apel" & presses Enter
    UI->>API: GET /api/v1/products/search?q=Apel
    API->>DB: SELECT TOP 10 WHERE Name LIKE '%Apel%' AND Stsrc=1
    DB-->>API: Product records
    API-->>UI: Returns products list
    
    Cashier->>UI: Clicks "Pilih", enters Qty (e.g., 2), clicks "Masukkan ke Keranjang"
    UI->>UI: Validates Qty <= Stock, updates Cart in sessionStorage
    
    Cashier->>UI: Selects Payment Method (e.g., Cash) and clicks "Submit Transaksi"
    UI->>API: POST /api/v1/transactions { userId, paymentMethodId, items }
    API->>DB: Validates stock >= requested Qty
    alt Stock Insufficient
        API-->>UI: 400 Bad Request: "Jumlah barang yang dimasukkan melebihi sisa stok."
        UI->>Cashier: Displays Error Alert/Toast
    else Stock Sufficient
        API->>DB: Deduct MsProduct.Stock, INSERT TrTransaction & TrTransactionDetail
        DB-->>API: Commit Transaction
        API-->>UI: 200 OK: Invoice generated, details saved
        UI->>Cashier: Success Toast, resets Cart, fetches new Invoice Number
    end
```

### 3.2 Role-Based History Filtering Flow

```mermaid
sequenceDiagram
    autonumber
    actor User as Cashier / Admin
    participant UI as BeeMart.Web
    participant API as TransactionsController
    participant Service as TransactionService
    participant DB as Database

    User->>UI: Selects Date (e.g., 2026-04-09) & clicks "Tampilkan Data"
    UI->>API: GET /api/v1/transactions/history?date=2026-04-09&role={role}&userId={idUser}
    API->>Service: GetHistoryAsync(date, userId, role, page, pageSize)
    
    alt Role is Cashier
        Service->>DB: WHERE TransactionDate.Date == date AND IdUser == userId ORDER BY TransactionDate ASC
    else Role is Admin
        Service->>DB: WHERE TransactionDate.Date == date ORDER BY TransactionDate ASC (All Cashiers)
    end
    
    DB-->>Service: Paginated records (max 20 per page)
    Service-->>API: PaginatedResponse<TransactionHistoryItemDto>
    API-->>UI: 200 OK with items and pagination metadata
    UI->>User: Renders table & pagination buttons
```

---

## 4. Invoice Number Generation Algorithm

The system enforces a standardized 14-character invoice number:
$$\text{InvoiceNo} = [\text{5-digit UserID}][\text{ddMMyy}][\text{3-digit Running Number}]$$

- **5-digit UserID**: Guaranteed 5 numeric digits (e.g., `76985`), zero-padded if necessary.
- **ddMMyy**: 6 digits representing transaction local date (e.g., `040926` for 4 September 2026).
- **3-digit Running Number**: Formatted as `D3` starting from `001` up to `999` per user per day.
- **Concurrency & Idempotency**: Evaluated using atomic database checks for existing invoices with matching prefix.
