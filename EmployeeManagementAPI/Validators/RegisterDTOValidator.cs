using EmployeeManagementModel.Constants;
using EmployeeManagementModel.DTOs;
using FluentValidation;

namespace EmployeeManagementAPI.Validators
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(20).WithMessage("Username cannot exceed 20 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(4).WithMessage("Password must be at least 4 characters");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(role => role.Equals(Roles.Admin, StringComparison.OrdinalIgnoreCase)
                           || role.Equals(Roles.User, StringComparison.OrdinalIgnoreCase))
                .WithMessage("Role must be Admin or User");
        }
    }
}