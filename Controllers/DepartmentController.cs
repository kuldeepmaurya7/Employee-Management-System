using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly IDepartmentService _departmentService;
        private readonly EmployeeDbContext _context;

        public DepartmentController(
            IDepartmentService departmentService,
            EmployeeDbContext context)
        {
            _departmentService = departmentService;
            _context = context;
        }

 
        [HttpGet]
        public IActionResult Index(
            string? search,
            int page = 1)
        {
            int pageSize = 5;

            if (page < 1)
                page = 1;

            var query = _context.Departments
                .Include(d => d.Employees)
                .AsQueryable();


            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(d =>
                    d.Name.Contains(search) ||
                    d.Description.Contains(search));
            }


            int totalDepartments = query.Count();

            int totalPages = (int)Math.Ceiling(
                totalDepartments / (double)pageSize);


            if (totalPages > 0 && page > totalPages)
                page = totalPages;


            var departments = query
                .OrderBy(d => d.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();


            var viewModel = new DepartmentSearchViewModel
            {
                Departments = departments,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };


            return View(viewModel);
        }
 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Department department)
        {
            // Duplicate department name check

            var existingDepartment = _context.Departments
                .FirstOrDefault(d =>
                    d.Name.ToLower() == department.Name.ToLower());


            if (existingDepartment != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A department with this name already exists.");
            }


            if (ModelState.IsValid)
            {
                _departmentService.AddDepartment(department);

                TempData["Success"] =
                    "Department created successfully.";

                return RedirectToAction(nameof(Index));
            }


            return View(department);
        }
 

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var department =
                _departmentService.GetDepartmentById(id);


            if (department == null)
                return NotFound();


            return View(department);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            Department department)
        {
            if (id != department.Id)
                return NotFound();


            // Duplicate name check
            // Current department ko ignore karega

            var existingDepartment = _context.Departments
                .FirstOrDefault(d =>
                    d.Name.ToLower() == department.Name.ToLower() &&
                    d.Id != department.Id);


            if (existingDepartment != null)
            {
                ModelState.AddModelError(
                    "Name",
                    "A department with this name already exists.");
            }


            if (ModelState.IsValid)
            {
                _departmentService.UpdateDepartment(department);

                TempData["Success"] =
                    "Department updated successfully.";

                return RedirectToAction(nameof(Index));
            }


            return View(department);
        }
 
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var department =
                _departmentService.GetDepartmentById(id);


            if (department == null)
                return NotFound();


            if (_departmentService.HasEmployees(id))
            {
                TempData["Error"] =
                    "This department cannot be deleted because employees are assigned to it.";

                return RedirectToAction(nameof(Index));
            }


            return View(department);
        }
 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var department =
                _departmentService.GetDepartmentById(id);


            if (department == null)
                return NotFound();


            // Safety check before deleting

            if (_departmentService.HasEmployees(id))
            {
                TempData["Error"] =
                    "This department cannot be deleted because employees are assigned to it.";

                return RedirectToAction(nameof(Index));
            }


            _departmentService.DeleteDepartment(id);


            TempData["Success"] =
                "Department deleted successfully.";


            return RedirectToAction(nameof(Index));
        }
    }
}