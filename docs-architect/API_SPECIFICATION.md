# 📡 BeeMart POS - RESTful API Specification (v1)

Base URL: `http://localhost:5000/api/v1`

---

## 1. Response Envelope

All API endpoints return a standardized JSON envelope:

```json
{
  "success": true,
  "message": "Operation description",
  "data": { ... },
  "errors": null
}
```

On validation or operational error:
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": [ "Detailed validation issue" ]
}
```

---

## 2. Authentication Endpoints

### 2.1 Login
- **Endpoint**: `POST /api/v1/auth/login`
- **Request Body**:
```json
{
  "username": "steven",
  "password": "password",
  "role": "Cashier"
}
```
- **Validation**:
  - `username`: Required.
  - `password`: Required.
  - `role`: Required (`"Admin"` or `"Cashier"`).
  - Validates combination against `MsUser` with SHA-256 hash.
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login berhasil.",
  "data": {
    "idUser": "76985",
    "username": "steven",
    "fullName": "Steven Anderson",
    "role": "Cashier",
    "token": "660761986de349b9b7cdad4239d9216d"
  }
}
```
- **Error Response (400 Bad Request)**:
```json
{
  "success": false,
  "message": "Invalid login",
  "data": null
}
```

---

### 2.2 Register
- **Endpoint**: `POST /api/v1/auth/register`
- **Request Body**:
```json
{
  "username": "kasirbaru",
  "fullName": "Kasir Baru BeeMart",
  "password": "password123",
  "rePassword": "password123",
  "role": "Cashier"
}
```
- **Validation**:
  - All fields mandatory.
  - Password and Re-Password must match.
  - Checks if username already exists.
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Registrasi berhasil.",
  "data": {
    "idUser": "84729",
    "username": "kasirbaru",
    "fullName": "Kasir Baru BeeMart",
    "role": "Cashier",
    "token": "a1b2c3..."
  }
}
```
- **Error Response if Username Exists (400 Bad Request)**:
```json
{
  "success": false,
  "message": "username has already exists",
  "data": null
}
```

---

## 3. Products Endpoints

### 3.1 Search Products
- **Endpoint**: `GET /api/v1/products/search?q={query}`
- **Query Parameters**:
  - `q` (optional): search term (e.g., `Apel`, `Susu`).
- **Behavior**: Returns at most 10 active products matching the search query case-insensitively.
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "idProduct": "11111111-1111-1111-1111-111111111101",
      "name": "Apel Fuji Premium",
      "weight": 1.0,
      "stock": 50,
      "price": 45000.0,
      "discount": 5.0,
      "weightFormatted": "1 Kg",
      "finalUnitPrice": 42750.0
    }
  ]
}
```

---

## 4. Payment Methods Endpoints

### 4.1 List Payment Methods
- **Endpoint**: `GET /api/v1/payment-methods`
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Success",
  "data": [
    { "idPaymentMethod": 1, "methodName": "Cash" },
    { "idPaymentMethod": 2, "methodName": "Debit" },
    { "idPaymentMethod": 3, "methodName": "Credit" }
  ]
}
```

---

## 5. Transactions Endpoints

### 5.1 Next Invoice Number
- **Endpoint**: `GET /api/v1/transactions/next-invoice?userId={userId}`
- **Query Parameters**:
  - `userId` (mandatory): 5-digit user ID (e.g., `76985`).
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "invoiceNo": "76985040926001"
  }
}
```

---

### 5.2 Submit Transaction
- **Endpoint**: `POST /api/v1/transactions`
- **Request Body**:
```json
{
  "idUser": "76985",
  "idPaymentMethod": 1,
  "items": [
    {
      "idProduct": "11111111-1111-1111-1111-111111111101",
      "qty": 2
    }
  ]
}
```
- **Validation & Business Logic**:
  - Validates `idUser` exists and active.
  - Validates `idPaymentMethod` exists and active.
  - Validates items count $\ge 1$.
  - Validates for each item: `qty <= product.stock`.
  - If stock insufficient: returns 400 Bad Request with `"Jumlah barang yang dimasukkan melebihi sisa stok."`
  - Deducts product stock in DB.
  - Generates immutable `InvoiceNo`.
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Transaksi berhasil diproses.",
  "data": {
    "idTransaction": "594d96c4-c975-4c9b-b4cf-c64d1a19c875",
    "invoiceNo": "76985040926001",
    "transactionDate": "2026-04-09T10:15:30Z",
    "idUser": "76985",
    "cashierName": "steven",
    "idPaymentMethod": 1,
    "paymentMethodName": "Cash",
    "totalPrice": 85500.0,
    "items": [
      {
        "idTransactionDetail": "1eec5cc3-...",
        "idProduct": "11111111-1111-1111-1111-111111111101",
        "productName": "Apel Fuji Premium",
        "qty": 2,
        "price": 45000.0,
        "discount": 5.0,
        "subTotal": 85500.0
      }
    ]
  }
}
```

---

### 5.3 History Transaction (With Pagination & Role Isolation)
- **Endpoint**: `GET /api/v1/transactions/history`
- **Query Parameters**:
  - `date` (mandatory): `YYYY-MM-DD` string.
  - `role` (optional): `"Admin"` or `"Cashier"`.
  - `userId` (optional for Admin, mandatory for Cashier): 5-digit user ID.
  - `page` (optional, default 1): Page number.
  - `pageSize` (optional, default 20, max 20): Items per page.
- **Success Response (200 OK)**:
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false,
    "items": [
      {
        "idTransaction": "594d96c4-c975-4c9b-b4cf-c64d1a19c875",
        "invoiceNo": "76985040926001",
        "transactionDate": "2026-04-09T10:15:30Z",
        "cashierUsername": "steven",
        "cashierFullName": "Steven Anderson",
        "totalItems": 2,
        "paymentMethodName": "Cash",
        "totalPrice": 85500.0
      }
    ]
  }
}
```
