using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly EmployeeDbContext _context;

        public DashboardController(
            EmployeeDbContext context)
        {
            _context = context;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            var model = new DashboardViewModel();

 
            if (User.IsInRole("Admin"))
            {
                model.TotalEmployees =
                    _context.Employees.Count();

                model.TotalDepartments =
                    _context.Departments.Count();

                model.PendingLeaves =
                    _context.Leaves
                        .Count(l => l.Status == "Pending");

                model.ApprovedLeaves =
                    _context.Leaves
                        .Count(l => l.Status == "Approved");


                // Average Salary

                model.AverageSalary =
                    _context.Employees
                        .Select(e => (decimal?)e.Salary)
                        .Average() ?? 0;


                // Highest Salary

                model.HighestSalary =
                    _context.Employees
                        .Select(e => (decimal?)e.Salary)
                        .Max() ?? 0;

                 model.DepartmentEmployeeCounts =
                    _context.Departments
                        .Select(d => new DepartmentEmployeeCountViewModel
                        {
                            DepartmentName = d.Name,

                            EmployeeCount =
                                d.Employees.Count()
                        })
                        .OrderByDescending(
                            d => d.EmployeeCount)
                        .ToList();
            }
 
            if (User.IsInRole("Employee"))
            {
                var email = User.FindFirstValue(
                    ClaimTypes.Email);

                var employee =
                    _context.Employees
                        .FirstOrDefault(
                            e => e.Email == email);

                if (employee != null)
                {
                    var myLeaves =
                        _context.Leaves
                            .Where(l =>
                                l.EmployeeId ==
                                employee.Id);


                    model.MyLeaves =
                        myLeaves.Count();

                    model.MyPendingLeaves =
                        myLeaves.Count(
                            l => l.Status == "Pending");

                    model.MyApprovedLeaves =
                        myLeaves.Count(
                            l => l.Status == "Approved");

                    model.MyRejectedLeaves =
                        myLeaves.Count(
                            l => l.Status == "Rejected");
                }
            }


            return View(model);
        }
    }
}