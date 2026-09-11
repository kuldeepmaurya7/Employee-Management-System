using System.Collections.Generic;

namespace EmployeeManagementSystem.Models
{
    public class DepartmentSearchViewModel
    {
        public List<Department> Departments { get; set; }
            = new List<Department>();

        public string? Search { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; } = 5;
    }
}