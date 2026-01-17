import userRepo from '../repositories/user.repo';
import { comparePassword } from '../utils/hash';
import { generateToken, generateRefreshToken, verifyRefreshToken } from '../utils/jwt';
import { Messages } from '../constants/messages';

export class AuthService {
  async login(username: string, password: string) {
    const user = await userRepo.findByUsername(username);

    if (!user) {
      throw new Error(Messages.INVALID_CREDENTIALS);
    }

    const isValid = await comparePassword(password, user.password);

    if (!isValid) {
      throw new Error(Messages.INVALID_CREDENTIALS);
    }

    const payload = {
      userId: user.id,
      username: user.username,
      role: user.role,
    };

    const accessToken = generateToken(payload);
    const refreshToken = generateRefreshToken(payload);

    return {
      accessToken,
      refreshToken,
      role: user.role,
      expiresIn: 3600, // 1 hour
    };
  }

  async refreshToken(token: string) {
    try {
      const decoded = verifyRefreshToken(token);
      
      // Optional: Check if user exists (in case user was deleted)
      const user = await userRepo.findById(decoded.userId);
      if (!user) {
        throw new Error(Messages.USER_NOT_FOUND);
      }

      const payload = {
        userId: user.id,
        username: user.username,
        role: user.role,
      };

      const accessToken = generateToken(payload);

      return {
        accessToken,
        expiresIn: 3600,
      };
    } catch (error) {
      throw new Error('Invalid refresh token');
    }
  }

  async getCurrentUser(userId: number) {
    const user = await userRepo.findById(userId);

    if (!user) {
      throw new Error(Messages.USER_NOT_FOUND);
    }

    return {
      id: user.id,
      username: user.username,
      role: user.role,
    };
  }
}

export default new AuthService();

