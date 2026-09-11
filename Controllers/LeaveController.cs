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
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly EmployeeDbContext _context;
        private readonly INotificationService _notificationService;

        public LeaveController(
            ILeaveService leaveService,
            EmployeeDbContext context,
            INotificationService notificationService)
        {
            _leaveService = leaveService;
            _context = context;
            _notificationService = notificationService;
        }

        // ==========================================
        // INDEX - SEARCH + STATUS FILTER + PAGINATION
        // ==========================================

        [HttpGet]
        public IActionResult Index(
            string? search,
            string? status,
            int page = 1)
        {
            int pageSize = 5;

            if (page < 1)
                page = 1;

            var query = _context.Leaves
                .Include(l => l.Employee)
                .AsQueryable();

            // Search by employee name or leave type
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(l =>
                    l.LeaveType.Contains(search) ||
                    (l.Employee != null &&
                     l.Employee.Name.Contains(search)));
            }

            // Status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l =>
                    l.Status == status);
            }

            // Total records
            int totalLeaves = query.Count();

            // Total pages
            int totalPages = (int)Math.Ceiling(
                totalLeaves / (double)pageSize);

            if (totalPages > 0 && page > totalPages)
                page = totalPages;

            // Current page
            var leaves = query
                .OrderByDescending(l => l.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new LeaveSearchViewModel
            {
                Leaves = leaves,
                Search = search,
                Status = status,
                CurrentPage = page,
                TotalPages = totalPages,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // ==========================================
        // CREATE - GET
        // ==========================================

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Employees = new SelectList(
                _context.Employees,
                "Id",
                "Name");

            return View();
        }

        // ==========================================
        // CREATE - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Leave leave)
        {
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before start date.");
            }

            if (ModelState.IsValid)
            {
                leave.Status = "Pending";

                _leaveService.AddLeave(leave);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(
                _context.Employees,
                "Id",
                "Name",
                leave.EmployeeId);

            return View(leave);
        }

        // ==========================================
        // EDIT - GET
        // ==========================================

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var leave = _leaveService.GetLeaveById(id);

            if (leave == null)
                return NotFound();

            ViewBag.Employees = new SelectList(
                _context.Employees,
                "Id",
                "Name",
                leave.EmployeeId);

            return View(leave);
        }

        // ==========================================
        // EDIT - POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(
            int id,
            Leave leave)
        {
            if (id != leave.Id)
                return NotFound();

            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before start date.");
            }

            if (ModelState.IsValid)
            {
                _leaveService.UpdateLeave(leave);

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Employees = new SelectList(
                _context.Employees,
                "Id",
                "Name",
                leave.EmployeeId);

            return View(leave);
        }

        // ==========================================
        // DELETE - GET
        // ==========================================

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var leave = _leaveService.GetLeaveById(id);

            if (leave == null)
                return NotFound();

            return View(leave);
        }

        // ==========================================
        // DELETE - POST
        // ==========================================

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var leave = _leaveService.GetLeaveById(id);

            if (leave == null)
                return NotFound();

            _leaveService.DeleteLeave(id);

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // APPROVE
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id)
        {
            var leave = _leaveService.GetLeaveById(id);

            if (leave == null)
                return NotFound();

            if (leave.Status != "Pending")
                return RedirectToAction(nameof(Index));

            _leaveService.ApproveLeave(id);

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == leave.EmployeeId);

            if (employee != null)
            {
                var employeeUser = _context.Users
                    .FirstOrDefault(u => u.Email == employee.Email);

                if (employeeUser != null)
                {
                    _notificationService.AddNotification(
                        employeeUser.Id,
                        $"Your {leave.LeaveType} leave has been approved."
                    );
                }
            }

            return RedirectToAction(nameof(Index));
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            var leave = _leaveService.GetLeaveById(id);

            if (leave == null)
                return NotFound();

            if (leave.Status != "Pending")
                return RedirectToAction(nameof(Index));

            _leaveService.RejectLeave(id);

            var employee = _context.Employees
                .FirstOrDefault(e => e.Id == leave.EmployeeId);

            if (employee != null)
            {
                var employeeUser = _context.Users
                    .FirstOrDefault(u => u.Email == employee.Email);

                if (employeeUser != null)
                {
                    _notificationService.AddNotification(
                        employeeUser.Id,
                        $"Your {leave.LeaveType} leave has been rejected."
                    );
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}