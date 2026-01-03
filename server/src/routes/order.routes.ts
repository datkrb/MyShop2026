import { Router } from 'express';
import orderController from '../controllers/order.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import { createOrderDto, updateOrderStatusDto } from '../dtos/order.dto';

const router = Router();

router.get('/', authMiddleware, orderController.getAll.bind(orderController));
router.get('/:id', authMiddleware, orderController.getById.bind(orderController));
router.post('/', authMiddleware, validate(createOrderDto), orderController.create.bind(orderController));
router.put('/:id', authMiddleware, orderController.update.bind(orderController));
router.delete('/:id', authMiddleware, orderController.delete.bind(orderController));
router.put('/:id/status', authMiddleware, validate(updateOrderStatusDto), orderController.updateStatus.bind(orderController));
router.post('/:id/autosave', authMiddleware, orderController.autosave.bind(orderController));
router.get('/:id/export', authMiddleware, orderController.export.bind(orderController));

export default router;

