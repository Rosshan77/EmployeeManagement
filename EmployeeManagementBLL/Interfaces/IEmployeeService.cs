using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementBLL.Interfaces
{
    public interface IEmployeeService
    {
        Task<PagedResultDTO<EmployeeDTO>> GetAll(EmployeeQueryParametersDTO query);
        Task<EmployeeDTO?> GetById(int id);
        Task<EmployeeDTO?> GetByEmail(string email);
        Task<bool> Add(EmployeeDTO emp);
        Task<bool> UpdateByEmail(EmployeeDTO emp);
        Task<bool> DeleteByEmail(string email);
    }
}
