import promotionRepo from '../repositories/promotion.repo';

export class PromotionService {
  async getAll(filters: any) {
    return promotionRepo.findAll(filters);
  }

  async getById(id: number) {
    const promotion = await promotionRepo.findById(id);
    if (!promotion) {
      throw new Error('Promotion not found');
    }
    return promotion;
  }

  async getByCode(code: string) {
    return promotionRepo.findByCode(code);
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
    // Check if code already exists
    const existing = await promotionRepo.findByCode(data.code);
    if (existing) {
      throw new Error('Promotion code already exists');
    }

    return promotionRepo.create(data);
  }

  async update(id: number, data: any) {
    const promotion = await promotionRepo.findById(id);
    if (!promotion) {
      throw new Error('Promotion not found');
    }

    // If changing code, check if new code exists
    if (data.code && data.code !== promotion.code) {
      const existing = await promotionRepo.findByCode(data.code);
      if (existing) {
        throw new Error('Promotion code already exists');
      }
    }

    return promotionRepo.update(id, data);
  }

  async delete(id: number) {
    const promotion = await promotionRepo.findById(id);
    if (!promotion) {
      throw new Error('Promotion not found');
    }
    return promotionRepo.delete(id);
  }

  /**
   * Validate a promotion code for an order
   */
  async validatePromotion(code: string, orderValue: number): Promise<{
    valid: boolean;
    promotion?: any;
    discountAmount?: number;
    message?: string;
  }> {
    const promotion = await promotionRepo.findByCode(code);

    if (!promotion) {
      return { valid: false, message: 'Invalid promotion code' };
    }

    if (!promotion.isActive) {
      return { valid: false, message: 'Promotion is not active' };
    }

    const now = new Date();
    if (now < promotion.startDate) {
      return { valid: false, message: 'Promotion has not started yet' };
    }

    if (now > promotion.endDate) {
      return { valid: false, message: 'Promotion has expired' };
    }

    if (promotion.usageLimit && promotion.usedCount >= promotion.usageLimit) {
      return { valid: false, message: 'Promotion usage limit reached' };
    }

    if (promotion.minOrderValue && orderValue < promotion.minOrderValue) {
      return {
        valid: false,
        message: `Minimum order value is ${promotion.minOrderValue}đ`,
      };
    }

    // Calculate discount
    const discountAmount = this.calculateDiscount(promotion, orderValue);

    return {
      valid: true,
      promotion,
      discountAmount,
    };
  }

  /**
   * Calculate discount amount for a promotion
   */
  calculateDiscount(promotion: any, orderValue: number): number {
    let discount = 0;

    if (promotion.discountType === 'PERCENTAGE') {
      discount = Math.floor(orderValue * promotion.discountValue / 100);
      // Apply max discount cap if set
      if (promotion.maxDiscount && discount > promotion.maxDiscount) {
        discount = promotion.maxDiscount;
      }
    } else {
      // FIXED discount
      discount = promotion.discountValue;
    }

    // Discount cannot exceed order value
    if (discount > orderValue) {
      discount = orderValue;
    }

    return discount;
  }

  /**
   * Apply promotion to an order (increment usage count)
   */
  async applyPromotion(id: number) {
    return promotionRepo.incrementUsedCount(id);
  }
}

export default new PromotionService();
