using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly EmployeeDbContext _context;

        public NotificationService(EmployeeDbContext context)
        {
            _context = context;
        }

        // Get notifications of a particular user
        public List<Notification> GetUserNotifications(int userId)
        {
            return _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
        }

        // Get unread notification count
        public int GetUnreadCount(int userId)
        {
            return _context.Notifications
                .Count(n => n.UserId == userId && !n.IsRead);
        }

        // Add new notification
        public void AddNotification(int userId, string message)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);

            _context.SaveChanges();
        }

        // Mark single notification as read
        public void MarkAsRead(int notificationId, int userId)
        {
            var notification = _context.Notifications
                .FirstOrDefault(n =>
                    n.Id == notificationId &&
                    n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;

                _context.SaveChanges();
            }
        }

        // Mark all notifications as read
        public void MarkAllAsRead(int userId)
        {
            var notifications = _context.Notifications
                .Where(n =>
                    n.UserId == userId &&
                    !n.IsRead)
                .ToList();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            _context.SaveChanges();
        }
    }
}