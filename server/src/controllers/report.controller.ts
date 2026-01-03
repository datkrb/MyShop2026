import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import reportService from '../services/report.service';
import { sendSuccess, sendError } from '../utils/response';

export class ReportController {
  async getRevenue(req: AuthRequest, res: Response) {
    try {
      const type = req.query.type as 'day' | 'month' | 'year';
      const year = parseInt(req.query.year as string) || new Date().getFullYear();
      const month = req.query.month ? parseInt(req.query.month as string) : undefined;

      if (!type || !['day', 'month', 'year'].includes(type)) {
        return sendError(res, 'BAD_REQUEST', 'Invalid type. Must be day, month, or year', 400);
      }

      const result = await reportService.getRevenueReport(type, year, month);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProfit(req: AuthRequest, res: Response) {
    try {
      const year = parseInt(req.query.year as string) || new Date().getFullYear();
      const month = req.query.month ? parseInt(req.query.month as string) : undefined;

      const result = await reportService.getProfitReport(year, month);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProductSales(req: AuthRequest, res: Response) {
    try {
      const year = parseInt(req.query.year as string) || new Date().getFullYear();
      const month = req.query.month ? parseInt(req.query.month as string) : undefined;

      const result = await reportService.getProductSalesReport(year, month);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getKPISales(req: AuthRequest, res: Response) {
    try {
      const year = parseInt(req.query.year as string) || new Date().getFullYear();
      const month = req.query.month ? parseInt(req.query.month as string) : undefined;

      const result = await reportService.getKPISalesReport(year, month);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new ReportController();

