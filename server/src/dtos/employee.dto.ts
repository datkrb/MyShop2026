import { body } from 'express-validator';

export const createEmployeeDto = [
  body('username')
    .notEmpty().withMessage('Username is required')
    .isLength({ min: 3, max: 50 }).withMessage('Username must be between 3 and 50 characters'),
  body('password')
    .notEmpty().withMessage('Password is required')
    .isLength({ min: 6, max: 100 }).withMessage('Password must be between 6 and 100 characters'),
  body('role')
    .notEmpty().withMessage('Role is required')
    .isIn(['ADMIN', 'SALE']).withMessage('Role must be either ADMIN or SALE'),
];

export const updateEmployeeDto = [
  body('username')
    .optional()
    .isLength({ min: 3, max: 50 }).withMessage('Username must be between 3 and 50 characters'),
  body('password')
    .optional()
    .custom((value) => {
      if (value && value.length > 0 && value.length < 6) {
        throw new Error('Password must be at least 6 characters');
      }
      return true;
    }),
  body('role')
    .optional()
    .isIn(['ADMIN', 'SALE']).withMessage('Role must be either ADMIN or SALE'),
];
