import { Router } from 'express';
import reportController from '../controllers/report.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { UserRole } from '../constants/roles';

const router = Router();

router.get('/revenue', authMiddleware, requireRole(UserRole.ADMIN), reportController.getRevenue.bind(reportController));
router.get('/profit', authMiddleware, requireRole(UserRole.ADMIN), reportController.getProfit.bind(reportController));
router.get('/products', authMiddleware, requireRole(UserRole.ADMIN), reportController.getProductSales.bind(reportController));
router.get('/products/timeseries', authMiddleware, requireRole(UserRole.ADMIN), reportController.getTopProductsTimeSeries.bind(reportController));
router.get('/kpi-sales', authMiddleware, requireRole(UserRole.ADMIN), reportController.getKPISales.bind(reportController));

export default router;

