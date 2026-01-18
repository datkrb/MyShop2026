import prisma from "../config/prisma";
import { OrderStatus } from "../constants/order-status";

export class ReportService {
  async getRevenueReport(
    startDate: Date,
    endDate: Date,
    type: "day" | "month" | "year" = "day",
    categoryId?: number,
    createdById?: number
  ) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

  async getProfitReport(startDate: Date, endDate: Date, categoryId?: number, createdById?: number) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

  async getProductSalesReport(startDate: Date, endDate: Date, createdById?: number) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

  async getTopProductsSalesTimeSeries(startDate: Date, endDate: Date, categoryId?: number, createdById?: number) {
    // First, get top 5 products by total quantity sold
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

  async getKPISalesReport(year: number, month?: number, userId?: number) {
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

    // If userId is provided, filter orders by that specific user (for SALE viewing own KPI)
    if (userId) {
      where.createdById = userId;
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
        commissionRate: number;
      };
    } = {};

    // Aggregate orders and revenue per user
    orders.forEach((order) => {
      if (!order.createdById) return;

      if (!salesData[order.createdById]) {
        salesData[order.createdById] = {
          user: order.createdBy,
          orders: 0,
          revenue: 0,
          commission: 0,
          commissionRate: 0,
        };
      }

      salesData[order.createdById].orders += 1;
      salesData[order.createdById].revenue += order.finalPrice;
    });

    // Calculate tiered commission based on total revenue
    const calculateTieredCommission = (revenue: number): { commission: number; rate: number } => {
      if (revenue >= 50000000) {
        return { commission: revenue * 0.07, rate: 7 }; // 7% for 50M+
      } else if (revenue >= 10000000) {
        return { commission: revenue * 0.05, rate: 5 }; // 5% for 10M-50M
      } else {
        return { commission: revenue * 0.03, rate: 3 }; // 3% for below 10M
      }
    };

    // Apply tiered commission to each sales person
    Object.values(salesData).forEach((data) => {
      const { commission, rate } = calculateTieredCommission(data.revenue);
      data.commission = commission;
      data.commissionRate = rate;
    });

    return Object.values(salesData).sort((a, b) => b.revenue - a.revenue);
  }

  /**
   * Get profit time series data (profit by day/month/year)
   */
  async getProfitTimeSeries(
    startDate: Date,
    endDate: Date,
    type: "day" | "month" | "year" = "day",
    categoryId?: number,
    createdById?: number
  ) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

    const profitData: { [key: string]: { revenue: number; cost: number } } = {};

    orders.forEach((order) => {
      let key = "";
      const date = new Date(order.createdTime);

      if (type === "day") {
        key = date.toISOString().split("T")[0];
      } else if (type === "month") {
        key = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, "0")}`;
      } else if (type === "year") {
        key = `${date.getFullYear()}`;
      }

      if (!profitData[key]) {
        profitData[key] = { revenue: 0, cost: 0 };
      }

      if (categoryId) {
        order.orderItems.forEach((item) => {
          profitData[key].revenue += item.totalPrice;
          profitData[key].cost += item.quantity * item.product.importPrice;
        });
      } else {
        profitData[key].revenue += order.finalPrice;
        order.orderItems.forEach((item) => {
          profitData[key].cost += item.quantity * item.product.importPrice;
        });
      }
    });

    const sortedData = Object.entries(profitData)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([date, data]) => ({
        date,
        revenue: data.revenue,
        cost: data.cost,
        profit: data.revenue - data.cost,
      }));

    return sortedData;
  }

  /**
   * Get sales time series for a specific product
   */
  async getProductSalesById(
    startDate: Date,
    endDate: Date,
    productId: number,
    type: "day" | "month" = "day",
    createdById?: number
  ) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
        orderItems: {
          some: {
            productId: productId,
          },
        },
      },
      include: {
        orderItems: {
          where: {
            productId: productId,
          },
          include: {
            product: true,
          },
        },
      },
    });

    const salesData: { [key: string]: { quantity: number; revenue: number } } = {};

    orders.forEach((order) => {
      let key = "";
      const date = new Date(order.createdTime);

      if (type === "day") {
        key = date.toISOString().split("T")[0];
      } else {
        key = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, "0")}`;
      }

      if (!salesData[key]) {
        salesData[key] = { quantity: 0, revenue: 0 };
      }

      order.orderItems.forEach((item) => {
        salesData[key].quantity += item.quantity;
        salesData[key].revenue += item.totalPrice;
      });
    });

    const sortedData = Object.entries(salesData)
      .sort((a, b) => a[0].localeCompare(b[0]))
      .map(([date, data]) => ({
        date,
        quantity: data.quantity,
        revenue: data.revenue,
      }));

    return sortedData;
  }

  /**
   * Get sales time series grouped by category
   */
  async getCategorySalesTimeSeries(
    startDate: Date,
    endDate: Date,
    type: "day" | "month" = "day",
    createdById?: number
  ) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: startDate,
          lte: endDate,
        },
        ...(createdById && { createdById }),
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

    // Get all categories
    const categories = await prisma.category.findMany({
      select: { id: true, name: true },
    });

    // Build date keys
    const dateKeys = new Set<string>();
    orders.forEach((order) => {
      const date = new Date(order.createdTime);
      let key = "";
      if (type === "day") {
        key = date.toISOString().split("T")[0];
      } else {
        key = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, "0")}`;
      }
      dateKeys.add(key);
    });

    const sortedDates = Array.from(dateKeys).sort();

    // Initialize data structure
    const categoryData: { [categoryId: number]: { [date: string]: number } } = {};
    categories.forEach((cat) => {
      categoryData[cat.id] = {};
      sortedDates.forEach((date) => {
        categoryData[cat.id][date] = 0;
      });
    });

    // Fill in sales data
    orders.forEach((order) => {
      const date = new Date(order.createdTime);
      let key = "";
      if (type === "day") {
        key = date.toISOString().split("T")[0];
      } else {
        key = `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, "0")}`;
      }

      order.orderItems.forEach((item) => {
        const categoryId = item.product.categoryId;
        if (categoryData[categoryId]) {
          categoryData[categoryId][key] += item.quantity;
        }
      });
    });

    // Format output
    const series = categories.map((cat) => ({
      categoryId: cat.id,
      categoryName: cat.name,
      data: sortedDates.map((date) => categoryData[cat.id][date] || 0),
    }));

    return {
      dates: sortedDates,
      series: series.filter((s) => s.data.some((v) => v > 0)), // Only include categories with data
    };
  }
}

export default new ReportService();
