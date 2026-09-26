using System;
using System.Collections.Generic;

namespace DashboardEmployee.Entities;

public class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public virtual ICollection<Employee> Employees { get; set; } = [];
}
