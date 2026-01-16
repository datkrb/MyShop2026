import prisma from '../config/prisma';
import { OrderStatus } from '../constants/order-status';

interface OrderFilters {
  page?: number;
  size?: number;
  fromDate?: string;
  toDate?: string;
  status?: OrderStatus;
  createdById?: number;
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

  async findById(id: number) {
    return prisma.order.findUnique({
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
  }) {
    return prisma.order.create({
      data: {
        customerId: data.customerId,
        createdById: data.createdById,
        status: data.status || OrderStatus.PENDING,
        finalPrice: data.finalPrice,
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
  }) {
    if (data.items) {
      // Delete existing items and create new ones
      await prisma.orderItem.deleteMany({
        where: { orderId: id },
      });
    }

    return prisma.order.update({
      where: { id },
      data: {
        ...(data.customerId !== undefined && { customerId: data.customerId }),
        ...(data.status && { status: data.status }),
        ...(data.finalPrice !== undefined && { finalPrice: data.finalPrice }),
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

  async updateStatus(id: number, status: OrderStatus) {
    return prisma.order.update({
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

  async delete(id: number) {
    return prisma.order.delete({
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
}

export default new OrderRepository();

