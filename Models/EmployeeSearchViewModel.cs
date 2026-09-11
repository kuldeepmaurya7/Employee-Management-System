using System.Collections.Generic;

namespace EmployeeManagementSystem.Models
{
    public class EmployeeSearchViewModel
    {
        public List<Employee> Employees { get; set; }
            = new List<Employee>();

        public List<Department> Departments { get; set; }
            = new List<Department>();

        public string? Search { get; set; }

        public int? DepartmentId { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; } = 5;
    }
}