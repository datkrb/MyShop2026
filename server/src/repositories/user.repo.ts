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

  async findAll() {
    return prisma.user.findMany({
      select: {
        id: true,
        username: true,
        role: true,
        createdAt: true,
      },
      orderBy: {
        createdAt: 'desc',
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

  async update(id: number, data: { username?: string; password?: string; role?: UserRole }) {
    return prisma.user.update({
      where: { id },
      data,
      select: {
        id: true,
        username: true,
        role: true,
      },
    });
  }

  async delete(id: number) {
    return prisma.user.delete({
      where: { id },
    });
  }

  async existsByUsername(username: string, excludeId?: number) {
    const user = await prisma.user.findUnique({
      where: { username },
      select: { id: true },
    });
    
    if (!user) return false;
    if (excludeId && user.id === excludeId) return false;
    return true;
  }
}

export default new UserRepository();

