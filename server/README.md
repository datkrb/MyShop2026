# MyShop API Server

API server for MyShop management system built with Express, TypeScript, and Prisma.

## Features

- 🔐 JWT Authentication
- 👥 Role-based access control (ADMIN, SALE)
- 📦 Product management
- 🛒 Order management
- 👤 Customer management
- 📊 Dashboard & Reports
- 🗄️ PostgreSQL database with Prisma ORM

## Prerequisites

- Node.js 20+
- PostgreSQL 15+
- npm or yarn

## Installation

1. Clone the repository
2. Install dependencies:

```bash
npm install
```

3. Set up environment variables:

```bash
cp .env.example .env
# Edit .env with your database credentials
```

4. Set up the database:

```bash
# Generate Prisma Client
npm run prisma:generate

# Run migrations
npm run prisma:migrate

# Seed the database
npm run prisma:seed
```

## Running the Application

### Development

```bash
npm run dev
```

### Production

```bash
npm run build
npm start
```

## Docker

### Using Docker Compose

```bash
cd docker
docker-compose up -d
```

### Build Docker image

```bash
docker build -f docker/Dockerfile -t myshop-api .
```

## API Documentation

Base URL: `http://localhost:3000/api`

### Authentication

- `POST /api/auth/login` - Login
- `GET /api/auth/me` - Get current user
- `POST /api/auth/logout` - Logout

### Health Check

- `GET /api/health` - Health check

### Dashboard

- `GET /api/dashboard/summary` - Summary statistics
- `GET /api/dashboard/low-stock` - Low stock products
- `GET /api/dashboard/top-selling` - Top selling products
- `GET /api/dashboard/revenue-chart` - Revenue chart data
- `GET /api/dashboard/recent-orders` - Recent orders

### Categories

- `GET /api/categories` - List categories
- `POST /api/categories` - Create category
- `PUT /api/categories/:id` - Update category
- `DELETE /api/categories/:id` - Delete category

### Products

- `GET /api/products` - List products (with filters)
- `GET /api/products/:id` - Get product detail
- `POST /api/products` - Create product
- `PUT /api/products/:id` - Update product
- `DELETE /api/products/:id` - Delete product

### Customers

- `GET /api/customers` - List customers
- `GET /api/customers/:id` - Get customer detail
- `POST /api/customers` - Create customer
- `PUT /api/customers/:id` - Update customer
- `DELETE /api/customers/:id` - Delete customer

### Orders

- `GET /api/orders` - List orders
- `GET /api/orders/:id` - Get order detail
- `POST /api/orders` - Create order
- `PUT /api/orders/:id` - Update order
- `DELETE /api/orders/:id` - Delete order
- `PUT /api/orders/:id/status` - Update order status
- `POST /api/orders/:id/autosave` - Autosave order

### Reports

- `GET /api/reports/revenue` - Revenue report
- `GET /api/reports/profit` - Profit report
- `GET /api/reports/products` - Product sales report
- `GET /api/reports/kpi-sales` - KPI & Commission report

### Admin

- `POST /api/admin/backup` - Backup database
- `POST /api/admin/restore` - Restore database

## Default Users

After seeding:

- **Admin**: username: `admin`, password: `123456`
- **Sale 1**: username: `sale1`, password: `123456`
- **Sale 2**: username: `sale2`, password: `123456`

## Role Permissions

### ADMIN

- Full system access
- Can view import prices
- Can perform backup/restore
- Can manage all orders

### SALE

- Cannot view import prices
- Can only view/manage their own orders
- Limited report access

## License

ISC
