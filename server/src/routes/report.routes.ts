import { Router } from 'express';
import reportController from '../controllers/report.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { UserRole } from '../constants/roles';

const router = Router();

router.get('/revenue', authMiddleware, reportController.getRevenue.bind(reportController));
router.get('/profit', authMiddleware, reportController.getProfit.bind(reportController));
router.get('/profit-series', authMiddleware, reportController.getProfitTimeSeries.bind(reportController));
router.get('/products', authMiddleware, reportController.getProductSales.bind(reportController));
router.get('/products/timeseries', authMiddleware, reportController.getTopProductsTimeSeries.bind(reportController));
router.get('/products/:productId/sales', authMiddleware, reportController.getProductSalesById.bind(reportController));
router.get('/category-sales', authMiddleware, reportController.getCategorySalesTimeSeries.bind(reportController));
router.get('/my-kpi', authMiddleware, reportController.getMyKPI.bind(reportController));
router.get('/kpi-sales', authMiddleware, requireRole(UserRole.ADMIN), reportController.getKPISales.bind(reportController));

export default router;


