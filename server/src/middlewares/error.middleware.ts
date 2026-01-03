import { Request, Response, NextFunction } from 'express';
import logger from '../utils/logger';
import { sendError } from '../utils/response';
import { Messages } from '../constants/messages';

export const errorMiddleware = (
  err: Error,
  _req: Request,
  res: Response,
  _next: NextFunction
) => {
  logger.error('Error:', err);

  if (err.name === 'ValidationError') {
    return sendError(res, 'VALIDATION_ERROR', err.message, 400);
  }

  if (err.name === 'UnauthorizedError' || err.message.includes('token')) {
    return sendError(res, 'UNAUTHORIZED', Messages.UNAUTHORIZED, 401);
  }

  if (err.name === 'PrismaClientKnownRequestError') {
    return sendError(res, 'DATABASE_ERROR', 'Database operation failed', 500);
  }

  sendError(res, 'INTERNAL_ERROR', Messages.INTERNAL_ERROR, 500);
};

