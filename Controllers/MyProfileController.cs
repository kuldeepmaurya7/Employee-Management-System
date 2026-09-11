using System.Security.Claims;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Employee")]
    public class MyProfileController : Controller
    {
        private readonly EmployeeDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MyProfileController(
            EmployeeDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
 
        [HttpGet]
        public IActionResult Index()
        {
            var email = User.FindFirstValue(
                ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return NotFound(
                    "Employee profile not found.");

            return View(employee);
        }
 
        [HttpGet]
        public IActionResult Edit()
        {
            var email = User.FindFirstValue(
                ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var employee = _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Email == email);

            if (employee == null)
                return NotFound(
                    "Employee profile not found.");

            return View(employee);
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Employee employee,
            IFormFile? ProfileImage)
        {
             
            var email = User.FindFirstValue(
                ClaimTypes.Email);

            if (string.IsNullOrEmpty(email))
                return Unauthorized();
 
            var existingEmployee = _context.Employees
                .FirstOrDefault(e => e.Email == email);

            if (existingEmployee == null)
                return NotFound(
                    "Employee profile not found.");
 
            ModelState.Remove(
                nameof(Employee.DepartmentId));

            ModelState.Remove(
                nameof(Employee.Salary));

            ModelState.Remove(
                nameof(Employee.JoiningDate));

            ModelState.Remove(
                nameof(Employee.Email));

 
            existingEmployee.Name =
                employee.Name;

            existingEmployee.Phone =
                employee.Phone;
 
            if (ProfileImage != null &&
                ProfileImage.Length > 0)
            { 
                const long maxFileSize =
                    2 * 1024 * 1024;

                if (ProfileImage.Length >
                    maxFileSize)
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Profile image size cannot exceed 2 MB.");

                    return View(employee);
                }

 
                var extension =
                    Path.GetExtension(
                        ProfileImage.FileName)
                        .ToLowerInvariant();

                var allowedExtensions =
                    new[]
                    {
                        ".jpg",
                        ".jpeg",
                        ".png",
                        ".webp"
                    };

                if (!allowedExtensions.Contains(
                    extension))
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Only JPG, JPEG, PNG and WEBP images are allowed.");

                    return View(employee);
                }
 
                var allowedMimeTypes =
                    new Dictionary<string, string>
                    {
                        { ".jpg", "image/jpeg" },
                        { ".jpeg", "image/jpeg" },
                        { ".png", "image/png" },
                        { ".webp", "image/webp" }
                    };

                if (!allowedMimeTypes.TryGetValue(
                    extension,
                    out var expectedMimeType))
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Invalid image type.");

                    return View(employee);
                }

                if (!string.Equals(
                    ProfileImage.ContentType,
                    expectedMimeType,
                    StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Invalid image MIME type.");

                    return View(employee);
                }
                 
                if (string.IsNullOrWhiteSpace(
                    ProfileImage.FileName))
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Please select a valid image.");

                    return View(employee);
                }

 
                var uploadsFolder =
                    Path.Combine(
                        _environment.WebRootPath,
                        "uploads");

                if (!Directory.Exists(
                    uploadsFolder))
                {
                    Directory.CreateDirectory(
                        uploadsFolder);
                }

 
                var fileName =
                    $"{Guid.NewGuid():N}{extension}";

                var filePath =
                    Path.Combine(
                        uploadsFolder,
                        fileName);
                 
                try
                {
                    using (var stream =
                           new FileStream(
                               filePath,
                               FileMode.CreateNew))
                    {
                        await ProfileImage
                            .CopyToAsync(stream);
                    }
                }
                catch
                {
                    ModelState.AddModelError(
                        "ProfileImage",
                        "Unable to upload the image. Please try again.");

                    return View(employee);
                }

                 
                if (!string.IsNullOrEmpty(
                    existingEmployee.ProfileImage))
                {
                    var oldImagePath =
                        Path.Combine(
                            _environment.WebRootPath,
                            existingEmployee
                                .ProfileImage
                                .TrimStart('/')
                                .Replace(
                                    '/',
                                    Path.DirectorySeparatorChar));

                    if (System.IO.File.Exists(
                        oldImagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(
                                oldImagePath);
                        }
                        catch
                        {
                            // Ignore old image delete error
                        }
                    }
                }

                 
                existingEmployee.ProfileImage =
                    "/uploads/" + fileName;
            }

 
            if (!ModelState.IsValid)
                return View(employee);

 
            await _context.SaveChangesAsync();

 
            TempData["Success"] =
                "Profile updated successfully.";

            return RedirectToAction(
                nameof(Index));
        }
    }
}