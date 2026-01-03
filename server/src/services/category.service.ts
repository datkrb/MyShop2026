import categoryRepo from '../repositories/category.repo';
import { Messages } from '../constants/messages';

export class CategoryService {
  async getAll() {
    return categoryRepo.findAll();
  }

  async getById(id: number) {
    const category = await categoryRepo.findById(id);

    if (!category) {
      throw new Error(Messages.CATEGORY_NOT_FOUND);
    }

    return category;
  }

  async create(name: string, description?: string) {
    const existing = await categoryRepo.findByName(name);

    if (existing) {
      throw new Error(Messages.CATEGORY_NAME_EXISTS);
    }

    return categoryRepo.create(name, description);
  }

  async update(id: number, data: { name?: string; description?: string }) {
    const category = await categoryRepo.findById(id);

    if (!category) {
      throw new Error(Messages.CATEGORY_NOT_FOUND);
    }

    if (data.name) {
      const existing = await categoryRepo.findByName(data.name);
      if (existing && existing.id !== id) {
        throw new Error(Messages.CATEGORY_NAME_EXISTS);
      }
    }

    return categoryRepo.update(id, data);
  }

  async delete(id: number) {
    const category = await categoryRepo.findById(id);

    if (!category) {
      throw new Error(Messages.CATEGORY_NOT_FOUND);
    }

    return categoryRepo.delete(id);
  }
}

export default new CategoryService();

