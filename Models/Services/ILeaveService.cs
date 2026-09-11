using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public interface ILeaveService
    {
        List<Leave> GetAllLeaves();

        Leave? GetLeaveById(int id);

        void AddLeave(Leave leave);

        void UpdateLeave(Leave leave);

        void DeleteLeave(int id);

        void ApproveLeave(int id);

        void RejectLeave(int id);
    }
}