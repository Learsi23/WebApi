using DashboardEmployee.Dtos;
using FluentValidation;

namespace DashboardEmployee.Validators;

public sealed class DepartmentRequestValidator : AbstractValidator<DepartmentRequest>
{
    // NotEmpty() avvisar null , "" och även " ". MaximumLength(100) matchar kolumnen: ett längre namn skulle ge ett SQL-fel
    public DepartmentRequestValidator()
    {
        RuleFor(x => x.Name)
        .NotEmpty()
        .MaximumLength(100);
    }
}