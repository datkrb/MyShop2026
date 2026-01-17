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
  // Advanced search filters
  stockStatus?: 'all' | 'inStock' | 'lowStock' | 'outOfStock';
  createdFrom?: string;
  createdTo?: string;
  categoryIds?: number[];
  skuSearch?: string;
  skuMode?: 'exact' | 'prefix' | 'contains';
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
      // Advanced search filters
      stockStatus,
      createdFrom,
      createdTo,
      categoryIds,
      skuSearch,
      skuMode = 'contains',
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

    // Single category (legacy support)
    if (categoryId) {
      where.categoryId = categoryId;
    }

    // Multiple categories filter (takes precedence over single categoryId)
    if (categoryIds && categoryIds.length > 0) {
      where.categoryId = { in: categoryIds };
    }

    if (id) {
      where.id = id;
    }

    // Stock status filter
    if (stockStatus && stockStatus !== 'all') {
      switch (stockStatus) {
        case 'inStock':
          where.stock = { gt: 10 };
          break;
        case 'lowStock':
          where.stock = { gt: 0, lte: 10 };
          break;
        case 'outOfStock':
          where.stock = { lte: 0 };
          break;
      }
    }

    // Date range filter
    if (createdFrom || createdTo) {
      where.createdAt = {};
      if (createdFrom) where.createdAt.gte = new Date(createdFrom);
      if (createdTo) where.createdAt.lte = new Date(createdTo);
    }

    // SKU search with mode
    if (skuSearch) {
      const skuCondition: any = { mode: 'insensitive' };
      switch (skuMode) {
        case 'exact':
          skuCondition.equals = skuSearch;
          break;
        case 'prefix':
          skuCondition.startsWith = skuSearch;
          break;
        case 'contains':
        default:
          skuCondition.contains = skuSearch;
          break;
      }
      where.sku = skuCondition;
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

  async addImages(data: { productId: number; url: string }[]) {
    return prisma.productImage.createMany({
      data,
    });
  }

  async findImageById(id: number) {
    return prisma.productImage.findUnique({
      where: { id },
    });
  }

  async deleteImage(id: number) {
    return prisma.productImage.delete({
      where: { id },
    });
  }

  async getStats() {
    const [totalProducts, totalCategories, lowStock, outOfStock] = await Promise.all([
      prisma.product.count(),
      prisma.category.count(),
      prisma.product.count({
        where: {
          stock: {
            gt: 0,
            lt: 10
          }
        }
      }),
      prisma.product.count({
        where: {
          stock: {
            lte: 0
          }
        }
      })
    ]);

    return {
      totalProducts,
      totalCategories,
      lowStock,
      outOfStock
    };
  }
}

export default new ProductRepository();

