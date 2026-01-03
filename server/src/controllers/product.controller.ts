import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import productService from '../services/product.service';
import { sendSuccess, sendError } from '../utils/response';

export class ProductController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const filters = {
        page: req.query.page ? parseInt(req.query.page as string) : undefined,
        size: req.query.size ? parseInt(req.query.size as string) : undefined,
        sort: req.query.sort as string,
        minPrice: req.query.minPrice ? parseInt(req.query.minPrice as string) : undefined,
        maxPrice: req.query.maxPrice ? parseInt(req.query.maxPrice as string) : undefined,
        keyword: req.query.keyword as string,
        categoryId: req.query.categoryId ? parseInt(req.query.categoryId as string) : undefined,
      };

      const result = await productService.getAll(filters);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const userRole = req.user?.role;
      const product = await productService.getById(id, userRole);
      sendSuccess(res, product);
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const product = await productService.create(req.body);
      sendSuccess(res, product, 'Product created successfully', 201);
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const product = await productService.update(id, req.body);
      sendSuccess(res, product, 'Product updated successfully');
    } catch (error: any) {
      const statusCode = error.message.includes('not found') ? 404 : 400;
      sendError(res, 'BAD_REQUEST', error.message, statusCode);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await productService.delete(id);
      sendSuccess(res, null, 'Product deleted successfully');
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async import(_req: AuthRequest, res: Response) {
    // TODO: Implement Excel/Access file import
    sendError(res, 'NOT_IMPLEMENTED', 'Import functionality not yet implemented', 501);
  }

  async uploadImages(_req: AuthRequest, res: Response) {
    // TODO: Implement image upload
    sendError(res, 'NOT_IMPLEMENTED', 'Image upload functionality not yet implemented', 501);
  }
}

export default new ProductController();

