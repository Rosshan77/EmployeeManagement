using EmployeeManagementDAL.Models;
using EmployeeManagementModel.Responses;

namespace EmployeeManagementDAL.Interfaces
{
    public interface IUserRepository
    {
        Task<AppUser?> GetByUsername(string username);
        Task<OperationResult> RegisterUser(AppUser user);
        Task<bool> IsUsernameExists(string username);
        Task<bool> IsEmailExists(string email);
        Task UpdateUser(AppUser user);
        Task<int> CleanupExpiredRefreshTokens();
    }
}
