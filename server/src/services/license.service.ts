import licenseRepo from '../repositories/license.repo';

export class LicenseService {
  // Sample valid license keys (hardcoded for demo)
  private readonly VALID_KEYS = [
    'MYSH-2024-SHOP-FREX',
    'MYSH-DEMO-TEST-ABCW',
    'MYSH-PREM-FULL-2024',
  ];

  /**
   * Lấy trạng thái license hiện tại
   */
  async getStatus() {
    const license = await licenseRepo.getOrCreate();
    
    // Tính số ngày trial còn lại
    const now = new Date();
    const trialStart = new Date(license.trialStartDate);
    const daysPassed = Math.floor((now.getTime() - trialStart.getTime()) / (1000 * 60 * 60 * 24));
    const daysRemaining = Math.max(0, license.trialDays - daysPassed);
    
    return {
      isActivated: license.isActivated,
      isTrialValid: daysRemaining > 0,
      trialDaysRemaining: daysRemaining,
      activatedAt: license.activatedAt,
    };
  }

  /**
   * Kích hoạt license với key
   */
  async activate(key: string, adminId: number) {
    // Validate key format và checksum
    if (!this.validateKey(key)) {
      throw new Error('License key không hợp lệ');
    }

    const license = await licenseRepo.getOrCreate();
    
    if (license.isActivated) {
      throw new Error('License đã được kích hoạt trước đó');
    }

    await licenseRepo.activate(license.id, key, adminId);
    
    return { success: true, message: 'Kích hoạt thành công!' };
  }

  /**
   * Validate license key
   * Format: MYSH-XXXX-XXXX-XXXX
   * Checksum: Ký tự cuối = (tổng ASCII) mod 26 + 'A'
   */
  validateKey(key: string): boolean {
    // Check format
    const pattern = /^MYSH-[A-Z0-9]{4}-[A-Z0-9]{4}-[A-Z0-9]{4}$/;
    if (!pattern.test(key)) {
      return false;
    }

    // Check against valid keys list (simple approach for demo)
    if (this.VALID_KEYS.includes(key)) {
      return true;
    }

    // Checksum validation
    const parts = key.split('-');
    const checkPart = parts[1] + parts[2] + parts[3].slice(0, 3); // First 11 chars after MYSH-
    const lastChar = parts[3].slice(-1);
    
    let sum = 0;
    for (const char of checkPart) {
      sum += char.charCodeAt(0);
    }
    
    const expectedChar = String.fromCharCode((sum % 26) + 65); // A-Z
    return lastChar === expectedChar;
  }
}

export default new LicenseService();
