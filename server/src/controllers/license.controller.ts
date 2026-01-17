import { Request, Response } from 'express';
import licenseService from '../services/license.service';

export class LicenseController {
  /**
   * GET /api/license/status
   * Lấy trạng thái license hiện tại
   */
  async getStatus(_req: Request, res: Response) {
    try {
      const status = await licenseService.getStatus();
      res.json({ success: true, data: status });
    } catch (error: any) {
      res.status(500).json({ success: false, message: error.message });
    }
  }

  /**
   * POST /api/license/activate
   * Kích hoạt license (ADMIN only)
   */
  async activate(req: Request, res: Response) {
    try {
      const { licenseKey } = req.body;
      const userId = (req as any).user?.userId;

      if (!licenseKey) {
        res.status(400).json({ success: false, message: 'License key là bắt buộc' });
        return;
      }

      const result = await licenseService.activate(licenseKey, userId);
      res.json({ success: true, data: result });
    } catch (error: any) {
      res.status(400).json({ success: false, message: error.message });
    }
  }
}

export default new LicenseController();
