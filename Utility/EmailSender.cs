using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
#nullable disable

namespace Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var emailSender = _configuration.GetSection("Authentication:EmailSender");
            if (emailSender.Exists())
            {
                string host = emailSender.GetValue(typeof(string), "Host") as string;
                string sender = emailSender.GetValue(typeof(string), "Email") as string;
                string pass = emailSender.GetValue(typeof(string), "Password") as string;
                int port = (int)emailSender.GetValue(typeof(int), "Port");
                var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(sender, pass),
                };
                MailMessage msg = new MailMessage(sender, email, subject, htmlMessage);
                msg.IsBodyHtml = true;

                return client.SendMailAsync(msg);
            }
            return Task.CompletedTask;
        }
    }
}
