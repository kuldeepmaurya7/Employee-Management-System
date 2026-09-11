using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly EmployeeDbContext _context;

        public NotificationController(
            INotificationService notificationService,
            EmployeeDbContext context)
        {
            _notificationService = notificationService;
            _context = context;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound("User not found.");

            var notifications =
                _notificationService.GetUserNotifications(user.Id);

            return View(notifications);
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAsRead(int id)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound("User not found.");

            _notificationService.MarkAsRead(
                id,
                user.Id);

            return RedirectToAction(nameof(Index));
        }
 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkAllAsRead()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = _context.Users
                .FirstOrDefault(u => u.Email == email);

            if (user == null)
                return NotFound("User not found.");

            _notificationService.MarkAllAsRead(user.Id);

            return RedirectToAction(nameof(Index));
        }
    }
}