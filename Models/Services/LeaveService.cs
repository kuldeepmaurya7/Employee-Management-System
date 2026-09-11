using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services
{
    public class LeaveService : ILeaveService
    {
        private readonly EmployeeDbContext _context;

        public LeaveService(EmployeeDbContext context)
        {
            _context = context;
        }

        public List<Leave> GetAllLeaves()
        {
            return _context.Leaves
                .Include(l => l.Employee)
                .ToList();
        }

        public Leave? GetLeaveById(int id)
        {
            return _context.Leaves
                .Include(l => l.Employee)
                .FirstOrDefault(l => l.Id == id);
        }

        public void AddLeave(Leave leave)
        {
            _context.Leaves.Add(leave);
            _context.SaveChanges();
        }

        public void UpdateLeave(Leave leave)
        {
            _context.Leaves.Update(leave);
            _context.SaveChanges();
        }

        public void DeleteLeave(int id)
        {
            var leave = _context.Leaves
                .FirstOrDefault(l => l.Id == id);

            if (leave != null)
            {
                _context.Leaves.Remove(leave);
                _context.SaveChanges();
            }
        }

        public void ApproveLeave(int id)
        {
            var leave = _context.Leaves
                .FirstOrDefault(l => l.Id == id);

            if (leave != null)
            {
                leave.Status = "Approved";
                _context.SaveChanges();
            }
        }

        public void RejectLeave(int id)
        {
            var leave = _context.Leaves
                .FirstOrDefault(l => l.Id == id);

            if (leave != null)
            {
                leave.Status = "Rejected";
                _context.SaveChanges();
            }
        }
    }
}