using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MyLeaveController : Controller
    {
        private readonly EmployeeDbContext _context;
        private readonly INotificationService _notificationService;

        public MyLeaveController(
            EmployeeDbContext context,
            INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var employee = _context.Employees
                .FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return NotFound("Employee record not found.");

            var leaves = _context.Leaves
                .Where(l => l.EmployeeId == employee.Id)
                .OrderByDescending(l => l.Id)
                .ToList();

            return View(leaves);
        }
 
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Leave leave)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var employee = _context.Employees
                .FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return NotFound("Employee record not found.");


            // Employee ID automatically set
            leave.EmployeeId = employee.Id;

            ModelState.Remove(nameof(Leave.EmployeeId));

 
            if (leave.EndDate < leave.StartDate)
            {
                ModelState.AddModelError(
                    "EndDate",
                    "End date cannot be before start date.");
            }
 
            leave.Status = "Pending";

 
            if (ModelState.IsValid)
            {
                _context.Leaves.Add(leave);

                _context.SaveChanges();
 
                var admin = _context.Users
                    .FirstOrDefault(u => u.Role == "Admin");

                if (admin != null)
                {
                    _notificationService.AddNotification(
                        admin.Id,
                        $"{employee.Name} has applied for {leave.LeaveType} leave."
                    );
                }


                return RedirectToAction(nameof(Index));
            }


            return View(leave);
        }
 
        [HttpGet]
        public IActionResult Reapply(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            var employee = _context.Employees
                .FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return NotFound("Employee record not found.");


            var oldLeave = _context.Leaves
                .FirstOrDefault(l =>
                    l.Id == id &&
                    l.EmployeeId == employee.Id);


            if (oldLeave == null)
                return NotFound();


            // Only rejected leave can be reapplied
            if (oldLeave.Status != "Rejected")
                return RedirectToAction(nameof(Index));


            var newLeave = new Leave
            {
                LeaveType = oldLeave.LeaveType,

                StartDate = oldLeave.StartDate,

                EndDate = oldLeave.EndDate,

                Reason = oldLeave.Reason,

                EmployeeId = employee.Id,

                Status = "Pending"
            };


            return View("Create", newLeave);
        }
    }
}