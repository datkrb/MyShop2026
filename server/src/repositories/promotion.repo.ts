import prisma from '../config/prisma';

interface PromotionFilters {
  page?: number;
  size?: number;
  isActive?: boolean;
  search?: string;
}

export class PromotionRepository {
  async findAll(filters: PromotionFilters = {}) {
    const { page = 1, size = 10, isActive, search } = filters;
    const skip = (page - 1) * size;

    const where: any = {};

    if (isActive !== undefined) {
      where.isActive = isActive;
    }

    if (search) {
      where.OR = [
        { code: { contains: search, mode: 'insensitive' } },
        { description: { contains: search, mode: 'insensitive' } },
      ];
    }

    const [data, total] = await Promise.all([
      prisma.promotion.findMany({
        where,
        skip,
        take: size,
        orderBy: { createdAt: 'desc' },
      }),
      prisma.promotion.count({ where }),
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
    return prisma.promotion.findUnique({
      where: { id },
    });
  }

  async findByCode(code: string) {
    return prisma.promotion.findUnique({
      where: { code: code.toUpperCase() },
    });
  }

  async create(data: {
    code: string;
    description?: string;
    discountType?: 'PERCENTAGE' | 'FIXED';
    discountValue: number;
    minOrderValue?: number;
    maxDiscount?: number;
    usageLimit?: number;
    startDate: Date;
    endDate: Date;
    isActive?: boolean;
  }) {
    return prisma.promotion.create({
      data: {
        code: data.code.toUpperCase(),
        description: data.description,
        discountType: data.discountType || 'PERCENTAGE',
        discountValue: data.discountValue,
        minOrderValue: data.minOrderValue,
        maxDiscount: data.maxDiscount,
        usageLimit: data.usageLimit,
        startDate: data.startDate,
        endDate: data.endDate,
        isActive: data.isActive ?? true,
      },
    });
  }

  async update(id: number, data: {
    code?: string;
    description?: string;
    discountType?: 'PERCENTAGE' | 'FIXED';
    discountValue?: number;
    minOrderValue?: number;
    maxDiscount?: number;
    usageLimit?: number;
    startDate?: Date;
    endDate?: Date;
    isActive?: boolean;
  }) {
    return prisma.promotion.update({
      where: { id },
      data: {
        ...(data.code && { code: data.code.toUpperCase() }),
        ...(data.description !== undefined && { description: data.description }),
        ...(data.discountType && { discountType: data.discountType }),
        ...(data.discountValue !== undefined && { discountValue: data.discountValue }),
        ...(data.minOrderValue !== undefined && { minOrderValue: data.minOrderValue }),
        ...(data.maxDiscount !== undefined && { maxDiscount: data.maxDiscount }),
        ...(data.usageLimit !== undefined && { usageLimit: data.usageLimit }),
        ...(data.startDate && { startDate: data.startDate }),
        ...(data.endDate && { endDate: data.endDate }),
        ...(data.isActive !== undefined && { isActive: data.isActive }),
      },
    });
  }

  async delete(id: number) {
    return prisma.promotion.delete({
      where: { id },
    });
  }

  async incrementUsedCount(id: number) {
    return prisma.promotion.update({
      where: { id },
      data: {
        usedCount: { increment: 1 },
      },
    });
  }
}

export default new PromotionRepository();
