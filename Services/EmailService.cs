using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace E_CommerceSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        private SmtpClient CreateClient()
        {
            var smtpSettings = _config.GetSection("Smtp");
            return new SmtpClient
            {
                Host = smtpSettings["Host"],
                Port = int.Parse(smtpSettings["Port"]),
                EnableSsl = bool.Parse(smtpSettings["EnableSsl"]),
                Credentials = new NetworkCredential(
                    smtpSettings["User"], smtpSettings["Password"])
            };
        }

        public void SendOrderPlacedEmail(string toEmail, int orderId, decimal total)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_config["Smtp:User"], "E-Commerce System"),
                Subject = $"Order #{orderId} Placed",
                Body = $"Your order #{orderId} has been placed successfully. Total: {total:C}.",
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = CreateClient();
            client.Send(message);
        }

        public void SendOrderCancelledEmail(string toEmail, int orderId)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_config["Smtp:User"], "E-Commerce System"),
                Subject = $"Order #{orderId} Cancelled",
                Body = $"Your order #{orderId} has been cancelled. If this wasn’t you, please contact support.",
                IsBodyHtml = false
            };
            message.To.Add(toEmail);

            using var client = CreateClient();
            client.Send(message);
        }
    }
}
