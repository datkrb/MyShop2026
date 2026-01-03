import { PrismaClient, UserRole, OrderStatus } from '@prisma/client';
import * as bcrypt from 'bcryptjs';

const prisma = new PrismaClient();

async function main() {
  console.log('🌱 Start seeding database...');

  // =========================
  // 1. CLEAN DATABASE
  // =========================
  await prisma.orderItem.deleteMany();
  await prisma.productImage.deleteMany();
  await prisma.order.deleteMany();
  await prisma.product.deleteMany();
  await prisma.category.deleteMany();
  await prisma.customer.deleteMany();
  await prisma.user.deleteMany();

  // =========================
  // 2. CREATE USERS
  // =========================
  const hashedPassword = await bcrypt.hash('123456', 10);
  
  const admin = await prisma.user.create({
    data: {
      username: 'admin',
      password: hashedPassword,
      role: UserRole.ADMIN
    }
  });

  const sale1 = await prisma.user.create({
    data: {
      username: 'sale1',
      password: hashedPassword,
      role: UserRole.SALE
    }
  });

  const sale2 = await prisma.user.create({
    data: {
      username: 'sale2',
      password: hashedPassword,
      role: UserRole.SALE
    }
  });

  console.log('✅ Users created');

  // =========================
  // 3. CREATE CUSTOMERS
  // =========================
  const customers = await prisma.customer.createMany({
    data: [
      { name: 'Nguyễn Văn A', phone: '0901234567', email: 'nguyenvana@example.com', address: '123 Đường ABC, Quận 1, TP.HCM' },
      { name: 'Trần Thị B', phone: '0907654321', email: 'tranthib@example.com', address: '456 Đường XYZ, Quận 2, TP.HCM' },
      { name: 'Lê Văn C', phone: '0912345678', email: 'levanc@example.com', address: '789 Đường DEF, Quận 3, TP.HCM' },
      { name: 'Phạm Thị D', phone: '0923456789', email: 'phamthid@example.com', address: '321 Đường GHI, Quận 4, TP.HCM' },
      { name: 'Hoàng Văn E', phone: '0934567890', email: 'hoangvane@example.com', address: '654 Đường JKL, Quận 5, TP.HCM' }
    ]
  });

  const customerList = await prisma.customer.findMany();
  console.log('✅ Customers created');

  // =========================
  // 4. CREATE CATEGORIES
  // =========================
  const categories = await prisma.category.createMany({
    data: [
      { name: 'Beverages', description: 'Drinks and beverages' },
      { name: 'Snacks', description: 'Fast food and snacks' },
      { name: 'Household', description: 'Daily household products' }
    ]
  });

  const categoryList = await prisma.category.findMany();
  console.log('✅ Categories created');

  // =========================
  // 5. CREATE PRODUCTS
  // 3 categories × 22 products
  // =========================
  const productsData = [];

  for (const category of categoryList) {
    for (let i = 1; i <= 22; i++) {
      const importPrice = 5000 + i * 200;
      const salePrice = importPrice + 3000;
      const stock = 10 + (i % 5) * 5;

      productsData.push({
        sku: `${category.name.substring(0, 3).toUpperCase()}-${i
          .toString()
          .padStart(3, '0')}`,
        name: `${category.name} Product ${i}`,
        importPrice: importPrice,
        salePrice: salePrice,
        stock: stock,
        description: `Sample description for ${category.name} product ${i}`,
        categoryId: category.id
      });
    }
  }

  await prisma.product.createMany({
    data: productsData
  });

  const products = await prisma.product.findMany();
  console.log('✅ Products created');

  // =========================
  // 6. CREATE ORDERS
  // =========================
  const statuses = [OrderStatus.PAID, OrderStatus.PENDING, OrderStatus.DRAFT];
  const users = [admin, sale1, sale2];

  for (let o = 1; o <= 10; o++) {
    const orderItems = [];
    let finalPrice = 0;

    const selectedProducts = products
      .sort(() => 0.5 - Math.random())
      .slice(0, 3);

    const randomCustomer = customerList[Math.floor(Math.random() * customerList.length)];
    const randomUser = users[Math.floor(Math.random() * users.length)];
    const randomStatus = statuses[Math.floor(Math.random() * statuses.length)];

    for (const product of selectedProducts) {
      const quantity = Math.floor(Math.random() * 3) + 1;
      const unitSalePrice = product.salePrice;
      const totalPrice = quantity * unitSalePrice;

      finalPrice += totalPrice;

      orderItems.push({
        productId: product.id,
        quantity,
        unitSalePrice,
        totalPrice
      });
    }

    await prisma.order.create({
      data: {
        finalPrice,
        status: randomStatus,
        customerId: randomCustomer.id,
        createdById: randomUser.id,
        orderItems: {
          create: orderItems
        }
      }
    });
  }

  console.log('✅ Orders created');
  console.log('✅ Database seeding completed.');
}

main()
  .catch((e) => {
    console.error('❌ Seeding error:', e);
    process.exit(1);
  })
  .finally(async () => {
    await prisma.$disconnect();
  });
