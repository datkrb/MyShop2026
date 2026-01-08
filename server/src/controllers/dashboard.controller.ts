import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import dashboardService from '../services/dashboard.service';
import { sendSuccess, sendError } from '../utils/response';

export class DashboardController {
  async getSummary(_req: AuthRequest, res: Response) {
    try {
      const summary = await dashboardService.getSummary();
      sendSuccess(res, summary);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getLowStock(_req: AuthRequest, res: Response) {
    try {
      const products = await dashboardService.getLowStock(5);
      sendSuccess(res, products);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getTopSelling(_req: AuthRequest, res: Response) {
    try {
      const products = await dashboardService.getTopSelling(5);
      sendSuccess(res, products);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getRevenueChart(_req: AuthRequest, res: Response) {
    try {
      const chart = await dashboardService.getRevenueChart();
      sendSuccess(res, chart);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getRecentOrders(_req: AuthRequest, res: Response) {
    try {
      const orders = await dashboardService.getRecentOrders(3);
      sendSuccess(res, orders);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getCategoryStats(_req: AuthRequest, res: Response) {
    try {
      const stats = await dashboardService.getCategoryStats();
      sendSuccess(res, stats);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new DashboardController();

