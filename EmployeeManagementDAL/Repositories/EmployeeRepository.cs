using EmployeeManagementDAL.Context;
using EmployeeManagementDAL.Interfaces;
using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementDAL.Repositories
{

    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly AppDbContext _context;

        public EmployeeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResultDTO<Employee>> GetAll(EmployeeQueryParametersDTO query)
        {
            var employeesQuery = _context.Employees.AsQueryable();

            // ===== SEARCH =====
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();

                employeesQuery = employeesQuery.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    x.Email.ToLower().Contains(search));
            }

            // ===== FILTER BY DEPARTMENT =====
            if (!string.IsNullOrWhiteSpace(query.Department))
            {
                var dept = query.Department.ToLower();

                employeesQuery = employeesQuery.Where(x =>
                    x.Department.ToLower() == dept);
            }

            // ===== SORTING =====
            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                switch (query.SortBy.ToLower())
                {
                    case "name":
                        employeesQuery = query.SortOrder?.ToLower() == "desc"
                            ? employeesQuery.OrderByDescending(x => x.Name)
                            : employeesQuery.OrderBy(x => x.Name);
                        break;

                    case "salary":
                        employeesQuery = query.SortOrder?.ToLower() == "desc"
                            ? employeesQuery.OrderByDescending(x => x.Salary)
                            : employeesQuery.OrderBy(x => x.Salary);
                        break;

                    case "department":
                        employeesQuery = query.SortOrder?.ToLower() == "desc"
                            ? employeesQuery.OrderByDescending(x => x.Department)
                            : employeesQuery.OrderBy(x => x.Department);
                        break;

                    default:
                        employeesQuery = employeesQuery.OrderBy(x => x.Id);
                        break;
                }
            }
            else
            {
                employeesQuery = employeesQuery.OrderBy(x => x.Id);
            }

            // ===== TOTAL COUNT =====
            var totalRecords = await employeesQuery.CountAsync();

            // ===== PAGINATION =====
            var employees = await employeesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResultDTO<Employee>
            {
                CurrentPage = query.PageNumber,
                PageSize = query.PageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)query.PageSize),
                Data = employees
            };
        }

        public async Task<Employee?> GetById(int id)
        {
            return await _context.Employees.FindAsync(id);
        }

        public async Task<Employee?> GetByEmail(string email)
        {
            return await _context.Employees
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        public async Task Add(Employee emp)
        {
            await _context.Employees.AddAsync(emp);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateByEmail(Employee emp)
        {
            var existingEmployee = await _context.Employees
                .FirstOrDefaultAsync(x => x.Email.ToLower() == emp.Email.ToLower());

            if (existingEmployee == null)
                return false;

            existingEmployee.Name = emp.Name;
            existingEmployee.Salary = emp.Salary;
            existingEmployee.Department = emp.Department;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteByEmail(string email)
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(x => x.Email.ToLower() == email.ToLower());

            if (employee == null)
                return false;

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
