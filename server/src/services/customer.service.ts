import customerRepo from '../repositories/customer.repo';
import { Messages } from '../constants/messages';

export class CustomerService {
  async getAll(filters: any) {
    return customerRepo.findAll(filters);
  }

  async getById(id: number) {
    const customer = await customerRepo.findById(id);

    if (!customer) {
      throw new Error(Messages.CUSTOMER_NOT_FOUND);
    }

    return customer;
  }

  async create(data: {
    name: string;
    phone?: string;
    email?: string;
    address?: string;
  }) {
    return customerRepo.create(data);
  }

  async update(id: number, data: {
    name?: string;
    phone?: string;
    email?: string;
    address?: string;
  }) {
    const customer = await customerRepo.findById(id);

    if (!customer) {
      throw new Error(Messages.CUSTOMER_NOT_FOUND);
    }

    return customerRepo.update(id, data);
  }

  async delete(id: number) {
    const customer = await customerRepo.findById(id);

    if (!customer) {
      throw new Error(Messages.CUSTOMER_NOT_FOUND);
    }

    return customerRepo.delete(id);
  }
}

export default new CustomerService();

