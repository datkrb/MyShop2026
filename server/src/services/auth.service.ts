import userRepo from '../repositories/user.repo';
import { comparePassword } from '../utils/hash';
import { generateToken } from '../utils/jwt';
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

    const token = generateToken({
      userId: user.id,
      username: user.username,
      role: user.role,
    });

    return {
      token,
      role: user.role,
      expiresIn: 3600, // 1 hour in seconds
    };
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

