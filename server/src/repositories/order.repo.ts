import prisma from '../config/prisma';
import { OrderStatus } from '../constants/order-status';

interface OrderFilters {
  page?: number;
  size?: number;
  fromDate?: string;
  toDate?: string;
  status?: OrderStatus;
  createdById?: number;
  // Advanced search filters
  customerId?: number;
  minAmount?: number;
  maxAmount?: number;
  keyword?: string;
}

export class OrderRepository {


  async findAll(filters: OrderFilters = {}) {
    const {
      page = 1,
      size = 10,
      fromDate,
      toDate,
      status,
      createdById,
      // Advanced search
      customerId,
      minAmount,
      maxAmount,
      keyword,
    } = filters;

    const skip = (page - 1) * size;

    const where: any = {};

    if (fromDate || toDate) {
      where.createdTime = {};
      if (fromDate) where.createdTime.gte = new Date(fromDate);
      if (toDate) where.createdTime.lte = new Date(toDate);
    }

    if (status) {
      where.status = status;
    }

    if (createdById) {
      where.createdById = createdById;
    }

    // Advanced search: Customer filter
    if (customerId) {
      where.customerId = customerId;
    }

    // Advanced search: Amount range
    if (minAmount || maxAmount) {
      where.finalPrice = {};
      if (minAmount) where.finalPrice.gte = minAmount;
      if (maxAmount) where.finalPrice.lte = maxAmount;
    }

    // Advanced search: Keyword (order ID or customer name)
    if (keyword) {
      const numericKeyword = parseInt(keyword);
      if (!isNaN(numericKeyword)) {
        where.OR = [
          { id: numericKeyword },
          { customer: { name: { contains: keyword, mode: 'insensitive' } } },
        ];
      } else {
        where.customer = { name: { contains: keyword, mode: 'insensitive' } };
      }
    }

    const [data, total] = await Promise.all([
      prisma.order.findMany({
        where,
        skip,
        take: size,
        orderBy: { createdTime: 'desc' },
        include: {
          customer: true,
          createdBy: {
            select: {
              id: true,
              username: true,
              role: true,
            },
          },
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
      }),
      prisma.order.count({ where }),
    ]);

    return {
      data,
      total,
      page,
      size,
      totalPages: Math.ceil(total / size),
    };
  }

  async findById(id: number, tx: any = prisma) {
    return tx.order.findUnique({
      where: { id },
      include: {
        customer: true,
        createdBy: {
          select: {
            id: true,
            username: true,
            role: true,
          },
        },
        orderItems: {
          include: {
            product: {
              include: {
                category: true,
                images: true,
              },
            },
          },
        },
      },
    });
  }

  async findRecent(limit: number = 3) {
    return prisma.order.findMany({
      take: limit,
      orderBy: { createdTime: 'desc' },
      include: {
        customer: true,
        orderItems: {
          include: {
            product: true,
          },
        },
      },
    });
  }

  async create(data: {
    customerId?: number;
    createdById: number;
    status?: OrderStatus;
    items: Array<{
      productId: number;
      quantity: number;
      unitSalePrice: number;
      totalPrice: number;
    }>;
    finalPrice: number;
    discountAmount?: number;
    promotionId?: number;
  }, tx: any = prisma) {
    return tx.order.create({
      data: {
        customerId: data.customerId,
        createdById: data.createdById,
        status: data.status || OrderStatus.PENDING,
        finalPrice: data.finalPrice,
        discountAmount: data.discountAmount || 0,
        promotionId: data.promotionId,
        orderItems: {
          create: data.items,
        },
      },
      include: {
        customer: true,
        createdBy: {
          select: {
            id: true,
            username: true,
            role: true,
          },
        },
        promotion: true,
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
  }

  async update(id: number, data: {
    customerId?: number;
    status?: OrderStatus;
    items?: Array<{
      productId: number;
      quantity: number;
      unitSalePrice: number;
      totalPrice: number;
    }>;
    finalPrice?: number;
    discountAmount?: number;
    promotionId?: number | null;
  }, tx: any = prisma) {
    if (data.items) {
      // Delete existing items and create new ones
      await tx.orderItem.deleteMany({
        where: { orderId: id },
      });
    }

    return tx.order.update({
      where: { id },
      data: {
        ...(data.customerId !== undefined && { customerId: data.customerId }),
        ...(data.status && { status: data.status }),
        ...(data.finalPrice !== undefined && { finalPrice: data.finalPrice }),
        ...(data.discountAmount !== undefined && { discountAmount: data.discountAmount }),
        ...(data.promotionId !== undefined && { promotionId: data.promotionId }),
        ...(data.items && {
          orderItems: {
            create: data.items,
          },
        }),
      },
      include: {
        customer: true,
        createdBy: {
          select: {
            id: true,
            username: true,
            role: true,
          },
        },
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
  }

  async updateStatus(id: number, status: OrderStatus, tx: any = prisma) {
    return tx.order.update({
      where: { id },
      data: { status },
      include: {
        customer: true,
        orderItems: {
          include: {
            product: true,
          },
        },
      },
    });
  }

  async delete(id: number, tx: any = prisma) {
    return tx.order.delete({
      where: { id },
    });
  }

  async getRevenueByDate(year: number, month?: number) {
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

    return prisma.order.groupBy({
      by: ['createdTime'],
      where,
      _sum: {
        finalPrice: true,
      },
      _count: {
        id: true,
      },
    });
  }

  async getRevenueByMonth(year: number) {
    const orders = await prisma.order.findMany({
      where: {
        status: OrderStatus.PAID,
        createdTime: {
          gte: new Date(year, 0, 1),
          lt: new Date(year + 1, 0, 1),
        },
      },
      select: {
        finalPrice: true,
        createdTime: true,
      },
    });

    const monthlyRevenue: { [key: number]: number } = {};
    for (let i = 0; i < 12; i++) {
      monthlyRevenue[i + 1] = 0;
    }

    orders.forEach((order) => {
      const month = order.createdTime.getMonth() + 1;
      monthlyRevenue[month] = (monthlyRevenue[month] || 0) + order.finalPrice;
    });

    return monthlyRevenue;
  }

  async getStats(userRole: string, userId?: number) {
    const where: any = {};
    
    // SALE role can only see their own orders
    if (userRole === 'SALE' && userId) {
      where.createdById = userId;
    }

    const [total, pending, paid] = await Promise.all([
      prisma.order.count({ where }),
      prisma.order.count({ where: { ...where, status: OrderStatus.PENDING } }),
      prisma.order.count({ where: { ...where, status: OrderStatus.PAID } })
    ]);

    return { total, pending, paid };
  }
}

export default new OrderRepository();

