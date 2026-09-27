using DashboardEmployee.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DashboardEmployee.Data.Configurations
{
    public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
    {
        public void Configure(EntityTypeBuilder<Department> builder)
        {
            builder.ToTable("Departments"); // Convención habitual en plural para tablas

            builder.HasKey(d => d.DepartmentId);

            builder.Property(d => d.Name)
                .HasMaxLength(100)
                .IsRequired();

            // Evita departamentos con nombres duplicados en la base de datos
            builder.HasIndex(d => d.Name)
                .IsUnique();

            builder.Property(d => d.CreatedAt)
                .HasDefaultValueSql("SYSUTCDATETIME()");

            //FAKE DATA

            builder.HasData(SeedData.Departments);
        }
    }
}