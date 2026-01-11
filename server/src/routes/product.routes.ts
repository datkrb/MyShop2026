import { Router } from 'express';
import productController from '../controllers/product.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { validate } from '../middlewares/validate.middleware';
import { createProductDto, updateProductDto } from '../dtos/product.dto';
import { UserRole } from '../constants/roles';

import { upload } from '../middlewares/upload.middleware';

const router = Router();

router.get('/', authMiddleware, productController.getAll.bind(productController));
router.get('/stats', authMiddleware, productController.getStats.bind(productController));
router.get('/:id', authMiddleware, productController.getById.bind(productController));
router.post('/', authMiddleware, requireRole(UserRole.ADMIN), validate(createProductDto), productController.create.bind(productController));
router.put('/:id', authMiddleware, requireRole(UserRole.ADMIN), validate(updateProductDto), productController.update.bind(productController));
router.delete('/:id', authMiddleware, requireRole(UserRole.ADMIN), productController.delete.bind(productController));
router.post('/import', authMiddleware, requireRole(UserRole.ADMIN), productController.import.bind(productController));
router.post('/:id/images', authMiddleware, requireRole(UserRole.ADMIN), upload.array('images', 10), productController.uploadImages.bind(productController));
router.delete('/images/:id', authMiddleware, requireRole(UserRole.ADMIN), productController.deleteImage.bind(productController));

export default router;

