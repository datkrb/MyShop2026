import { Router } from 'express';
import dashboardController from '../controllers/dashboard.controller';
import { authMiddleware } from '../middlewares/auth.middleware';

const router = Router();

router.get('/summary', authMiddleware, dashboardController.getSummary.bind(dashboardController));
router.get('/low-stock', authMiddleware, dashboardController.getLowStock.bind(dashboardController));
router.get('/top-selling', authMiddleware, dashboardController.getTopSelling.bind(dashboardController));
router.get('/revenue-chart', authMiddleware, dashboardController.getRevenueChart.bind(dashboardController));
router.get('/recent-orders', authMiddleware, dashboardController.getRecentOrders.bind(dashboardController));

export default router;

