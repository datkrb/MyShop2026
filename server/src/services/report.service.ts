import orderRepo from '../repositories/order.repo';
import prisma from '../config/prisma';
import { OrderStatus } from '../constants/order-status';

export class ReportService {
  async getRevenueReport(type: 'day' | 'month' | 'year', year: number, month?: number) {
    if (type === 'month' && month) {
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
      orders.forEach((order) => {
        const day = order.createdTime.getDate();
        dailyRevenue[day] = (dailyRevenue[day] || 0) + order.finalPrice;
      });

      return dailyRevenue;
    } else if (type === 'year') {
      return orderRepo.getRevenueByMonth(year);
    }

    throw new Error('Invalid report type');
  }

  async getProfitReport(year: number, month?: number) {
    const where: any = {
      status: OrderStatus.PAID,
    };

    if (month) {
      where.createdTime = {
        gte: new Date(year, month - 1, 1),
        lt: new Date(year, month, 1),
      };
    } else {
      where.createdTime = {
        gte: new Date(year, 0, 1),
        lt: new Date(year + 1, 0, 1),
      };
    }

    const orders = await prisma.order.findMany({
      where,
      include: {
        orderItems: {
          include: {
            product: true,
          },
        },
      },
    });

    let totalRevenue = 0;
    let totalCost = 0;

    orders.forEach((order) => {
      totalRevenue += order.finalPrice;
      order.orderItems.forEach((item) => {
        totalCost += item.quantity * item.product.importPrice;
      });
    });

    return {
      revenue: totalRevenue,
      cost: totalCost,
      profit: totalRevenue - totalCost,
      profitMargin: totalRevenue > 0 ? ((totalRevenue - totalCost) / totalRevenue) * 100 : 0,
    };
  }

  async getProductSalesReport(year: number, month?: number) {
    const where: any = {
      status: OrderStatus.PAID,
    };

    if (month) {
      where.createdTime = {
        gte: new Date(year, month - 1, 1),
        lt: new Date(year, month, 1),
      };
    } else {
      where.createdTime = {
        gte: new Date(year, 0, 1),
        lt: new Date(year + 1, 0, 1),
      };
    }

    const orders = await prisma.order.findMany({
      where,
      include: {
        orderItems: {
          include: {
            product: {
              include: {
                category: true,
              },
            },
          },
        },
      },
    });

    const productSales: { [key: number]: { product: any; quantity: number; revenue: number } } = {};

    orders.forEach((order) => {
      order.orderItems.forEach((item) => {
        if (!productSales[item.productId]) {
          productSales[item.productId] = {
            product: item.product,
            quantity: 0,
            revenue: 0,
          };
        }
        productSales[item.productId].quantity += item.quantity;
        productSales[item.productId].revenue += item.totalPrice;
      });
    });

    return Object.values(productSales).sort((a, b) => b.quantity - a.quantity);
  }

  async getKPISalesReport(year: number, month?: number) {
    const where: any = {
      status: OrderStatus.PAID,
    };

    if (month) {
      where.createdTime = {
        gte: new Date(year, month - 1, 1),
        lt: new Date(year, month, 1),
      };
    } else {
      where.createdTime = {
        gte: new Date(year, 0, 1),
        lt: new Date(year + 1, 0, 1),
      };
    }

    const orders = await prisma.order.findMany({
      where,
      include: {
        createdBy: {
          select: {
            id: true,
            username: true,
            role: true,
          },
        },
        orderItems: true,
      },
    });

    const salesData: { [key: number]: { user: any; orders: number; revenue: number; commission: number } } = {};

    orders.forEach((order) => {
      if (!order.createdById) return;

      if (!salesData[order.createdById]) {
        salesData[order.createdById] = {
          user: order.createdBy,
          orders: 0,
          revenue: 0,
          commission: 0,
        };
      }

      salesData[order.createdById].orders += 1;
      salesData[order.createdById].revenue += order.finalPrice;
      // Commission: 5% of revenue
      salesData[order.createdById].commission += order.finalPrice * 0.05;
    });

    return Object.values(salesData).sort((a, b) => b.revenue - a.revenue);
  }
}

export default new ReportService();

