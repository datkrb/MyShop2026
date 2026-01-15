# API SPECIFICATION – MYSHOP 2025

| Meta Key           | Value                       |
| :----------------- | :-------------------------- |
| **Base URL**       | `http://localhost:3000/api` |
| **Authentication** | JWT Bearer Token            |
| **Database**       | PostgreSQL (via Prisma ORM) |

---

## 1. AUTHENTICATION

### 1.1 Login

**POST** `/auth/login`
Đăng nhập hệ thống.

**Request:**

```json
{
  "username": "admin",
  "password": "123456"
}
```

**Response:**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsIn...",
  "role": "ADMIN",
  "expiresIn": 3600
}
```

### 1.2 Get Current User

**GET** `/auth/me`
Lấy thông tin người dùng hiện tại (dùng cho auto login / reload page).

**Response:**

```json
{
  "id": 1,
  "username": "admin",
  "role": "ADMIN"
}
```

### 1.3 Logout

**POST** `/auth/logout`
Đăng xuất (Client side xóa token, Server side có thể blacklist token nếu cần).

---

## 2. SYSTEM / HEALTH CHECK

### 2.1 Health Check

**GET** `/health`
Kiểm tra trạng thái server.

**Response:**

```json
{
  "status": "OK",
  "version": "1.0.0"
}
```

---

## 3. DASHBOARD

### 3.1 Summary

**GET** `/dashboard/summary`
Tổng quan hệ thống.

**Response:**

```json
{
  "totalProducts": 120,
  "totalOrdersToday": 15,
  "revenueToday": 3500000
}
```

### 3.2 Low Stock Products

**GET** `/dashboard/low-stock`
Danh sách top 5 sản phẩm sắp hết hàng.

### 3.3 Top Selling Products

**GET** `/dashboard/top-selling`
Danh sách top 5 sản phẩm bán chạy nhất.

### 3.4 Revenue Chart

**GET** `/dashboard/revenue-chart`
Dữ liệu biểu đồ doanh thu theo ngày trong tháng hiện tại.

### 3.5 Recent Orders

**GET** `/dashboard/recent-orders`
Danh sách 3 đơn hàng gần nhất.

---

## 4. CATEGORY

### 4.1 List Categories

**GET** `/categories`
Lấy danh sách loại sản phẩm.

### 4.2 Create Category

**POST** `/categories`
Tạo loại sản phẩm mới.

**Request:**

```json
{
  "name": "Beverages",
  "description": "Drinks and beverages"
}
```

### 4.3 Update Category

**PUT** `/categories/{id}`

### 4.4 Delete Category

**DELETE** `/categories/{id}`

---

## 5. PRODUCT

### 5.1 List Products

**GET** `/products`
Danh sách sản phẩm (hỗ trợ phân trang, lọc, tìm kiếm).

**Query Params:**

- `page`: 1
- `size`: 10
- `sort`: `price,desc`
- `minPrice`: 10000
- `maxPrice`: 50000
- `keyword`: `coffee`
- `categoryId`: 1

### 5.2 Product Detail

**GET** `/products/{id}`

### 5.3 Create Product

**POST** `/products`

**Request:**

```json
{
  "name": "Milk Tea",
  "sku": "MT001",
  "importPrice": 10000,
  "salePrice": 20000,
  "stock": 50,
  "categoryId": 1,
  "description": "Delicious milk tea"
}
```

### 5.4 Update Product

**PUT** `/products/{id}`

### 5.5 Delete Product

**DELETE** `/products/{id}`

### 5.6 Import Products

**POST** `/products/import`
Import sản phẩm từ file Excel / Access.

### 5.7 Upload Images

**POST** `/products/{id}/images`
Upload hình ảnh cho sản phẩm.

---

## 6. CUSTOMER

### 6.1 List Customers

**GET** `/customers`
Danh sách khách hàng (phân trang, tìm kiếm).

### 6.2 Customer Detail

**GET** `/customers/{id}`

### 6.3 Create Customer

**POST** `/customers`

### 6.4 Update Customer

**PUT** `/customers/{id}`

### 6.5 Delete Customer

**DELETE** `/customers/{id}`

---

## 7. ORDER

### 7.1 List Orders

**GET** `/orders`
Danh sách đơn hàng.

**Query Params:**

- `page`: 1
- `size`: 10
- `fromDate`: `2025-01-01`
- `toDate`: `2025-01-31`
- `status`: `PAID`

### 7.2 Order Detail

**GET** `/orders/{id}`

### 7.3 Create Order

**POST** `/orders`

**Request:**

```json
{
  "customerId": 1,
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

### 7.4 Update Order

**PUT** `/orders/{id}`

### 7.5 Delete Order

**DELETE** `/orders/{id}`

### 7.6 Update Order Status

**PUT** `/orders/{id}/status`

**Request:**

```json
{
  "status": "PAID"
}
```

### 7.7 Autosave Order

**POST** `/orders/{id}/autosave`
Lưu nháp đơn hàng (Draft).

### 7.8 Export Order

**GET** `/orders/{id}/export`
Xuất đơn hàng ra định dạng PDF / XPS.

---

## 8. REPORT

### 8.1 Revenue Report

**GET** `/reports/revenue`
Báo cáo doanh thu theo ngày / tháng / năm.

**Query Params:**

- `type`: `month` (hoặc `day`, `year`)
- `year`: 2025

### 8.2 Profit Report

**GET** `/reports/profit`

### 8.3 Product Sales Report

**GET** `/reports/products`
Báo cáo số lượng sản phẩm bán ra.

### 8.4 KPI & Commission

**GET** `/reports/kpi-sales`
Báo cáo KPI và hoa hồng cho nhân viên Sales.

---

## 9. BACKUP / RESTORE (ADMIN ONLY)

### 9.1 Backup Database

**POST** `/admin/backup`
Tạo bản sao lưu cơ sở dữ liệu.

### 9.2 Restore Database

**POST** `/admin/restore`
Khôi phục dữ liệu từ file backup.

---

## 10. SECURITY & ROLES

### 10.1 Roles Defined

1. **ADMIN**

   - Toàn quyền hệ thống.
   - Có thể thực hiện Backup/Restore.
   - Xem được giá nhập (Import Price).

2. **SALE**
   - **Không** xem được giá nhập.
   - Chỉ xem được đơn hàng do mình tạo (hoặc quản lý).
   - Quyền hạn chế trên các module báo cáo.

---

## 11. ERROR FORMAT

Mọi lỗi trả về từ server sẽ tuân theo cấu trúc sau:

```json
{
  "error": "INVALID_REQUEST",
  "message": "Invalid input data"
}
```

---

## 12. STATUS CODES

| Code    | Status                | Description                             |
| :------ | :-------------------- | :-------------------------------------- |
| **200** | OK                    | Request thành công.                     |
| **201** | Created               | Tạo mới tài nguyên thành công.          |
| **400** | Bad Request           | Dữ liệu gửi lên không hợp lệ.           |
| **401** | Unauthorized          | Chưa đăng nhập hoặc token hết hạn.      |
| **403** | Forbidden             | Không có quyền truy cập tài nguyên này. |
| **404** | Not Found             | Không tìm thấy tài nguyên.              |
| **500** | Internal Server Error | Lỗi nội bộ server.                      |
