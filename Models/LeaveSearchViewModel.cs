using System.Collections.Generic;

namespace EmployeeManagementSystem.Models
{
    public class LeaveSearchViewModel
    {
        public List<Leave> Leaves { get; set; }
            = new List<Leave>();

        public string? Search { get; set; }

        public string? Status { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int PageSize { get; set; } = 5;
    }
}