using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagementSystem.Models
{
    public class Leave
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select an employee")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select an employee")]
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }

        [Required(ErrorMessage = "Please enter leave type")]
        public string LeaveType { get; set; }

        [Required(ErrorMessage = "Please select start date")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Please select end date")]
        public DateTime EndDate { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = "Pending";
         
        // LEAVE DURATION
         
        [NotMapped]
        public int DurationDays
        {
            get
            {
                if (EndDate < StartDate)
                    return 0;

                return (EndDate.Date - StartDate.Date).Days + 1;
            }
        }
    }
}