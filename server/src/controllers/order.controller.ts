import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import orderService from '../services/order.service';
import { sendSuccess, sendError } from '../utils/response';

export class OrderController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const filters = {
        page: req.query.page ? parseInt(req.query.page as string) : undefined,
        size: req.query.size ? parseInt(req.query.size as string) : undefined,
        fromDate: req.query.fromDate as string,
        toDate: req.query.toDate as string,
        status: req.query.status as string,
        // Advanced search filters
        customerId: req.query.customerId ? parseInt(req.query.customerId as string) : undefined,
        minAmount: req.query.minAmount ? parseFloat(req.query.minAmount as string) : undefined,
        maxAmount: req.query.maxAmount ? parseFloat(req.query.maxAmount as string) : undefined,
        keyword: req.query.keyword as string,
      };

      const userRole = req.user?.role || '';
      const userId = req.user?.userId;
      const result = await orderService.getAll(filters, userRole, userId);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }



  async getById(req: AuthRequest, res: Response) {
    console.log('OrderController.getById hit', req.params.id); // DEBUG
    try {
      const id = parseInt(req.params.id);
      const userRole = req.user?.role || '';
      const userId = req.user?.userId;
      const order = await orderService.getById(id, userRole, userId);
      sendSuccess(res, order);
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : error.message.includes('Forbidden') ? 403 : 500;
      sendError(res, 'ERROR', error.message, statusCode);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', 'Unauthorized', 401);
      }

      const order = await orderService.create(req.body, req.user.userId);
      sendSuccess(res, order, 'Order created successfully', 201);
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', 'Unauthorized', 401);
      }

      const id = parseInt(req.params.id);
      const userRole = req.user.role;
      const userId = req.user.userId;
      const order = await orderService.update(id, req.body, userRole, userId);
      sendSuccess(res, order, 'Order updated successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : error.message.includes('Forbidden') ? 403 : 400;
      sendError(res, 'ERROR', error.message, statusCode);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', 'Unauthorized', 401);
      }

      const id = parseInt(req.params.id);
      const userRole = req.user.role;
      const userId = req.user.userId;
      await orderService.delete(id, userRole, userId);
      sendSuccess(res, null, 'Order deleted successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : error.message.includes('Forbidden') ? 403 : 500;
      sendError(res, 'ERROR', error.message, statusCode);
    }
  }

  async updateStatus(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', 'Unauthorized', 401);
      }

      const id = parseInt(req.params.id);
      const { status } = req.body;
      const userRole = req.user.role;
      const userId = req.user.userId;
      const order = await orderService.updateStatus(id, status, userRole, userId);
      sendSuccess(res, order, 'Order status updated successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : error.message.includes('Forbidden') ? 403 : 400;
      sendError(res, 'ERROR', error.message, statusCode);
    }
  }



  async export(_req: AuthRequest, res: Response) {
    // TODO: Implement PDF/XPS export
    sendError(res, 'NOT_IMPLEMENTED', 'Export functionality not yet implemented', 501);
  }
}

export default new OrderController();

