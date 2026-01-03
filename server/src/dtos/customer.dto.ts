import { body } from 'express-validator';

export const createCustomerDto = [
  body('name').notEmpty().withMessage('Customer name is required'),
  body('phone').optional().isString(),
  body('email').optional().isEmail().withMessage('Invalid email format'),
  body('address').optional().isString(),
];

export const updateCustomerDto = [
  body('name').optional().notEmpty().withMessage('Customer name cannot be empty'),
  body('phone').optional().isString(),
  body('email').optional().isEmail().withMessage('Invalid email format'),
  body('address').optional().isString(),
];

