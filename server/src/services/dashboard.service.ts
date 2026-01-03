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

    const [totalProducts, ordersToday, revenueToday] = await Promise.all([
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
    ]);

    return {
      totalProducts,
      totalOrdersToday: ordersToday,
      revenueToday: revenueToday._sum.finalPrice || 0,
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
}

export default new DashboardService();

