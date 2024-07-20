using DevBlog.WebApp.Models;

namespace DevBlog.WebApp.Services
{
    public interface IEmailSender
    {
        Task SendEmail(EmailData emailData);
    }
}
