using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Please enter current password")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Please enter new password")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, number and special character")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Please confirm new password")]
        [DataType(DataType.Password)]
        [Compare(
            "NewPassword",
            ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }
    }
}