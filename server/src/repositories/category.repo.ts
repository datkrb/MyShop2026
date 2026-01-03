import prisma from '../config/prisma';

export class CategoryRepository {
  async findAll() {
    return prisma.category.findMany({
      orderBy: { name: 'asc' },
    });
  }

  async findById(id: number) {
    return prisma.category.findUnique({
      where: { id },
      include: {
        products: {
          take: 5,
        },
      },
    });
  }

  async findByName(name: string) {
    return prisma.category.findFirst({
      where: { name },
    });
  }

  async create(name: string, description?: string) {
    return prisma.category.create({
      data: {
        name,
        description,
      },
    });
  }

  async update(id: number, data: { name?: string; description?: string }) {
    return prisma.category.update({
      where: { id },
      data,
    });
  }

  async delete(id: number) {
    return prisma.category.delete({
      where: { id },
    });
  }
}

export default new CategoryRepository();

