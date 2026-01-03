import { body } from 'express-validator';

export const createProductDto = [
  body('name').notEmpty().withMessage('Product name is required'),
  body('sku').notEmpty().withMessage('SKU is required'),
  body('importPrice').isInt({ min: 0 }).withMessage('Import price must be a positive integer'),
  body('salePrice').isInt({ min: 0 }).withMessage('Sale price must be a positive integer'),
  body('stock').isInt({ min: 0 }).withMessage('Stock must be a non-negative integer'),
  body('categoryId').isInt({ min: 1 }).withMessage('Category ID is required'),
  body('description').optional().isString(),
];

export const updateProductDto = [
  body('name').optional().notEmpty().withMessage('Product name cannot be empty'),
  body('sku').optional().notEmpty().withMessage('SKU cannot be empty'),
  body('importPrice').optional().isInt({ min: 0 }).withMessage('Import price must be a positive integer'),
  body('salePrice').optional().isInt({ min: 0 }).withMessage('Sale price must be a positive integer'),
  body('stock').optional().isInt({ min: 0 }).withMessage('Stock must be a non-negative integer'),
  body('categoryId').optional().isInt({ min: 1 }).withMessage('Category ID must be a positive integer'),
  body('description').optional().isString(),
];

