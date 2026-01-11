import productRepo from '../repositories/product.repo';
import { Messages } from '../constants/messages';

export class ProductService {
  async getAll(filters: any) {
    return productRepo.findAll(filters);
  }

  async getById(id: number, userRole?: string) {
    const product = await productRepo.findById(id);

    if (!product) {
      throw new Error(Messages.PRODUCT_NOT_FOUND);
    }

    // SALE role cannot see importPrice
    if (userRole !== 'ADMIN') {
      const { importPrice, ...productWithoutImportPrice } = product;
      return productWithoutImportPrice;
    }

    return product;
  }

  async create(data: {
    name: string;
    sku: string;
    importPrice: number;
    salePrice: number;
    stock: number;
    categoryId: number;
    description?: string;
  }) {
    const existing = await productRepo.findBySku(data.sku);

    if (existing) {
      throw new Error(Messages.PRODUCT_SKU_EXISTS);
    }

    return productRepo.create(data);
  }

  async update(id: number, data: {
    name?: string;
    sku?: string;
    importPrice?: number;
    salePrice?: number;
    stock?: number;
    categoryId?: number;
    description?: string;
  }) {
    const product = await productRepo.findById(id);

    if (!product) {
      throw new Error(Messages.PRODUCT_NOT_FOUND);
    }

    if (data.sku && data.sku !== product.sku) {
      const existing = await productRepo.findBySku(data.sku);
      if (existing) {
        throw new Error(Messages.PRODUCT_SKU_EXISTS);
      }
    }

    return productRepo.update(id, data);
  }

  async delete(id: number) {
    const product = await productRepo.findById(id);

    if (!product) {
      throw new Error(Messages.PRODUCT_NOT_FOUND);
    }

    return productRepo.delete(id);
  }

  async getLowStock(limit: number = 5) {
    return productRepo.findLowStock(limit);
  }

  async getTopSelling(limit: number = 5) {
    return productRepo.findTopSelling(limit);
  }

  async getStats() {
    return productRepo.getStats();
  }
}

export default new ProductService();

