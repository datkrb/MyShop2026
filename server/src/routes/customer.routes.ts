import { Router } from 'express';
import customerController from '../controllers/customer.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { validate } from '../middlewares/validate.middleware';
import { createCustomerDto, updateCustomerDto } from '../dtos/customer.dto';
import { UserRole } from '../constants/roles';

const router = Router();

router.get('/', authMiddleware, customerController.getAll.bind(customerController));
router.get('/stats', authMiddleware, customerController.getStats.bind(customerController));
router.get('/:id', authMiddleware, customerController.getById.bind(customerController));
router.post('/', authMiddleware, requireRole(UserRole.ADMIN, UserRole.SALE), validate(createCustomerDto), customerController.create.bind(customerController));
router.put('/:id', authMiddleware, requireRole(UserRole.ADMIN, UserRole.SALE), validate(updateCustomerDto), customerController.update.bind(customerController));
router.delete('/:id', authMiddleware, requireRole(UserRole.ADMIN), customerController.delete.bind(customerController));

export default router;

