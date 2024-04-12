using EllipticCurve;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
            var supasecretsection = _configuration.GetSection("Authentication:SuperSecretNutNut");
            if (supasecretsection["Host"] != null)
            {
                string host = supasecretsection["Host"];
                string sender = supasecretsection["Email"];
                string pass = supasecretsection["Password"];
                var client = new SmtpClient(host, 25)
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
