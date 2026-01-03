import { body } from 'express-validator';
import { OrderStatus } from '../constants/order-status';

export const createOrderDto = [
  body('customerId').optional().isInt({ min: 1 }).withMessage('Customer ID must be a positive integer'),
  body('items').isArray({ min: 1 }).withMessage('Order items are required'),
  body('items.*.productId').isInt({ min: 1 }).withMessage('Product ID is required'),
  body('items.*.quantity').isInt({ min: 1 }).withMessage('Quantity must be at least 1'),
];

export const updateOrderStatusDto = [
  body('status')
    .isIn(Object.values(OrderStatus))
    .withMessage(`Status must be one of: ${Object.values(OrderStatus).join(', ')}`),
];

