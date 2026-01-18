import { Router } from 'express';
import employeeController from '../controllers/employee.controller';
import { validate } from '../middlewares/validate.middleware';
import { createEmployeeDto, updateEmployeeDto } from '../dtos/employee.dto';

const router = Router();

// GET /employees - Get all employees
router.get('/', employeeController.getAll.bind(employeeController));

// GET /employees/:id - Get employee by ID
router.get('/:id', employeeController.getById.bind(employeeController));

// POST /employees - Create new employee
router.post('/', validate(createEmployeeDto), employeeController.create.bind(employeeController));

// PUT /employees/:id - Update employee
router.put('/:id', validate(updateEmployeeDto), employeeController.update.bind(employeeController));

// DELETE /employees/:id - Delete employee
router.delete('/:id', employeeController.delete.bind(employeeController));

export default router;
