import { Request, Response } from 'express';
import promotionService from '../services/promotion.service';
import { sendSuccess, sendError } from '../utils/response';

interface AuthRequest extends Request {
  user?: { id: number; role: string };
}

export class PromotionController {
  async getAll(req: AuthRequest, res: Response) {
    try {
      const { page, size, isActive, search, discountType, validFrom, validTo, minDiscount, maxDiscount } = req.query;
      const result = await promotionService.getAll({
        page: page ? parseInt(page as string) : undefined,
        size: size ? parseInt(size as string) : undefined,
        isActive: isActive !== undefined ? isActive === 'true' : undefined,
        search: search as string,
        // Advanced search filters
        discountType: discountType as 'PERCENTAGE' | 'FIXED' | undefined,
        validFrom: validFrom as string,
        validTo: validTo as string,
        minDiscount: minDiscount ? parseFloat(minDiscount as string) : undefined,
        maxDiscount: maxDiscount ? parseFloat(maxDiscount as string) : undefined,
      });
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }

  async getById(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const promotion = await promotionService.getById(id);
      sendSuccess(res, promotion);
    } catch (error: any) {
        const statusCode = error.message.includes('not found') ? 404 : 500;
        sendError(res, 'ERROR', error.message, statusCode);
    }
  }

  async create(req: AuthRequest, res: Response) {
    try {
      const data = {
        ...req.body,
        startDate: new Date(req.body.startDate),
        endDate: new Date(req.body.endDate),
      };
      const promotion = await promotionService.create(data);
      sendSuccess(res, promotion, 'Promotion created successfully', 201);
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async update(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      const data = {
        ...req.body,
        ...(req.body.startDate && { startDate: new Date(req.body.startDate) }),
        ...(req.body.endDate && { endDate: new Date(req.body.endDate) }),
      };
      const promotion = await promotionService.update(id, data);
      sendSuccess(res, promotion, 'Promotion updated successfully');
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async delete(req: AuthRequest, res: Response) {
    try {
      const id = parseInt(req.params.id);
      await promotionService.delete(id);
      sendSuccess(res, null, 'Promotion deleted successfully');
    } catch (error: any) {
      sendError(res, 'BAD_REQUEST', error.message, 400);
    }
  }

  async validate(req: AuthRequest, res: Response) {
    try {
      const { code, orderValue } = req.body;

      if (!code || orderValue === undefined) {
        return sendError(res, 'BAD_REQUEST', 'Code and orderValue are required', 400);
      }

      const result = await promotionService.validatePromotion(code, orderValue);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new PromotionController();
