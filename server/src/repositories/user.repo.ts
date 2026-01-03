import prisma from '../config/prisma';
import { UserRole } from '../constants/roles';

export class UserRepository {
  async findByUsername(username: string) {
    return prisma.user.findUnique({
      where: { username },
    });
  }

  async findById(id: number) {
    return prisma.user.findUnique({
      where: { id },
      select: {
        id: true,
        username: true,
        role: true,
      },
    });
  }

  async create(username: string, password: string, role: UserRole) {
    return prisma.user.create({
      data: {
        username,
        password,
        role,
      },
      select: {
        id: true,
        username: true,
        role: true,
      },
    });
  }
}

export default new UserRepository();

