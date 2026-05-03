using EmployeeManagementDAL.Context;
using EmployeeManagementDAL.Interfaces;
using EmployeeManagementDAL.Models;
using EmployeeManagementModel.Responses;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementDAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task UpdateUser(AppUser user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<AppUser?> GetByUsername(string username)
        {
            var normalizedUsername = username.Trim().ToLower();

            return await _context.Users
                .FirstOrDefaultAsync(x => x.Username.ToLower() == normalizedUsername);
        }

        public async Task<bool> IsUsernameExists(string username)
        {
            var normalizedUsername = username.Trim().ToLower();

            return await _context.Users.AnyAsync(x => x.Username.ToLower() == normalizedUsername);
        }

        public async Task<bool> IsEmailExists(string email)
        {
            var normalizedEmail = email.Trim().ToLower();

            return await _context.Users.AnyAsync(x => x.Email.ToLower() == normalizedEmail);
        }

        public async Task<OperationResult> RegisterUser(AppUser user)
        {
            try
            {
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                return new OperationResult
                {
                    Success = true,
                    Message = "User registered successfully"
                };
            }
            catch
            {
                return new OperationResult
                {
                    Success = false,
                    Message = "Unable to register user"
                };
            }
        }

        public async Task<int> CleanupExpiredRefreshTokens()
        {
            var expiredUsers = await _context.Users
                .Where(x => x.RefreshToken != null &&
                            x.RefreshTokenExpiryTime != null &&
                            x.RefreshTokenExpiryTime <= DateTime.Now)
                .ToListAsync();

            if (!expiredUsers.Any())
                return 0;

            foreach (var user in expiredUsers)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;
            }

            await _context.SaveChangesAsync();

            return expiredUsers.Count;
        }
    }
}
