namespace EmployeeManagementSystem.Models
{
    public class DashboardViewModel
    {
        // Admin Statistics

        public int TotalEmployees { get; set; }

        public int TotalDepartments { get; set; }

        public int PendingLeaves { get; set; }

        public int ApprovedLeaves { get; set; }
        public int RejectedLeaves { get; set; }

        public decimal AverageSalary { get; set; }

        public decimal HighestSalary { get; set; }


        // Employee Statistics

        public int MyLeaves { get; set; }

        public int MyPendingLeaves { get; set; }

        public int MyApprovedLeaves { get; set; }

        public int MyRejectedLeaves { get; set; }


        // Department Chart

        public List<DepartmentEmployeeCountViewModel>
            DepartmentEmployeeCounts
        { get; set; }
            = new List<DepartmentEmployeeCountViewModel>();
    }
}