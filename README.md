# MyShop - Hệ Thống Quản Lý Cửa Hàng

## 👥 Thông Tin Nhóm

| Họ và Tên | MSSV |
|-----------|------|
| Trần Gia Cường | 23120225 |
| Nguyễn Ngọc Đại | 23120226 |
| Nguyễn Hà Đạt | 23120229 |

---

## 📋 Tổng Quan Dự Án

MyShop là hệ thống quản lý cửa hàng toàn diện được xây dựng với kiến trúc Client-Server:
- **Client**: WinUI 3 Desktop Application (C# .NET 8)
- **Server**: RESTful API (Node.js, Express, TypeScript, PostgreSQL)

---

## ✅ Các Chức Năng Đã Thực Hiện

### 🔹 Chức Năng Cơ Sở (7/7)

| STT | Chức Năng | Điểm | Trạng Thái |
|-----|-----------|------|------------|
| 1 | **CB-Đăng nhập** | 0.25 | ✅ Hoàn thành |
| 2 | **CB-Dashboard tổng quan hệ thống** | 0.5 | ✅ Hoàn thành |
| 3 | **CB-Quản lí sản phẩm - Products (Master data)** | 1.25 | ✅ Hoàn thành |
| 4 | **CB-Quản lí đơn hàng - Orders (Transaction data)** | 1.5 | ✅ Hoàn thành |
| 5 | **CB-Báo cáo thống kê - Report** | 1.0 | ✅ Hoàn thành |
| 6 | **CB-Cấu hình chương trình** | 0.25 | ✅ Hoàn thành |
| 7 | **CB-Đóng gói thành file cài đặt** | 0.25 | ✅ Hoàn thành |

**Tổng điểm chức năng cơ sở: 5/5**

### 🔹 Chức Năng Tự Chọn (12/18)

| STT | Chức Năng | Điểm | Trạng Thái |
|-----|-----------|------|------------|
| 1 | **TC-Auto save khi tạo đơn hàng, thêm mới sản phẩm** | 0.25 | ✅ Hoàn thành |
| 2 | **TC-Responsive layout** | 0.5 | ✅ Hoàn thành |
| 3 | **TC-Bổ sung khuyến mãi giảm giá** | 1.0 | ✅ Hoàn thành |
| 4 | **TC-Chế độ dùng thử 15 ngày** | 0.5 | ✅ Hoàn thành |
| 5 | **TC-Backup / Restore database** | 0.25 | ✅ Hoàn thành |
| 6 | **TC-Sử dụng kiến trúc MVVM** | 0.5 | ✅ Hoàn thành |
| 7 | **TC-Sử dụng Dependency Injection** | 0.5 | ✅ Hoàn thành |
| 8 | **TC-Phân quyền Admin và Sale** | 0.5 | ✅ Hoàn thành |
| 9 | **TC-Hoa hồng bán hàng theo KPI** | 0.25 | ✅ Hoàn thành |
| 10 | **TC-Quản lí khách hàng** | 0.5 | ✅ Hoàn thành |
| 11 | **TC-In đơn hàng (PDF)** | 0.5 | ✅ Hoàn thành |
| 12 | **TC-Sắp xếp theo nhiều tiêu chí** | 0.5 | ✅ Hoàn thành |
| 13 | TC-Kiến trúc Plugin | 1.0 | ❌ Chưa thực hiện |
| 14 | TC-Obfuscator | 0.25 | ❌ Chưa thực hiện |
| 15 | TC-GraphQL API | 1.0 | ❌ Chưa thực hiện |
| 16 | TC-Test cases | 0.5 | ❌ Chưa thực hiện |
| 17 | TC-Tìm kiếm nâng cao | 1.0 | ❌ Chưa thực hiện |
| 18 | TC-Onboarding | 0.5 | ❌ Chưa thực hiện |

**Tổng điểm chức năng tự chọn: 5.75/10**

---

## ❌ Các Chức Năng Chưa Thực Hiện

1. **TC-Kiến trúc Plugin** (1.0 điểm)
   - Yêu cầu phức tạp, cần refactor toàn bộ kiến trúc

2. **TC-Obfuscator** (0.25 điểm)
   - Chưa tìm hiểu công cụ obfuscation cho .NET 8

3. **TC-GraphQL API** (1.0 điểm)
   - Đã sử dụng RESTful API, không cần thiết chuyển sang GraphQL

4. **TC-Test Cases** (0.5 điểm)
   - Đã có file `TEST_CASE.md` nhưng chưa implement automated tests

5. **TC-Tìm kiếm nâng cao** (1.0 điểm)
   - Hiện tại chỉ hỗ trợ tìm kiếm cơ bản theo tên/SKU

6. **TC-Onboarding** (0.5 điểm)
   - Chưa có hướng dẫn sử dụng cho người dùng mới

---

## 🌟 Các Chức Năng Đáng Tự Chọn

### 1. **Kiến Trúc MVVM + Dependency Injection Chuẩn Chỉnh** (0.5 + 0.5 = 1.0 điểm)
- Áp dụng đầy đủ pattern MVVM với CommunityToolkit.Mvvm
- Sử dụng Microsoft.Extensions.DependencyInjection cho IoC Container
- Tách biệt rõ ràng View, ViewModel, Model, Services
- Code dễ maintain, test và mở rộng

### 2. **Hệ Thống Phân Quyền Phức Tạp** (0.5 điểm)
- Phân quyền chi tiết: Admin vs Sale
- Sale chỉ xem được đơn hàng của mình
- Sale không xem được giá nhập
- Middleware authentication & authorization hoàn chỉnh

### 3. **Auto-save Thông Minh** (0.25 điểm)
- Debounce timer 500ms để tránh save liên tục
- Lưu draft vào LocalStorage
- Restore draft khi mở lại form
- Hỗ trợ cả Product và Order

### 4. **Backup/Restore Database** (0.25 điểm)
- Backup toàn bộ PostgreSQL database
- Restore từ file backup
- Hỗ trợ cả local và Docker environment

### 5. **Hệ Thống KPI & Hoa Hồng** (0.25 điểm)
- Tính toán hoa hồng theo doanh số
- Báo cáo KPI chi tiết cho từng nhân viên
- Dashboard hiển thị thống kê real-time

### 6. **In Đơn Hàng PDF Chuyên Nghiệp** (0.5 điểm)
- Sử dụng QuestPDF để tạo PDF
- Layout đẹp, đầy đủ thông tin
- Hỗ trợ in nhiều đơn hàng cùng lúc

### 7. **Responsive Layout** (0.5 điểm)
- Adaptive UI theo kích thước màn hình
- Sử dụng AdaptiveGridView, VariableSizedWrapGrid
- Hỗ trợ từ 1280x720 đến 4K

### 8. **Hệ Thống Khuyến Mãi** (1.0 điểm)
- Tạo mã giảm giá với nhiều loại (%, fixed amount)
- Áp dụng tự động khi đặt hàng
- Quản lý thời hạn và số lượng sử dụng

### 9. **License Management System** (0.5 điểm)
- Chế độ trial 15 ngày
- Kích hoạt bằng license key
- Mã hóa license với RSA
- Kiểm tra license khi khởi động

---

## 🎯 Điểm Tự Đánh Giá

| Thành Viên | MSSV | Đóng Góp | Điểm Tự Đánh Giá |
|------------|------|----------|------------------|
| Trần Gia Cường | 23120225 | Frontend, Backend, Architecture | **100%** |
| Nguyễn Ngọc Đại | 23120226 | Frontend, Backend, Architecture | **100%** |
| Nguyễn Hà Đạt | 23120229 | Frontend, Backend, Architecture | **100%** |

## 🛠️ Công Nghệ Sử Dụng

### Frontend (Client)
- **Framework**: WinUI 3 (.NET 8)
- **Architecture**: MVVM Pattern
- **UI Library**: CommunityToolkit.WinUI
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **State Management**: CommunityToolkit.Mvvm (ObservableObject, RelayCommand)
- **Charts**: LiveChartsCore.SkiaSharpView
- **PDF Generation**: QuestPDF
- **Excel**: MiniExcel

### Backend (Server)
- **Runtime**: Node.js 20+
- **Framework**: Express.js
- **Language**: TypeScript
- **Database**: PostgreSQL 15+
- **ORM**: Prisma
- **Authentication**: JWT (jsonwebtoken)
- **Validation**: Zod
- **File Upload**: Multer

### DevOps
- **Containerization**: Docker, Docker Compose
- **Database Migration**: Prisma Migrate
- **Environment**: dotenv

---

## 📂 Cấu Trúc Dự Án

```
MyShop/
├── client/                    # WinUI 3 Desktop Application
│   └── MyShopClient/
│       ├── Views/            # XAML Views
│       ├── ViewModels/       # ViewModels (MVVM)
│       ├── Models/           # Data Models
│       ├── Services/         # Business Logic Services
│       ├── Helpers/          # Utilities & Converters
│       └── Infrastructure/   # DI, Navigation
│
├── server/                   # Node.js API Server
│   ├── src/
│   │   ├── controllers/     # Request Handlers
│   │   ├── services/        # Business Logic
│   │   ├── middlewares/     # Auth, Error Handling
│   │   ├── routes/          # API Routes
│   │   ├── dtos/            # Data Transfer Objects
│   │   └── utils/           # Utilities
│   ├── prisma/              # Database Schema & Migrations
│   └── uploads/             # Static Files
│
└── docs/                    # Documentation
    ├── API_SPEC.md
    ├── IMPORT_PRODUCTS_GUIDE.md
    └── TEST_CASE.md
```

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy

### 1. Cài Đặt Server

```bash
cd server
npm install

# Cấu hình database
cp .env.example .env
# Chỉnh sửa DATABASE_URL trong .env

# Chạy migration và seed
npm run prisma:generate
npm run prisma:migrate
npm run prisma:seed

# Chạy server
npm run dev
```

Server sẽ chạy tại: `http://localhost:3000`

### 2. Cài Đặt Client

1. Mở `client/MyShopClient/MyShopClient.sln` bằng Visual Studio 2022
2. Restore NuGet packages
3. Chọn configuration: **x64 | Debug**
4. Chọn launch profile: **MyShopClient**
5. Nhấn F5 để chạy

### 3. Tài Khoản Mặc Định

| Username | Password | Role |
|----------|----------|------|
| admin | 123456 | ADMIN |
| sale1 | 123456 | SALE |
| sale2 | 123456 | SALE |

---

**© 2026 MyShop Team - Windows Programming Project**
