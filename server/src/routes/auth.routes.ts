import { Router } from 'express';
import authController from '../controllers/auth.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { validate } from '../middlewares/validate.middleware';
import { loginDto } from '../dtos/auth.dto';

const router = Router();

router.post('/login', validate(loginDto), authController.login.bind(authController));
router.get('/me', authMiddleware, authController.getCurrentUser.bind(authController));
router.post('/logout', authMiddleware, authController.logout.bind(authController));

export default router;

