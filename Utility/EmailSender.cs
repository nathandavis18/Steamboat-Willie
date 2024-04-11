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
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var client = new SmtpClient("mail.nathandavis18.com", 25)
            {
                Credentials = new NetworkCredential("postmaster@nathandavis18.com", "NutNut123!"),
            };
            MailMessage msg = new MailMessage("postmaster@nathandavis18.com", email, subject, htmlMessage);
            msg.IsBodyHtml = true;

            return client.SendMailAsync(msg);
        }
    }
}
