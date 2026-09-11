using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }


       

        [Required(ErrorMessage = "Please enter employee name")]
        public string Name { get; set; }


         
        [Required(ErrorMessage = "Please enter employee email")]
        [EmailAddress(ErrorMessage = "Please enter a valid email")]
        public string Email { get; set; }

 
        [Required(ErrorMessage = "Please enter phone number")]
        [RegularExpression(
            @"^[6-9][0-9]{9}$",
            ErrorMessage = "Please enter a valid 10-digit mobile number")]
        public string Phone { get; set; }

 

        [Range(
            1,
            int.MaxValue,
            ErrorMessage = "Please select a department")]
        public int DepartmentId { get; set; }


        public Department? Department { get; set; }


        

        [Range(
            1,
            double.MaxValue,
            ErrorMessage = "Salary must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

 

        [Required(ErrorMessage = "Please select joining date")]
        public DateTime JoiningDate { get; set; }

 

        public string? ProfileImage { get; set; }
    }
}