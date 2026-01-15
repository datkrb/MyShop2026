import prisma from "../config/prisma";
import { OrderStatus } from "../constants/order-status";

export class ReportService {
  async getRevenueReport(
    startDate: Date,
    endDate: Date,
    type: "day" | "month" | "year" = "day"
  ) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
      },
      select: {
        finalPrice: true,
        createdTime: true,
      },
    });

    console.log(`[RevenueReport] Range: ${startDate.toISOString()} - ${endDate.toISOString()}`);
    console.log(`[RevenueReport] Found ${orders.length} orders.`);

    const revenueData: { [key: string]: number } = {};

    orders.forEach((order) => {
      let key = "";
      const date = new Date(order.createdTime);

      if (type === "day") {
        key = date.toISOString().split("T")[0]; // YYYY-MM-DD
      } else if (type === "month") {
        key = `${date.getFullYear()}-${(date.getMonth() + 1)
          .toString()
          .padStart(2, "0")}`; // YYYY-MM
      } else if (type === "year") {
        key = `${date.getFullYear()}`; // YYYY
      }

      revenueData[key] = (revenueData[key] || 0) + order.finalPrice;
    });

    // Sort by date key
    const sortedData = Object.entries(revenueData)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([date, revenue]) => ({ date, revenue }));

    return sortedData;
  }

  async getProfitReport(startDate: Date, endDate: Date) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
      },
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
      profitMargin:
        totalRevenue > 0
          ? ((totalRevenue - totalCost) / totalRevenue) * 100
          : 0,
    };
  }

  async getProductSalesReport(startDate: Date, endDate: Date) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
      },
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

    const productSales: {
      [key: number]: { product: any; quantity: number; revenue: number };
    } = {};

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

    const salesData: {
      [key: number]: {
        user: any;
        orders: number;
        revenue: number;
        commission: number;
      };
    } = {};

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
