import { body } from 'express-validator';

export const createCategoryDto = [
  body('name').notEmpty().withMessage('Category name is required'),
  body('description').optional().isString(),
];

export const updateCategoryDto = [
  body('name').optional().notEmpty().withMessage('Category name cannot be empty'),
  body('description').optional().isString(),
];

