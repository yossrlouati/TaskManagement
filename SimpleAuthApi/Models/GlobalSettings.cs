namespace SimpleAuthApi.Models
{
    public class GlobalSettings
    {
        public int Id { get; set; }
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 587;
        public string SenderEmail { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public bool IsEmailNotificationEnabled { get; set; } = true;
        public string CompanyName { get; set; } = "Nouvelle Entreprise";
    }
}
