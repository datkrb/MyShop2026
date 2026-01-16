import prisma from '../config/prisma';
import orderRepo from '../repositories/order.repo';
import productRepo from '../repositories/product.repo';
import { OrderStatus } from '../constants/order-status';
import { Messages } from '../constants/messages';

export class OrderService {
  async getAll(filters: any, userRole: string, userId?: number) {
    // SALE role can only see their own orders
    if (userRole === 'SALE' && userId) {
      filters.createdById = userId;
    }

    return orderRepo.findAll(filters);
  }



  async getById(id: number, userRole: string, userId?: number) {
    const order = await orderRepo.findById(id);

    if (!order) {
      throw new Error(Messages.ORDER_NOT_FOUND);
    }

    // SALE role can only see their own orders
    if (userRole === 'SALE' && userId && order.createdById !== userId) {
      throw new Error(Messages.FORBIDDEN);
    }

    return order;
  }

  async create(data: {
    customerId?: number;
    items: Array<{ productId: number; quantity: number }>;
    promotionCode?: string;
  }, userId: number) {
    const orderItems = [];
    let finalPrice = 0;
    let discountAmount = 0;
    let promotionId: number | undefined;

    for (const item of data.items) {
      const product = await productRepo.findById(item.productId);

      if (!product) {
        throw new Error(Messages.PRODUCT_NOT_FOUND);
      }

      if (product.stock < item.quantity) {
        throw new Error(`${Messages.PRODUCT_OUT_OF_STOCK}: ${product.name}`);
      }

      const unitSalePrice = product.salePrice;
      const totalPrice = item.quantity * unitSalePrice;

      orderItems.push({
        productId: product.id,
        quantity: item.quantity,
        unitSalePrice,
        totalPrice,
      });

      finalPrice += totalPrice;
    }

    // Apply promotion if code is provided
    if (data.promotionCode) {
      const importPromotionService = (await import('./promotion.service')).default; // Dynamic import to avoid circular dependency
      const promoResult = await importPromotionService.validatePromotion(data.promotionCode, finalPrice);
      
      if (promoResult.valid && promoResult.discountAmount !== undefined) {
        discountAmount = promoResult.discountAmount;
        promotionId = promoResult.promotion.id;
        finalPrice -= discountAmount;
        
        // Increment usage count
        await importPromotionService.applyPromotion(promoResult.promotion.id);
      } else {
        throw new Error(promoResult.message || 'Invalid promotion code');
      }
    }

    const order = await orderRepo.create({
      customerId: data.customerId,
      createdById: userId,
      status: OrderStatus.PENDING,
      items: orderItems,
      finalPrice,
      discountAmount,
      promotionId,
    });

    // Update product stock
    for (const item of data.items) {
      await productRepo.updateStock(item.productId, item.quantity);
    }

    return order;
  }

