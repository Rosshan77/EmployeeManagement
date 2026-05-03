using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementDAL.Interfaces
{
    public interface IEmployeeRepository
    {
        Task<PagedResultDTO<Employee>> GetAll(EmployeeQueryParametersDTO query);
        Task<Employee?> GetById(int id);
        Task<Employee?> GetByEmail(string email);
        Task Add(Employee emp);
        Task<bool> UpdateByEmail(Employee emp);
        Task<bool> DeleteByEmail(string email);
    }
}
