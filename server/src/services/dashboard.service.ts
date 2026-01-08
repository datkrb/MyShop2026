import orderRepo from '../repositories/order.repo';
import productRepo from '../repositories/product.repo';
import prisma from '../config/prisma';
import { OrderStatus } from '../constants/order-status';

export class DashboardService {
  async getSummary() {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    
    const yesterday = new Date(today);
    yesterday.setDate(yesterday.getDate() - 1);

    const [
      totalProducts,
      ordersToday,
      revenueToday,
      pendingOrders,
      ordersYesterday,
      revenueYesterday,
      productsAddedToday,
      productsAddedYesterday,
    ] = await Promise.all([
      prisma.product.count(),
      prisma.order.count({
        where: {
          createdTime: {
            gte: today,
            lt: tomorrow,
          },
        },
      }),
      prisma.order.aggregate({
        where: {
          status: OrderStatus.PAID,
          createdTime: {
            gte: today,
            lt: tomorrow,
          },
        },
        _sum: {
          finalPrice: true,
        },
      }),
      prisma.order.count({
        where: {
          status: OrderStatus.PENDING,
        },
      }),
      // Yesterday's orders
      prisma.order.count({
        where: {
          createdTime: {
            gte: yesterday,
            lt: today,
          },
        },
      }),
      // Yesterday's revenue
      prisma.order.aggregate({
        where: {
          status: OrderStatus.PAID,
          createdTime: {
            gte: yesterday,
            lt: today,
          },
        },
        _sum: {
          finalPrice: true,
        },
      }),
      // Products added today
      prisma.product.count({
        where: {
          createdAt: {
            gte: today,
            lt: tomorrow,
          },
        },
      }),
      // Products added yesterday
      prisma.product.count({
        where: {
          createdAt: {
            gte: yesterday,
            lt: today,
          },
        },
      }),
    ]);

    const revenueTodayValue = revenueToday._sum.finalPrice || 0;
    const revenueYesterdayValue = revenueYesterday._sum.finalPrice || 0;

    // Calculate percentage changes
    const calculateChange = (current: number, previous: number): number => {
      if (previous === 0) return current > 0 ? 100 : 0;
      return ((current - previous) / previous) * 100;
    };

    return {
      totalProducts,
      totalOrdersToday: ordersToday,
      revenueToday: revenueTodayValue,
      pendingOrders,
      // Percentage changes
      productsChange: calculateChange(productsAddedToday, productsAddedYesterday),
      ordersChange: calculateChange(ordersToday, ordersYesterday),
      revenueChange: calculateChange(revenueTodayValue, revenueYesterdayValue),
    };
  }

  async getLowStock(limit: number = 5) {
    return productRepo.findLowStock(limit);
  }

  async getTopSelling(limit: number = 5) {
    return productRepo.findTopSelling(limit);
  }

  async getRevenueChart() {
    const now = new Date();
    const year = now.getFullYear();
    const month = now.getMonth() + 1;

    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: new Date(year, month - 1, 1),
          lt: new Date(year, month, 1),
        },
      },
      select: {
        finalPrice: true,
        createdTime: true,
      },
    });

    const dailyRevenue: { [key: number]: number } = {};
    const daysInMonth = new Date(year, month, 0).getDate();

    for (let day = 1; day <= daysInMonth; day++) {
      dailyRevenue[day] = 0;
    }

    orders.forEach((order) => {
      const day = order.createdTime.getDate();
      dailyRevenue[day] = (dailyRevenue[day] || 0) + order.finalPrice;
    });

    return dailyRevenue;
  }

  async getRecentOrders(limit: number = 3) {
    return orderRepo.findRecent(limit);
  }

  async getCategoryStats() {
    const categories = await prisma.category.findMany({
      select: {
        id: true,
        name: true,
        _count: {
          select: {
            products: true,
          },
        },
      },
      orderBy: {
        products: {
          _count: 'desc',
        },
      },
    });

    return categories.map((cat) => ({
      id: cat.id,
      name: cat.name,
      productCount: cat._count.products,
    }));
  }
}

export default new DashboardService();

