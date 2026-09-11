using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly EmployeeDbContext _context;

        public UserController(EmployeeDbContext context)
        {
            _context = context;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            var users = _context.Users
                .OrderBy(u => u.Name)
                .ToList();

            return View(users);
        }
 
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User user)
        {
            if (id != user.Id)
                return NotFound();

            var existingUser = _context.Users
                .FirstOrDefault(u => u.Id == id);

            if (existingUser == null)
                return NotFound();

            // Password is not changed from User Management
            ModelState.Remove("Password");

            // Role validation
            if (user.Role != "Admin" &&
                user.Role != "Employee")
            {
                ModelState.AddModelError(
                    "Role",
                    "Invalid role selected.");
            }

            // Duplicate email validation
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var duplicateUser = _context.Users
                    .FirstOrDefault(u =>
                        u.Email.ToLower() ==
                        user.Email.ToLower() &&
                        u.Id != user.Id);

                if (duplicateUser != null)
                {
                    ModelState.AddModelError(
                        "Email",
                        "A user with this email already exists.");
                }
            }

            if (!ModelState.IsValid)
                return View(user);

            // Store old email before changing it
            string oldEmail = existingUser.Email;
 
            if (existingUser.Role == "Employee" &&
                !oldEmail.Equals(
                    user.Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var employee = _context.Employees
                    .FirstOrDefault(e => e.Email == oldEmail);

                if (employee != null)
                {
                    employee.Email = user.Email;
                }
            }

            // Update User
            existingUser.Name = user.Name;
            existingUser.Email = user.Email;
            existingUser.Role = user.Role;

            _context.SaveChanges();

            TempData["Success"] =
                "User updated successfully.";

            return RedirectToAction(nameof(Index));
        }
 
        [HttpGet]
        public IActionResult Delete(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            return View(user);
        }
 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var user = _context.Users
                .FirstOrDefault(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Admin cannot be deleted
            if (user.Role == "Admin")
            {
                TempData["Error"] =
                    "Admin accounts cannot be deleted.";

                return RedirectToAction(nameof(Index));
            }

            // Current logged-in user cannot delete himself
            var currentEmail = User.FindFirst(
                ClaimTypes.Email)?.Value;

            if (user.Email == currentEmail)
            {
                TempData["Error"] =
                    "You cannot delete your own account.";

                return RedirectToAction(nameof(Index));
            }

            _context.Users.Remove(user);

            _context.SaveChanges();

            TempData["Success"] =
                "Employee account deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}