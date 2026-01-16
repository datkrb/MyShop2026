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
  }, userId: number) {
    const orderItems = [];
    let finalPrice = 0;

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

    const order = await orderRepo.create({
      customerId: data.customerId,
      createdById: userId,
      status: OrderStatus.PENDING,
      items: orderItems,
      finalPrice,
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

    return prisma.$transaction(async (tx) => {
      let finalPrice = existingOrder.finalPrice;
      let newOrderItems: Array<{
        productId: number;
        quantity: number;
        unitSalePrice: number;
        totalPrice: number;
      }> | undefined;

      if (data.items) {
        newOrderItems = [];
        finalPrice = 0;

        // Map old items for diff calculation
        const oldItemsMap = new Map<number, number>();
        existingOrder.orderItems.forEach((item: any) => {
          oldItemsMap.set(item.productId, item.quantity);
        });

        for (const item of data.items) {
          // Use findById with transaction
          const product = await productRepo.findById(item.productId, tx);

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

      return orderRepo.update(id, {
        customerId: data.customerId,
        items: newOrderItems,
        finalPrice: data.items ? finalPrice : undefined,
      }, tx);
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

