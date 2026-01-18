import prisma from '../config/prisma';

interface CustomerFilters {
  page?: number;
  size?: number;
  keyword?: string;
  // Advanced search filters
  hasOrders?: boolean;
  createdFrom?: string;
  createdTo?: string;
  sort?: string;
}

export class CustomerRepository {
  async findAll(filters: CustomerFilters = {}) {
    const { page = 1, size = 10, keyword, hasOrders, createdFrom, createdTo, sort = 'name,asc' } = filters;
    const skip = (page - 1) * size;
    const [sortField, sortOrder] = sort.split(',');

    const where: any = {};

    if (keyword) {
      where.OR = [
        { name: { contains: keyword, mode: 'insensitive' } },
        { phone: { contains: keyword, mode: 'insensitive' } },
        { email: { contains: keyword, mode: 'insensitive' } },
      ];
    }

    // Has orders filter
    if (hasOrders !== undefined) {
      if (hasOrders) {
        where.orders = { some: {} };
      } else {
        where.orders = { none: {} };
      }
    }

    // Date range filter
    if (createdFrom || createdTo) {
      where.createdAt = {};
      if (createdFrom) where.createdAt.gte = new Date(createdFrom);
      if (createdTo) where.createdAt.lte = new Date(createdTo);
    }

    const [data, total] = await Promise.all([
      prisma.customer.findMany({
        where,
        skip,
        take: size,
        orderBy: { [sortField]: sortOrder as 'asc' | 'desc' },
      }),
      prisma.customer.count({ where }),
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
    return prisma.customer.findUnique({
      where: { id },
      include: {
        orders: {
          take: 10,
          orderBy: { createdTime: 'desc' },
        },
      },
    });
  }

  async create(data: {
    name: string;
    phone?: string;
    email?: string;
    address?: string;
  }) {
    return prisma.customer.create({
      data,
    });
  }

  async update(id: number, data: {
    name?: string;
    phone?: string;
    email?: string;
    address?: string;
  }) {
    return prisma.customer.update({
      where: { id },
      data,
    });
  }

  async delete(id: number) {
    return prisma.customer.delete({
      where: { id },
    });
  }

  async getStats() {
    const now = new Date();
    
    // Start of current month
    const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1);
    
    // Start of current week (Monday)
    const dayOfWeek = now.getDay();
    const diffToMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1;
    const startOfWeek = new Date(now);
    startOfWeek.setDate(now.getDate() - diffToMonday);
    startOfWeek.setHours(0, 0, 0, 0);

    const [total, newThisMonth, newThisWeek] = await Promise.all([
      prisma.customer.count(),
      prisma.customer.count({
        where: {
          createdAt: { gte: startOfMonth }
        }
      }),
      prisma.customer.count({
        where: {
          createdAt: { gte: startOfWeek }
        }
      })
    ]);

    return { total, newThisMonth, newThisWeek };
  }
}

export default new CustomerRepository();

