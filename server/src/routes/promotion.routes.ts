import { Router } from 'express';
import promotionController from '../controllers/promotion.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { UserRole } from '../constants/roles';

const router = Router();

// Admin only - CRUD operations
router.get('/', authMiddleware, requireRole(UserRole.ADMIN), promotionController.getAll.bind(promotionController));
router.get('/:id', authMiddleware, requireRole(UserRole.ADMIN), promotionController.getById.bind(promotionController));
router.post('/', authMiddleware, requireRole(UserRole.ADMIN), promotionController.create.bind(promotionController));
router.put('/:id', authMiddleware, requireRole(UserRole.ADMIN), promotionController.update.bind(promotionController));
router.delete('/:id', authMiddleware, requireRole(UserRole.ADMIN), promotionController.delete.bind(promotionController));

// All authenticated users - validate promotion code
router.post('/validate', authMiddleware, promotionController.validate.bind(promotionController));

export default router;
