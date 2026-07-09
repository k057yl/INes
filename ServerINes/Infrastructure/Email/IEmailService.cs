using System.Threading.Tasks;

namespace INest.Infrastructure.Email
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);
        System.Threading.Tasks.Task SendLendingNotificationAsync(
            string toEmail,
            string itemName,
            string personName,
            DateTime? returnDate,
            bool isBorrowedByMe);
        Task SendReminderNotificationAsync(string toEmail, string title, DateTime triggerAt);
    }
}
