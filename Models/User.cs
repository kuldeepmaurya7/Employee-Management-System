using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter password")]
        [MinLength(
            8,
            ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            ErrorMessage =
                "Password must contain uppercase, lowercase, number and special character")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; } = "Employee";

        // Password Reset Security
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}