import prisma from '../config/prisma';

export class LicenseRepository {
  /**
   * Lấy license record hiện tại, tạo mới nếu chưa có
   */
  async getOrCreate() {
    let license = await prisma.shopLicense.findFirst();
    
    if (!license) {
      license = await prisma.shopLicense.create({
        data: {
          trialDays: 15,
          isActivated: false,
        },
      });
    }
    
    return license;
  }

  /**
   * Cập nhật license khi kích hoạt
   */
  async activate(id: number, licenseKey: string, adminId: number) {
    return prisma.shopLicense.update({
      where: { id },
      data: {
        licenseKey,
        isActivated: true,
        activatedAt: new Date(),
        activatedBy: adminId,
      },
    });
  }
}

export default new LicenseRepository();
