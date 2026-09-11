using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDepartmentService _departmentService;
        private readonly EmployeeDbContext _context;

        public EmployeeController(
            IEmployeeService employeeService,
            IDepartmentService departmentService,
            EmployeeDbContext context)
        {
            _employeeService = employeeService;
            _departmentService = departmentService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index(
            string? search,
            int? departmentId,
            int page = 1)
        {
            int pageSize = 5;

            if (page < 1)
                page = 1;

            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            // Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(e =>
                    e.Name.Contains(search) ||
                    e.Email.Contains(search) ||
                    e.Phone.Contains(search));
            }

            // Department filter
            if (departmentId.HasValue)
            {
                query = query.Where(e =>
                    e.DepartmentId == departmentId.Value);
            }

            int totalEmployees = query.Count();

            int totalPages = (int)Math.Ceiling(
                totalEmployees / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            var employees = query
                .OrderBy(e => e.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new EmployeeSearchViewModel
            {
                Employees = employees,
                Departments =
                    _departmentService.GetAllDepartments(),

                Search = search,
                DepartmentId = departmentId,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }
         
        [HttpGet]
        public IActionResult Details(int id)
        {
            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Id == id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }
 
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Departments = new SelectList(
                _departmentService.GetAllDepartments(),
                "Id",
                "Name");

            return View();
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Employee employee)
        {
            // Joining Date validation
            if (employee.JoiningDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "JoiningDate",
                    "Joining date cannot be a future date.");
            }

            // Duplicate email validation
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var existingEmployee = _context.Employees
                    .FirstOrDefault(e =>
                        e.Email.ToLower() ==
                        employee.Email.ToLower());

                if (existingEmployee != null)
                {
                    ModelState.AddModelError(
                        "Email",
                        "An employee with this email already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                _employeeService.AddEmployee(employee);

                TempData["Success"] =
                    "Employee created successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(
                _departmentService.GetAllDepartments(),
                "Id",
                "Name",
                employee.DepartmentId);

            return View(employee);
        }
 
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var employee =
                _employeeService.GetEmployeeById(id);

            if (employee == null)
                return NotFound();

            ViewBag.Departments = new SelectList(
                _departmentService.GetAllDepartments(),
                "Id",
                "Name",
                employee.DepartmentId);

            return View(employee);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            Employee employee)
        {
            if (id != employee.Id)
                return NotFound();

            // Joining Date validation
            if (employee.JoiningDate.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    "JoiningDate",
                    "Joining date cannot be a future date.");
            }

            // Duplicate email validation
            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var existingEmployee = _context.Employees
                    .FirstOrDefault(e =>
                        e.Email.ToLower() ==
                        employee.Email.ToLower() &&
                        e.Id != employee.Id);

                if (existingEmployee != null)
                {
                    ModelState.AddModelError(
                        "Email",
                        "An employee with this email already exists.");
                }
            }

            if (ModelState.IsValid)
            {
                _employeeService.UpdateEmployee(employee);

                TempData["Success"] =
                    "Employee updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Departments = new SelectList(
                _departmentService.GetAllDepartments(),
                "Id",
                "Name",
                employee.DepartmentId);

            return View(employee);
        }
 
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var employee =
                _employeeService.GetEmployeeById(id);

            if (employee == null)
                return NotFound();

            return View(employee);
        }
         
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var employee =
                _employeeService.GetEmployeeById(id);

            if (employee == null)
                return NotFound();

            _employeeService.DeleteEmployee(id);

            TempData["Success"] =
                "Employee deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}