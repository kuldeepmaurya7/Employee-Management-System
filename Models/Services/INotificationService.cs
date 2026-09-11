using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public interface INotificationService
    {
        List<Notification> GetUserNotifications(int userId);

        int GetUnreadCount(int userId);

        void AddNotification(int userId, string message);

        void MarkAsRead(int notificationId, int userId);

        void MarkAllAsRead(int userId);
    }
}