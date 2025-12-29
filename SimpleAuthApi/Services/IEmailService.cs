namespace SimpleAuthApi.Services
{
    public interface IEmailService
    {
        Task SendTaskNotificationAsync(List<string> emails, string taskTitle, string? attachmentPath, string adminId);

        //Task SendAssignementNotifiactionAsync(string recipientEmail, string taskTitle, string assignedBy);

        Task SendResetPasswordEmailAsync(string email, string resetLink);
    }

}
