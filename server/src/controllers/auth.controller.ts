import { Response } from 'express';
import { AuthRequest } from '../middlewares/auth.middleware';
import authService from '../services/auth.service';
import { sendSuccess, sendError } from '../utils/response';
import { Messages } from '../constants/messages';

export class AuthController {
  async login(req: AuthRequest, res: Response) {
    try {
      const { username, password } = req.body;
      const result = await authService.login(username, password);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'INVALID_CREDENTIALS', error.message, 401);
    }
  }

  async getCurrentUser(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', Messages.UNAUTHORIZED, 401);
      }

      const user = await authService.getCurrentUser(req.user.userId);
      sendSuccess(res, user);
    } catch (error: any) {
      sendError(res, 'NOT_FOUND', error.message, 404);
    }
  }

  async logout(_req: AuthRequest, res: Response) {
    // Client-side logout (token removal)
    // Server-side can implement token blacklist if needed
    sendSuccess(res, { message: 'Logged out successfully' });
  }
  async refreshToken(req: AuthRequest, res: Response) {
    try {
      const { refreshToken } = req.body;
      if (!refreshToken) {
        return sendError(res, 'BAD_REQUEST', 'Refresh token is required', 400);
      }

      const result = await authService.refreshToken(refreshToken);
      sendSuccess(res, result);
    } catch (error: any) {
      sendError(res, 'UNAUTHORIZED', error.message, 401);
    }
  }

  async changePassword(req: AuthRequest, res: Response) {
    try {
      if (!req.user) {
        return sendError(res, 'UNAUTHORIZED', Messages.UNAUTHORIZED, 401);
      }

      const { currentPassword, newPassword } = req.body;
      
      if (!currentPassword || !newPassword) {
        return sendError(res, 'BAD_REQUEST', 'Current password and new password are required', 400);
      }

      if (newPassword.length < 6) {
        return sendError(res, 'BAD_REQUEST', 'New password must be at least 6 characters', 400);
      }

      const result = await authService.changePassword(req.user.userId, currentPassword, newPassword);
      sendSuccess(res, result);
    } catch (error: any) {
      if (error.message === 'Current password is incorrect') {
        return sendError(res, 'INVALID_CREDENTIALS', error.message, 400);
      }
      sendError(res, 'INTERNAL_ERROR', error.message, 500);
    }
  }
}

export default new AuthController();

