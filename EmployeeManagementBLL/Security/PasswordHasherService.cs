using EmployeeManagementDAL.Models;
using Microsoft.AspNetCore.Identity;

public class PasswordHasherService
{
    private readonly PasswordHasher<AppUser> _passwordHasher = new PasswordHasher<AppUser>();

    public string HashPassword(AppUser user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(AppUser user, string storedHash, string enteredPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, storedHash, enteredPassword);
        return result == PasswordVerificationResult.Success;
    }
}
