import { Router, Request, Response } from 'express';
import authRoutes from './auth.routes';
import categoryRoutes from './category.routes';
import productRoutes from './product.routes';
import customerRoutes from './customer.routes';
import orderRoutes from './order.routes';
import reportRoutes from './report.routes';
import dashboardRoutes from './dashboard.routes';
import adminRoutes from './admin.routes';
import promotionRoutes from './promotion.routes';
import licenseRoutes from './license.routes';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { UserRole } from '../constants/roles';

const router = Router();

// Health check
router.get('/health', (_req: Request, res: Response) => {
  res.json({
    status: 'OK',
    version: '1.0.0',
  });
});

router.use('/auth', authRoutes);
router.use('/categories', categoryRoutes);
router.use('/products', productRoutes);
router.use('/customers', customerRoutes);
router.use('/orders', orderRoutes);
router.use('/reports', reportRoutes);
router.use('/dashboard', dashboardRoutes);
router.use('/promotions', promotionRoutes);
router.use('/license', licenseRoutes);
router.use('/admin', authMiddleware, requireRole(UserRole.ADMIN), adminRoutes);

export default router;

