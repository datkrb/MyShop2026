import userRepo from '../repositories/user.repo';
import { hashPassword } from '../utils/hash';
import { UserRole } from '../constants/roles';
import { Messages } from '../constants/messages';

export class EmployeeService {
  async getAll() {
    return userRepo.findAll();
  }

  async getById(id: number) {
    const employee = await userRepo.findById(id);
    if (!employee) {
      throw new Error(Messages.USER_NOT_FOUND);
    }
    return employee;
  }

  async create(username: string, password: string, role: UserRole) {
    // Check if username already exists
    const exists = await userRepo.existsByUsername(username);
    if (exists) {
      throw new Error('Username already exists');
    }

    // Hash password
    const hashedPassword = await hashPassword(password);

    return userRepo.create(username, hashedPassword, role);
  }

  async update(id: number, data: { username?: string; password?: string; role?: UserRole }) {
    // Check if user exists
    const employee = await userRepo.findById(id);
    if (!employee) {
      throw new Error(Messages.USER_NOT_FOUND);
    }

    // Check username uniqueness if changing
    if (data.username && data.username !== employee.username) {
      const exists = await userRepo.existsByUsername(data.username, id);
      if (exists) {
        throw new Error('Username already exists');
      }
    }

    // Hash password if provided and not empty
    const updateData: { username?: string; password?: string; role?: UserRole } = {};
    
    if (data.username) {
      updateData.username = data.username;
    }
    
    if (data.password && data.password.length > 0) {
      updateData.password = await hashPassword(data.password);
    }
    
    if (data.role) {
      updateData.role = data.role;
    }

    return userRepo.update(id, updateData);
  }

  async delete(id: number, currentUserId: number) {
    // Prevent self-deletion
    if (id === currentUserId) {
      throw new Error('Cannot delete your own account');
    }

    // Check if user exists
    const employee = await userRepo.findById(id);
    if (!employee) {
      throw new Error(Messages.USER_NOT_FOUND);
    }

    return userRepo.delete(id);
  }
}

export default new EmployeeService();
