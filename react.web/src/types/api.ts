// TypeScript copies of the C# DTOs in DashboardEmployee/Dtos.
// If a DTO changes in the API, change it here too.


export type Employee = {
    id: number
    fullName: string
    email: string
    salary: number
    imageUrl: string | null
    departmentId: number
    departmentName: string
}
export type EmployeeRequest = {
    fullName: string
    email: string
    salary: number
    departmentId: number
}

export type SortField = 'name' | 'email' | 'salary' | 'department'
export type SortDirection = 'asc' | 'desc'

export type EmployeeQuery = {
    search: string
    departmentId: number | null
    sortBy: SortField
    sortDirection: SortDirection
    page: number
    pageSize: number
}
export type PagedResult<T> = {
    items: T[]
    page: number
    pageSize: number
    totalCount: number
    totalPages: number
}

export type Department = {
    id: number
    name: string
    employeeCount: number
    createdAt: string
}

export type DepartmentRequest = {
    name: string
}
export type DepartmentStats = {
    id: number
    name: string
    employeeCount: number
    totalSalary: number
    averageSalary: number
}
export type DashboardStats = {
    totalEmployees: number
    totalDepartments: number
    totalPayroll: number
    averageSalary: number
    highestSalary: number
    lowestSalary: number
    departments: DepartmentStats[]
}

export type LoginRequest = {
    email: string
    password: string
}
export type CurrentUser = {
    email: string
    roles: string[]
}