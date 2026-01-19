import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import employeeService from '../services/employee.service';
import { sendSuccess, sendError } from '../utils/response';
import { UserRole } from '../constants/roles';

export class EmployeeController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const filters = {
        page: req.query.page ? parseInt(req.query.page as string) : undefined,
        size: req.query.size ? parseInt(req.query.size as string) : undefined,
        search: req.query.search as string | undefined,
        role: req.query.role as string | undefined,
      };

      // If pagination params provided, use paginated response
      if (filters.page || filters.size || filters.search || filters.role) {
        const result = await employeeService.getAllPaginated(filters);
        sendSuccess(res, result);
      } else {
        // Return all for backward compatibility
        const employees = await employeeService.getAll();
        sendSuccess(res, employees);
      }
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      if (isNaN(id)) {
        return sendError(res, 'INVALID_ID', 'Invalid employee ID', 400);
      }

      const employee = await employeeService.getById(id);
      sendSuccess(res, employee);
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const { username, password, role } = req.body;
      const employee = await employeeService.create(username, password, role as UserRole);
      sendSuccess(res, employee, 'Employee created successfully', 201);
    } catch (error: any) {
      if (error.message === 'Username already exists') {
        return sendError(res, 'DUPLICATE', error.message, 409);
      }
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      if (isNaN(id)) {
        return sendError(res, 'INVALID_ID', 'Invalid employee ID', 400);
      }

      const { username, password, role } = req.body;
      const employee = await employeeService.update(id, { 
        username, 
        password, 
        role: role as UserRole 
      });
      sendSuccess(res, employee);
    } catch (error: any) {
      if (error.message === 'Username already exists') {
        return sendError(res, 'DUPLICATE', error.message, 409);
      }
      if (error.message.includes('not found')) {
        return sendError(res, 'NOT_FOUND', error.message, 404);
      }
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      if (isNaN(id)) {
        return sendError(res, 'INVALID_ID', 'Invalid employee ID', 400);
      }

      const currentUserId = req.user?.userId;
      if (!currentUserId) {
        return sendError(res, 'UNAUTHORIZED', 'User not authenticated', 401);
      }

      await employeeService.delete(id, currentUserId);
      sendSuccess(res, { message: 'Employee deleted successfully' });
    } catch (error: any) {
      if (error.message === 'Cannot delete your own account') {
        return sendError(res, 'FORBIDDEN', error.message, 403);
      }
      if (error.message.includes('not found')) {
        return sendError(res, 'NOT_FOUND', error.message, 404);
      }
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new EmployeeController();
