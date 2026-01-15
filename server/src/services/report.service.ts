import prisma from "../config/prisma";
import { OrderStatus } from "../constants/order-status";

export class ReportService {
  async getRevenueReport(
    startDate: Date,
    endDate: Date,
    type: "day" | "month" | "year" = "day",
    categoryId?: number
  ) {
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
          ...(categoryId && {
            where: {
              product: {
                categoryId: categoryId,
              },
            },
          }),
        },
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

      // If categoryId is specified, sum only the revenue from items in that category
      if (categoryId) {
        const categoryRevenue = order.orderItems.reduce((sum, item) => sum + item.totalPrice, 0);
        revenueData[key] = (revenueData[key] || 0) + categoryRevenue;
      } else {
        revenueData[key] = (revenueData[key] || 0) + order.finalPrice;
      }
    });

    // Sort by date key
    const sortedData = Object.entries(revenueData)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([date, revenue]) => ({ date, revenue }));

    return sortedData;
  }

  async getProfitReport(startDate: Date, endDate: Date, categoryId?: number) {
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
          ...(categoryId && {
            where: {
              product: {
                categoryId: categoryId,
              },
            },
          }),
        },
      },
    });

    let totalRevenue = 0;
    let totalCost = 0;

    orders.forEach((order) => {
      if (categoryId) {
        // If filtered by category, prevent order-level revenue, count only items
        order.orderItems.forEach((item) => {
          totalRevenue += item.totalPrice; // Revenue for this item
          totalCost += item.quantity * item.product.importPrice;
        });
      } else {
        totalRevenue += order.finalPrice;
        order.orderItems.forEach((item) => {
          totalCost += item.quantity * item.product.importPrice;
        });
      }
    });

    const profit = totalRevenue - totalCost;
    const profitMargin = totalRevenue > 0 ? (profit / totalRevenue) * 100 : 0;

    return {
      revenue: totalRevenue,
      cost: totalCost,
      profit,
      profitMargin,
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

  async getTopProductsSalesTimeSeries(startDate: Date, endDate: Date, categoryId?: number) {
    // First, get top 5 products by total quantity sold
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
          // Filter by category if provided
          ...(categoryId && {
            where: {
              product: {
                categoryId: categoryId,
              },
            },
          }),
        },
      },
    });

    // Aggregate total quantity per product
    const productTotals: { [key: number]: { product: any; quantity: number } } = {};

    orders.forEach((order) => {
      order.orderItems.forEach((item) => {
        if (!productTotals[item.productId]) {
          productTotals[item.productId] = {
            product: item.product,
            quantity: 0,
          };
        }
        productTotals[item.productId].quantity += item.quantity;
      });
    });

    // Get top 5 products
    const topProducts = Object.values(productTotals)
      .sort((a, b) => b.quantity - a.quantity)
      .slice(0, 5);

    const topProductIds = topProducts.map((p) => p.product.id);

    // Calculate number of milestones (max 6 points for clean visualization)
    const totalDays = Math.ceil((endDate.getTime() - startDate.getTime()) / (1000 * 60 * 60 * 24)) + 1;
    const numMilestones = Math.min(6, totalDays);
    const daysPerMilestone = Math.ceil(totalDays / numMilestones);

    // Generate milestone dates
    const milestones: { start: Date; end: Date; label: string }[] = [];
    for (let i = 0; i < numMilestones; i++) {
      const milestoneStart = new Date(startDate);
      milestoneStart.setDate(milestoneStart.getDate() + i * daysPerMilestone);
      
      const milestoneEnd = new Date(startDate);
      milestoneEnd.setDate(milestoneEnd.getDate() + (i + 1) * daysPerMilestone - 1);
      
      // Ensure last milestone doesn't exceed endDate
      if (milestoneEnd > endDate) {
        milestoneEnd.setTime(endDate.getTime());
      }
      
      // Format label based on date range
      const startLabel = milestoneStart.toISOString().split("T")[0].slice(5); // MM-DD
      const endLabel = milestoneEnd.toISOString().split("T")[0].slice(5);
      const label = startLabel === endLabel ? startLabel : `${startLabel}`;
      
      milestones.push({ start: milestoneStart, end: milestoneEnd, label });
    }

    // Build time series data for each product
    const salesByProductAndMilestone: { [productId: number]: number[] } = {};
    topProductIds.forEach((id) => {
      salesByProductAndMilestone[id] = new Array(numMilestones).fill(0);
    });

    // Fill in actual sales data by milestone
    orders.forEach((order) => {
      const orderDate = new Date(order.createdTime);
      order.orderItems.forEach((item) => {
        if (!topProductIds.includes(item.productId)) return;
        
        // Find which milestone this order belongs to
        for (let i = 0; i < milestones.length; i++) {
          if (orderDate >= milestones[i].start && orderDate <= milestones[i].end) {
            salesByProductAndMilestone[item.productId][i] += item.quantity;
            break;
          }
        }
      });
    });

    // Format output
    const products = topProducts.map((p) => ({
      id: p.product.id,
      name: p.product.name,
    }));

    const dates = milestones.map((m) => m.label);

    const series = topProductIds.map((productId) => ({
      productId,
      data: salesByProductAndMilestone[productId],
    }));

    return {
      products,
      dates,
      series,
    };
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
