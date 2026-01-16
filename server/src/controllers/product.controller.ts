import { Response } from "express";
import { AuthRequest } from "../middlewares/auth.middleware";
import productService from "../services/product.service";
import { sendSuccess, sendError } from "../utils/response";

export class ProductController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const filters = {
        page: req.query.page ? parseInt(req.query.page as string) : undefined,
        size: req.query.size ? parseInt(req.query.size as string) : undefined,
        sort: req.query.sort as string,
        minPrice: req.query.minPrice
          ? parseInt(req.query.minPrice as string)
          : undefined,
        maxPrice: req.query.maxPrice
          ? parseInt(req.query.maxPrice as string)
          : undefined,
        keyword: req.query.keyword as string,
        categoryId: req.query.categoryId
          ? parseInt(req.query.categoryId as string)
          : undefined,
        id: req.query.id ? parseInt(req.query.id as string) : undefined,
        inStock: req.query.inStock === 'true',
      };

      const result = await productService.getAll(filters);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, "INTERNAL_ERROR", error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const userRole = req.user?.role;
      const product = await productService.getById(id, userRole);
      sendSuccess(res, product);
    } catch (error: any) {
      sendError(res, "NOT_FOUND", error.message, 404);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const product = await productService.create(req.body);
      sendSuccess(res, product, "Product created successfully", 201);
    } catch (error: any) {
      sendError(res, "BAD_REQUEST", error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const product = await productService.update(id, req.body);
      sendSuccess(res, product, "Product updated successfully");
    } catch (error: any) {
      const statusCode = error.message.includes("not found") ? 404 : 400;
      sendError(res, "BAD_REQUEST", error.message, statusCode);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await productService.delete(id);
      sendSuccess(res, null, "Product deleted successfully");
    } catch (error: any) {
      sendError(res, "NOT_FOUND", error.message, 404);
    }
  }

  async import(_req: AuthRequest, res: Response) {
    sendError(
      res,
      "NOT_IMPLEMENTED",
      "Import functionality not yet implemented",
      501
    );
  }

  async uploadImages(req: AuthRequest, res: Response) {
    try {
      const productId = parseInt(req.params.id);
      const files = req.files as Express.Multer.File[];

      if (!files || files.length === 0) {
        return sendError(res, "BAD_REQUEST", "No images provided", 400);
      }

      const product = await productService.getById(productId);
      if (!product) {
        return sendError(res, "NOT_FOUND", "Product not found", 404);
      }

      const images = await productService.addImages(productId, files);
      sendSuccess(res, images, "Images uploaded successfully", 201);
    } catch (error: any) {
      sendError(res, "INTERNAL_ERROR", error.message, 500);
    }
  }

  async deleteImage(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await productService.deleteImage(id);
      sendSuccess(res, null, "Image deleted successfully");
    } catch (error: any) {
      sendError(res, "NOT_FOUND", error.message, 404);
    }
  }

  async getStats(_req: AuthRequest, res: Response) {
    try {
      const stats = await productService.getStats();
      sendSuccess(res, stats);
    } catch (error: any) {
      sendError(res, "INTERNAL_ERROR", error.message, 500);
    }
  }
}

export default new ProductController();
