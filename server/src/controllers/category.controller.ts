import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import categoryService from '../services/category.service';
import { sendSuccess, sendError } from '../utils/response';

export class CategoryController {
  async getAll(_req: AuthRequest, res: Response) {
    try {
      const categories = await categoryService.getAll();
      sendSuccess(res, categories);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const category = await categoryService.getById(id);
      sendSuccess(res, category);
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const { name, description } = req.body;
      const category = await categoryService.create(name, description);
      sendSuccess(res, category, 'Category created successfully', 201);
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const { name, description } = req.body;
      const category = await categoryService.update(id, { name, description });
      sendSuccess(res, category, 'Category updated successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : 400;
      sendError(res, 'BAD_REQUEST', error.message, statusCode);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await categoryService.delete(id);
      sendSuccess(res, null, 'Category deleted successfully');
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }
}

export default new CategoryController();

