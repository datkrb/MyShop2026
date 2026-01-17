import { Router } from 'express';
import licenseController from '../controllers/license.controller';
import { authMiddleware } from '../middlewares/auth.middleware';
import { requireRole } from '../middlewares/role.middleware';
import { UserRole } from '../constants/roles';

const router = Router();

// All authenticated users - get license status
router.get('/status', authMiddleware, licenseController.getStatus.bind(licenseController));

// Admin only - activate license
router.post('/activate', authMiddleware, requireRole(UserRole.ADMIN), licenseController.activate.bind(licenseController));

export default router;
