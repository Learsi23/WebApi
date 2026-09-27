using DashboardEmployee.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardEmployee.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employee");

            builder.HasKey(e => e.EmployeeId);

            builder.Property(e => e.FullName)
            .HasMaxLength(150)
            .IsRequired();

            builder.Property(e => e.Email)
            .HasMaxLength(100)
            .IsRequired();

            builder.HasIndex(e => e.Email)
            .IsUnique();

            builder.Property(e => e.Salary)
            .HasPrecision(18, 2);

            builder.Property(e => e.ImageUrl)
            .HasMaxLength(300);

            builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);



            builder.HasData(SeedData.Employees);
        }
    }
}
