using DashboardEmployee.Data;
using DashboardEmployee.Dtos;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DashboardEmployee.Validators
{
    public sealed class EmployeeRequestValidator : AbstractValidator<EmployeeRequest>
    {

        public EmployeeRequestValidator(AppDbContext db)
        {
            RuleFor(x => x.FullName)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(e => e.Email)
                .NotEmpty()
                .EmailAddress()
                .MaximumLength(100);

            RuleFor(x => x.Salary)
                .GreaterThan(0)
                .LessThanOrEqualTo(10_000_000);

            RuleFor(x => x.DepartmentId)
             .Cascade(CascadeMode.Stop)
             .GreaterThan(0)
             .MustAsync(async (id, ct) =>
             {
                 return await db.Departments
                     .AsNoTracking()
                     .AnyAsync(d => d.DepartmentId == id, ct);
             })
             .WithMessage("Department {PropertyValue} does not exist.");
        }
    }
}