  async update(id: number, data: {
    customerId?: number;
    items?: Array<{ productId: number; quantity: number }>;
    promotionCode?: string;
  }, userRole: string, userId?: number) {
    const existingOrder = await orderRepo.findById(id);

    if (!existingOrder) {
      throw new Error(Messages.ORDER_NOT_FOUND);
    }

    // SALE role can only update their own orders
    if (userRole === 'SALE' && userId && existingOrder.createdById !== userId) {
      throw new Error(Messages.FORBIDDEN);
    }

    if (existingOrder.status === OrderStatus.PAID) {
      throw new Error(Messages.ORDER_ALREADY_PAID);
    }

    let finalPrice = existingOrder.finalPrice;
    let discountAmount = existingOrder.discountAmount || 0; // Assuming dynamic type check or updated interface in future
    let promotionId = existingOrder.promotionId;
    
    let newOrderItems: Array<{
      productId: number;
      quantity: number;
      unitSalePrice: number;
      totalPrice: number;
    }> | undefined;

    // Calculate base price from existing items or new items
    let basePrice = 0;

    if (data.items) {
      // Restore stock from old items
      for (const oldItem of existingOrder.orderItems) {
        await productRepo.updateStock(oldItem.productId, -oldItem.quantity);
      }

      // Create new items
      newOrderItems = [];
      basePrice = 0;

      for (const item of data.items) {
        const product = await productRepo.findById(item.productId);

        if (!product) {
          throw new Error(Messages.PRODUCT_NOT_FOUND);
        }

        if (product.stock < item.quantity) {
          throw new Error(`${Messages.PRODUCT_OUT_OF_STOCK}: ${product.name}`);
        }

        const unitSalePrice = product.salePrice;
        const totalPrice = item.quantity * unitSalePrice;

        newOrderItems.push({
          productId: product.id,
          quantity: item.quantity,
          unitSalePrice,
          totalPrice,
        });

        basePrice += totalPrice;

          if (!product) {
            throw new Error(Messages.PRODUCT_NOT_FOUND);
          }

          const oldQty = oldItemsMap.get(item.productId) || 0;
          const diff = item.quantity - oldQty;

          if (diff > 0) {
              // Increasing quantity, check if we have enough stock
              if (product.stock < diff) {
                  throw new Error(`${Messages.PRODUCT_OUT_OF_STOCK}: ${product.name}`);
              }
              // Deduct stock
              await productRepo.updateStock(item.productId, diff, tx);
          } else if (diff < 0) {
              // Decreasing quantity, return stock
              // diff is negative, updateStock decrements => increments
              await productRepo.updateStock(item.productId, diff, tx);
          }
          
          // Mark as processed
          oldItemsMap.delete(item.productId);

          const unitSalePrice = product.salePrice;
          const totalPrice = item.quantity * unitSalePrice;

          newOrderItems.push({
            productId: product.id,
            quantity: item.quantity,
            unitSalePrice,
            totalPrice,
          });

          finalPrice += totalPrice;
        }
        
        // Restore stock for removed items
        for (const [productId, oldQty] of oldItemsMap) {
            await productRepo.updateStock(productId, -oldQty, tx);
        }
      }
    } else {
      // Recalculate base price from existing items (finalPrice + discountAmount) to be safe, 
      // or just trust the previous calculation logic if we assume finalPrice is correct.
      // Better to iterate items if we want exact base price, but for now:
       basePrice = existingOrder.finalPrice + (existingOrder.discountAmount || 0);
    }

    // Handle Promotion
    if (data.promotionCode) {
      // Applying new promotion
      const importPromotionService = (await import('./promotion.service')).default;
      const promoResult = await importPromotionService.validatePromotion(data.promotionCode, basePrice);
      
      if (promoResult.valid && promoResult.discountAmount !== undefined) {
        discountAmount = promoResult.discountAmount;
        promotionId = promoResult.promotion.id;
        
        // Increment usage count for new promotion
        await importPromotionService.applyPromotion(promoResult.promotion.id);
      } else {
         throw new Error(promoResult.message || 'Invalid promotion code');
      }
    } 
    // Optimization: If items changed but no new code, we should technically re-validate the OLD code against new value?
    // For MVP, we keep existing discount OR remove it? 
    // Let's simplified assumption: If items update, we keep the SAME promotion but re-calculate discount IF possible.
    // However, simplicity: If items change, we might need to re-validate.
    // Let's implement simpler logic: update finalPrice based on basePrice - discountAmount
    
    finalPrice = basePrice - discountAmount;
    if (finalPrice < 0) finalPrice = 0;

    return orderRepo.update(id, {
      customerId: data.customerId,
      items: newOrderItems,
      finalPrice,
      discountAmount,
      promotionId,
    });
  }

  async updateStatus(id: number, status: OrderStatus, userRole: string, userId?: number) {
    const order = await orderRepo.findById(id);

    if (!order) {
      throw new Error(Messages.ORDER_NOT_FOUND);
    }

    // SALE role can only update their own orders
    if (userRole === 'SALE' && userId && order.createdById !== userId) {
      throw new Error(Messages.FORBIDDEN);
    }

    if (!Object.values(OrderStatus).includes(status)) {
      throw new Error(Messages.ORDER_INVALID_STATUS);
    }

    return orderRepo.updateStatus(id, status);
  }

  async delete(id: number, userRole: string, userId?: number) {
    const order = await orderRepo.findById(id);

    if (!order) {
      throw new Error(Messages.ORDER_NOT_FOUND);
    }

    // SALE role can only delete their own orders
    if (userRole === 'SALE' && userId && order.createdById !== userId) {
      throw new Error(Messages.FORBIDDEN);
    }

    // Restore stock
    for (const item of order.orderItems) {
      await productRepo.updateStock(item.productId, -item.quantity);
    }

    return orderRepo.delete(id);
  }


}

export default new OrderService();

