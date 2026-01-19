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

  async findAllPaginated(filters: {
    page?: number;
    size?: number;
    search?: string;
    role?: string;
  }) {
    const page = filters.page || 1;
    const size = filters.size || 10;
    const skip = (page - 1) * size;

    const where: any = {};

    if (filters.search) {
      where.username = {
        contains: filters.search,
        mode: 'insensitive',
      };
    }

    if (filters.role) {
      where.role = filters.role;
    }

    const [data, total] = await Promise.all([
      prisma.user.findMany({
        where,
        select: {
          id: true,
          username: true,
          role: true,
          createdAt: true,
        },
        orderBy: {
          createdAt: 'desc',
        },
        skip,
        take: size,
      }),
      prisma.user.count({ where }),
    ]);

    return {
      data,
      total,
      page,
      size,
      totalPages: Math.ceil(total / size),
    };
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

