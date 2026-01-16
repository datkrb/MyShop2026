import productRepo from '../repositories/product.repo';
import { Messages } from '../constants/messages';
import fs from 'fs';
import path from 'path';

export class ProductService {
  async getAll(filters: any, userRole?: string) {
    const result = await productRepo.findAll(filters);

    // SALE role cannot see importPrice
    if (userRole !== 'ADMIN') {
      result.data = result.data.map((product: any) => {
        const { importPrice, ...productWithoutImportPrice } = product;
        return productWithoutImportPrice;
      });
    }

    return result;
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

  async addImages(productId: number, files: Express.Multer.File[]) {
    const imageData = files.map(file => ({
      productId,
      url: `/uploads/products/${file.filename}`
    }));

    return productRepo.addImages(imageData);
  }

  async deleteImage(id: number) {
    const image = await productRepo.findImageById(id);
    if (!image) {
      throw new Error('Image not found');
    }

    const relativePath = image.url.startsWith('/') ? image.url.substring(1) : image.url;
    const absolutePath = path.resolve(process.cwd(), relativePath);

    if (fs.existsSync(absolutePath)) {
      try {
        fs.unlinkSync(absolutePath);
      } catch (err) {
        console.error('Failed to delete image file:', err);
      }
    }

    return productRepo.deleteImage(id);
  }

  async getStats() {
    return productRepo.getStats();
  }
}

export default new ProductService();

