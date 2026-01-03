import prisma from '../config/prisma';

interface CustomerFilters {
  page?: number;
  size?: number;
  keyword?: string;
}

export class CustomerRepository {
  async findAll(filters: CustomerFilters = {}) {
    const { page = 1, size = 10, keyword } = filters;
    const skip = (page - 1) * size;

    const where: any = {};

    if (keyword) {
      where.OR = [
        { name: { contains: keyword, mode: 'insensitive' } },
        { phone: { contains: keyword, mode: 'insensitive' } },
        { email: { contains: keyword, mode: 'insensitive' } },
      ];
    }

    const [data, total] = await Promise.all([
      prisma.customer.findMany({
        where,
        skip,
        take: size,
        orderBy: { name: 'asc' },
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
}

export default new CustomerRepository();

