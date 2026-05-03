using EmployeeManagementModel.DTOs;
using FluentValidation;

namespace EmployeeManagementAPI.Validators
{
    public class RefreshTokenRequestDTOValidator : AbstractValidator<RefreshTokenRequestDTO>
    {
        public RefreshTokenRequestDTOValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
}