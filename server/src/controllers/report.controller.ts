import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import reportService from '../services/report.service';
import { sendSuccess, sendError } from '../utils/response';
import { UserRole } from '../constants/roles';

export class ReportController {
  // Helper to get createdById filter for SALE users
  private getCreatedByIdFilter(req: AuthRequest): number | undefined {
    return req.user?.role === UserRole.SALE ? req.user?.userId : undefined;
  }

  async getRevenue(req: AuthRequest, res: Response) {
    try {
      const type = (req.query.type as 'day' | 'month' | 'year') || 'day';
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;
      const categoryIdStr = req.query.categoryId as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        // Fallback to current year/month if not provided, or strict error
        // For now, let's default to current month
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const categoryId = categoryIdStr ? parseInt(categoryIdStr) : undefined;
      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getRevenueReport(startDate, endDate, type, categoryId, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProfit(req: AuthRequest, res: Response) {
    try {
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), 0, 1);
        endDate = new Date(now.getFullYear() + 1, 0, 1);
      }
      
      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const categoryIdStr = req.query.categoryId as string;
      const categoryId = categoryIdStr ? parseInt(categoryIdStr) : undefined;
      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getProfitReport(startDate, endDate, categoryId, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProductSales(req: AuthRequest, res: Response) {
    try {
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), 0, 1);
        endDate = new Date(now.getFullYear() + 1, 0, 1);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getProductSalesReport(startDate, endDate, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getTopProductsTimeSeries(req: AuthRequest, res: Response) {
    try {
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;
      const categoryIdStr = req.query.categoryId as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const categoryId = categoryIdStr ? parseInt(categoryIdStr) : undefined;
      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getTopProductsSalesTimeSeries(startDate, endDate, categoryId, createdById);
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

  async getMyKPI(req: AuthRequest, res: Response) {
    try {
      const year = parseInt(req.query.year as string) || new Date().getFullYear();
      const month = req.query.month ? parseInt(req.query.month as string) : undefined;
      const userId = req.user?.userId;

      if (!userId) {
        return sendError(res, 'UNAUTHORIZED', 'Unauthorized', 401);
      }

      const result = await reportService.getKPISalesReport(year, month, userId);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProfitTimeSeries(req: AuthRequest, res: Response) {
    try {
      const type = (req.query.type as 'day' | 'month' | 'year') || 'day';
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;
      const categoryIdStr = req.query.categoryId as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const categoryId = categoryIdStr ? parseInt(categoryIdStr) : undefined;
      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getProfitTimeSeries(startDate, endDate, type, categoryId, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getProductSalesById(req: AuthRequest, res: Response) {
    try {
      const productId = parseInt(req.params.productId);
      const type = (req.query.type as 'day' | 'month') || 'day';
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;

      if (isNaN(productId)) {
        return sendError(res, 'BAD_REQUEST', 'Invalid product ID', 400);
      }

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getProductSalesById(startDate, endDate, productId, type, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getCategorySalesTimeSeries(req: AuthRequest, res: Response) {
    try {
      const type = (req.query.type as 'day' | 'month') || 'day';
      const startDateStr = req.query.startDate as string;
      const endDateStr = req.query.endDate as string;

      let startDate: Date;
      let endDate: Date;

      if (startDateStr && endDateStr) {
        startDate = new Date(startDateStr);
        endDate = new Date(endDateStr);
      } else {
        const now = new Date();
        startDate = new Date(now.getFullYear(), now.getMonth(), 1);
        endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);
      }

      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        return sendError(res, 'BAD_REQUEST', 'Invalid date format', 400);
      }

      const createdById = this.getCreatedByIdFilter(req);

      const result = await reportService.getCategorySalesTimeSeries(startDate, endDate, type, createdById);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new ReportController();

