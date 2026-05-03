using EmployeeManagementDAL.Models;
using EmployeeManagementModel.DTOs;
using EmployeeManagementModel.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementBLL.Interfaces
{
    public interface IUserService
    {
        Task<OperationResult<LoginResponseDTO>> Login(LoginDTO login);
        Task<OperationResult> RegisterUser(RegisterDTO register);
        Task<OperationResult<LoginResponseDTO>> RefreshToken(RefreshTokenRequestDTO request);
    }
}
