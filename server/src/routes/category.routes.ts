import { Router } from 'express';
import categoryController from '../controllers/category.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { validate } from '../middlewares/validate.middleware';
import { createCategoryDto, updateCategoryDto } from '../dtos/category.dto';
import { UserRole } from '../constants/roles';

const router = Router();

router.get('/', authMiddleware, categoryController.getAll.bind(categoryController));
router.get('/:id', authMiddleware, categoryController.getById.bind(categoryController));
router.post('/', authMiddleware, requireRole(UserRole.ADMIN), validate(createCategoryDto), categoryController.create.bind(categoryController));
router.put('/:id', authMiddleware, requireRole(UserRole.ADMIN), validate(updateCategoryDto), categoryController.update.bind(categoryController));
router.delete('/:id', authMiddleware, requireRole(UserRole.ADMIN), categoryController.delete.bind(categoryController));

export default router;

