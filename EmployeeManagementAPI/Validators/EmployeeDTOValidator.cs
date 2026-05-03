using EmployeeManagementModel.DTOs;
using FluentValidation;

namespace EmployeeManagementAPI.Validators
{
    public class EmployeeDTOValidator : AbstractValidator<EmployeeDTO>
    {
        public EmployeeDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Employee name is required");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Employee email is required")
                .EmailAddress().WithMessage("Invalid employee email format");

            RuleFor(x => x.Department)
                .NotEmpty().WithMessage("Department is required");

            RuleFor(x => x.Salary)
                .GreaterThan(0).WithMessage("Salary must be greater than zero");
        }
    }
}