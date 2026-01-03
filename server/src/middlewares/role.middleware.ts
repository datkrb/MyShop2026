import { Response, NextFunction } from 'express';
import { AuthRequest } from './auth.middleware';
import { UserRole } from '../constants/roles';
import { sendError } from '../utils/response';
import { Messages } from '../constants/messages';

export const requireRole = (...roles: UserRole[]) => {
  return (req: AuthRequest, res: Response, next: NextFunction) => {
    if (!req.user) {
      return sendError(res, 'UNAUTHORIZED', Messages.UNAUTHORIZED, 401);
    }

    if (!roles.includes(req.user.role as UserRole)) {
      return sendError(res, 'FORBIDDEN', Messages.FORBIDDEN, 403);
    }

    next();
  };
};

