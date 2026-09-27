using DashboardEmployee.Entities;

namespace DashboardEmployee.Data
{
    public static class SeedData
    {
        /// <summary>
        /// Fixed demo data inserted by the migrations. Values must be constants:
        /// a changing value (like DateTime.UtcNow) would produce a new migration every time.
        /// </summary>
        /// 
        private static readonly DateTimeOffset SeedDate = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static readonly Department[] Departments =
        [
        new() { DepartmentId = 1, Name = "Technology & Development", CreatedAt = SeedDate },
        new() { DepartmentId = 2, Name = "Human Resources", CreatedAt = SeedDate },
        new() { DepartmentId = 3, Name = "Finance", CreatedAt = SeedDate },
        ];

        public static readonly Employee[] Employees =
        [
        new() { EmployeeId = 1, FullName = "Carlos Mendoza", Email = "carlos.mendoza@company.se",
        Salary = 55000, ImageUrl = "/images/avatar1.jpg", DepartmentId = 1 },
        new() { EmployeeId = 2, FullName = "Åsa Öberg", Email = "asa.oberg@company.se", Salary =
        48000, ImageUrl = "/images/avatar2.jpg", DepartmentId = 2 },
        new() { EmployeeId = 3, FullName = "José Martínez", Email = "jose.martinez@company.se",
        Salary = 62000, ImageUrl = "/images/avatar3.jpg", DepartmentId = 1 },
        new() { EmployeeId = 4, FullName = "Linnea Sjöberg", Email = "linnea.sjoberg@company.se",
        Salary = 51000, ImageUrl = "/images/avatar4.jpg", DepartmentId = 3 },
        new() { EmployeeId = 5, FullName = "Erik Lindqvist", Email = "erik.lindqvist@company.se",
        Salary = 58500, ImageUrl = "/images/avatar5.jpg", DepartmentId = 1 },
        new() { EmployeeId = 6, FullName = "Sofía Hernández", Email = "sofia.hernandez@company.se",
        Salary = 46000, ImageUrl = "/images/avatar6.jpg", DepartmentId = 3 },
        ];
    }
}

