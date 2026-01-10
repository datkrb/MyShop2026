import prisma from '../config/prisma';

interface ProductFilters {
  page?: number;
  size?: number;
  sort?: string;
  minPrice?: number;
  maxPrice?: number;
  keyword?: string;
  categoryId?: number;
  id?: number;
}

export class ProductRepository {
  async findAll(filters: ProductFilters = {}) {
    const {
      page = 1,
      size = 10,
      sort = 'id,desc',
      minPrice,
      maxPrice,
      keyword,
      categoryId,
      id,
    } = filters;

    const skip = (page - 1) * size;
    const [sortField, sortOrder] = sort.split(',');

    const where: any = {};

    if (minPrice || maxPrice) {
      where.salePrice = {};
      if (minPrice) where.salePrice.gte = minPrice;
      if (maxPrice) where.salePrice.lte = maxPrice;
    }

    if (keyword) {
      where.OR = [
        { name: { contains: keyword, mode: 'insensitive' } },
        { sku: { contains: keyword, mode: 'insensitive' } },
        { description: { contains: keyword, mode: 'insensitive' } },
      ];
    }

    if (categoryId) {
      where.categoryId = categoryId;
    }

    if (id) {
      where.id = id;
    }

    const [data, total] = await Promise.all([
      prisma.product.findMany({
        where,
        skip,
        take: size,
        orderBy: {
          [sortField]: sortOrder as 'asc' | 'desc',
        },
        include: {
          category: true,
          images: true,
        },
      }),
      prisma.product.count({ where }),
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
    return prisma.product.findUnique({
      where: { id },
      include: {
        category: true,
        images: true,
      },
    });
  }

  async findBySku(sku: string) {
    return prisma.product.findUnique({
      where: { sku },
    });
  }

  async findLowStock(limit: number = 5) {
    return prisma.product.findMany({
      where: {
        stock: {
          lte: 10,
        },
      },
      orderBy: {
        stock: 'asc',
      },
      take: limit,
      include: {
        category: true,
      },
    });
  }

  async findTopSelling(limit: number = 5) {
    const products = await prisma.product.findMany({
      include: {
        category: true,
        orderItems: {
          where: {
            order: {
              status: 'PAID',
            },
          },
        },
      },
    });

    // Calculate total quantity sold for each product
    const productsWithSales = products.map((product) => {
      const totalSold = product.orderItems.reduce((sum, item) => {
        return sum + item.quantity;
      }, 0);

      return {
        ...product,
        totalSold,
      };
    });

    // Sort by total sold and return top N
    return productsWithSales
      .sort((a, b) => b.totalSold - a.totalSold)
      .slice(0, limit)
      .map(({ totalSold, ...product }) => product);
  }

  async create(data: {
    name: string;
    sku: string;
    importPrice: number;
    salePrice: number;
    stock: number;
    categoryId: number;
    description?: string;
  }) {
    return prisma.product.create({
      data,
      include: {
        category: true,
      },
    });
  }

  async update(id: number, data: {
    name?: string;
    sku?: string;
    importPrice?: number;
    salePrice?: number;
    stock?: number;
    categoryId?: number;
    description?: string;
  }) {
    return prisma.product.update({
      where: { id },
      data,
      include: {
        category: true,
        images: true,
      },
    });
  }

  async delete(id: number) {
    return prisma.product.delete({
      where: { id },
    });
  }

  async updateStock(id: number, quantity: number) {
    return prisma.product.update({
      where: { id },
      data: {
        stock: {
          decrement: quantity,
        },
      },
    });
  }
}

export default new ProductRepository();

