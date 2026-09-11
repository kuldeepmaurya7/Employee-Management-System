using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly EmployeeDbContext _context;

        public HomeController(EmployeeDbContext context)
        {
            _context = context;
        }
 
        public IActionResult Index()
        {
            var dashboard = new DashboardViewModel();

 
            if (User.IsInRole("Admin"))
            {
                dashboard.TotalEmployees =
                    _context.Employees.Count();


                dashboard.TotalDepartments =
                    _context.Departments.Count();


                dashboard.PendingLeaves =
                    _context.Leaves
                        .Count(l => l.Status == "Pending");


                dashboard.ApprovedLeaves =
                    _context.Leaves
                        .Count(l => l.Status == "Approved");


                dashboard.RejectedLeaves =
                    _context.Leaves
                        .Count(l => l.Status == "Rejected");


                dashboard.AverageSalary =
                    _context.Employees
                        .Select(e => (decimal?)e.Salary)
                        .Average() ?? 0;


                dashboard.HighestSalary =
                    _context.Employees
                        .Select(e => (decimal?)e.Salary)
                        .Max() ?? 0;

                dashboard.DepartmentEmployeeCounts =
                  _context.Departments
                    .Select(d => new DepartmentEmployeeCountViewModel
                    {
                          DepartmentName = d.Name,
                            EmployeeCount = d.Employees.Count()
                    })
                    .OrderByDescending(d => d.EmployeeCount)
                    .ToList();
            }

 
            if (User.IsInRole("Employee"))
            {
                var email =
                    User.FindFirstValue(ClaimTypes.Email);


                var employee =
                    _context.Employees
                        .FirstOrDefault(e => e.Email == email);


                if (employee != null)
                {
                    dashboard.MyLeaves =
                        _context.Leaves
                            .Count(l =>
                                l.EmployeeId == employee.Id);


                    dashboard.MyPendingLeaves =
                        _context.Leaves
                            .Count(l =>
                                l.EmployeeId == employee.Id &&
                                l.Status == "Pending");


                    dashboard.MyApprovedLeaves =
                        _context.Leaves
                            .Count(l =>
                                l.EmployeeId == employee.Id &&
                                l.Status == "Approved");


                    dashboard.MyRejectedLeaves =
                        _context.Leaves
                            .Count(l =>
                                l.EmployeeId == employee.Id &&
                                l.Status == "Rejected");
                }
            }


            return View(dashboard);
        }
    }
}