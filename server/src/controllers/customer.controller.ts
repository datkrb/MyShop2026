import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import customerService from '../services/customer.service';
import { sendSuccess, sendError } from '../utils/response';

export class CustomerController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const filters = {
        page: req.query.page ? parseInt(req.query.page as string) : undefined,
        size: req.query.size ? parseInt(req.query.size as string) : undefined,
        keyword: req.query.keyword as string,
        // Advanced search filters
        hasOrders: req.query.hasOrders !== undefined ? req.query.hasOrders === 'true' : undefined,
        createdFrom: req.query.createdFrom as string,
        createdTo: req.query.createdTo as string,
        sort: req.query.sort as string,
      };

      const result = await customerService.getAll(filters);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const customer = await customerService.getById(id);
      sendSuccess(res, customer);
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const customer = await customerService.create(req.body);
      sendSuccess(res, customer, 'Customer created successfully', 201);
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const customer = await customerService.update(id, req.body);
      sendSuccess(res, customer, 'Customer updated successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : 400;
      sendError(res, 'BAD_REQUEST', error.message, statusCode);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await customerService.delete(id);
      sendSuccess(res, null, 'Customer deleted successfully');
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async getStats(_req: AuthRequest, res: Response) {
    try {
      const stats = await customerService.getStats();
      sendSuccess(res, stats);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new CustomerController();

