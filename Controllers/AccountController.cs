using System.Security.Claims;
using System.Security.Cryptography;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly EmployeeDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher;

        public AccountController(EmployeeDbContext context)
        {
            _context = context;

            _passwordHasher =
                new PasswordHasher<User>();
        }
         
        // LOGIN - GET
         
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(
                    "Index",
                    "Home");
            }

            return View();
        }
          
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Email.ToLower() ==
                    model.Email.ToLower());

            if (user == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var passwordResult =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.Password,
                    model.Password);

            if (passwordResult ==
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid email or password.");

                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(
                    ClaimTypes.Name,
                    user.Name),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Role,
                    user.Role),

                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString())
            };

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            var principal =
                new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme,
                principal);

            return RedirectToAction(
                "Index",
                "Home");
        }

         
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(User user)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                var existingUser =
                    _context.Users.FirstOrDefault(u =>
                        u.Email.ToLower() ==
                        user.Email.ToLower());

                if (existingUser != null)
                {
                    ModelState.AddModelError(
                        "Email",
                        "A user with this email already exists.");
                }
            }

            if (!ModelState.IsValid)
                return View(user);

            user.Password =
                _passwordHasher.HashPassword(
                    user,
                    user.Password);

            _context.Users.Add(user);

            _context.SaveChanges();

            TempData["Success"] =
                "Registration successful. Please login.";

            return RedirectToAction(nameof(Login));
        }
 
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Email.ToLower() ==
                    model.Email.ToLower());

            /*
             * Security:
             * Don't tell the user whether the email
             * exists or not.
             */

            if (user == null)
            {
                TempData["Success"] =
                    "If an account exists for this email, " +
                    "a password reset link has been generated.";

                return RedirectToAction(
                    nameof(ForgotPassword));
            }

            // Generate secure random token
            var tokenBytes =
                RandomNumberGenerator.GetBytes(32);

            var token =
                Convert.ToBase64String(tokenBytes)
                    .Replace("+", "-")
                    .Replace("/", "_")
                    .Replace("=", "");

            // Token valid for 30 minutes
            user.PasswordResetToken = token;

            user.PasswordResetTokenExpiry =
                DateTime.UtcNow.AddMinutes(30);

            _context.SaveChanges();

            var resetUrl = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { token = token },
                Request.Scheme);

            /*
             * Learning project:
             * We display the generated reset link.
             *
             * Real production project:
             * Send this URL through an email service.
             */

            TempData["ResetLink"] = resetUrl;

            TempData["Success"] =
                "Password reset link generated. " +
                "The link is valid for 30 minutes.";

            return RedirectToAction(
                nameof(ForgotPassword));
        }

         
        [HttpGet]
        public IActionResult ResetPassword(
            string token)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return View("ResetPasswordError");
            }

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.PasswordResetToken == token);

            if (user == null)
            {
                return View(
                    "ResetPasswordError");
            }

            if (user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry <=
                DateTime.UtcNow)
            {
                return View(
                    "ResetPasswordError");
            }

            var model =
                new ResetPasswordViewModel
                {
                    Token = token
                };

            return View(model);
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.PasswordResetToken ==
                    model.Token);

            if (user == null)
            {
                return View(
                    "ResetPasswordError");
            }

            // Check token expiry
            if (user.PasswordResetTokenExpiry == null ||
                user.PasswordResetTokenExpiry <=
                DateTime.UtcNow)
            {
                return View(
                    "ResetPasswordError");
            }

            // Hash new password
            user.Password =
                _passwordHasher.HashPassword(
                    user,
                    model.NewPassword);

            // Invalidate token
            user.PasswordResetToken = null;

            user.PasswordResetTokenExpiry = null;

            _context.SaveChanges();

            TempData["Success"] =
                "Password reset successfully. " +
                "Please login with your new password.";

            return RedirectToAction(
                nameof(Login));
        }
         
        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
 
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(
            ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.FindFirst(
                ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var user = _context.Users
                .FirstOrDefault(u =>
                    u.Email == email);

            if (user == null)
                return NotFound();

            var result =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.Password,
                        model.CurrentPassword);

            if (result ==
                PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(
                    "CurrentPassword",
                    "Current password is incorrect.");

                return View(model);
            }

            user.Password =
                _passwordHasher.HashPassword(
                    user,
                    model.NewPassword);

            _context.Users.Update(user);

            _context.SaveChanges();

            TempData["Success"] =
                "Password changed successfully.";

            return RedirectToAction(
                nameof(ChangePassword));
        }
         
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults
                    .AuthenticationScheme);

            return RedirectToAction(
                nameof(Login));
        }
         
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}