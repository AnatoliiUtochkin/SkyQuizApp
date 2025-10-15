using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SkyQuizApp.Configurations;
using SkyQuizApp.Services.Interfaces;

namespace SkyQuizApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.Username, _smtp.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtp.Username, "SkyQuiz App"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            await client.SendMailAsync(mailMessage);
        }
    }
}