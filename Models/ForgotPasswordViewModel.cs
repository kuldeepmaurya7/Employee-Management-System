using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Please enter email")]
        [EmailAddress(
            ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }
    }
}