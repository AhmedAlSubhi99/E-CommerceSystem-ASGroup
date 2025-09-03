using E_CommerceSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace E_CommerceSystem.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = _settings.UseSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
        }
        public async Task SendOrderPlacedEmail(string to, int orderId, decimal totalAmount)
        {
            var subject = $"Order #{orderId} Confirmation";
            var body = $@"
        <h3>Thank you for your order!</h3>
        <p>Your order <strong>#{orderId}</strong> has been placed successfully.</p>
        <p>Total Amount: <strong>{totalAmount:C}</strong></p>
        <p>We’ll notify you once it’s shipped.</p>";

            await SendEmailAsync(to, subject, body);
        }

        public async Task SendOrderCancelledEmail(string to, int orderId)
        {
            var subject = $"Order #{orderId} Cancelled";
            var body = $@"
        <h3>Order Cancelled</h3>
        <p>Your order <strong>#{orderId}</strong> has been cancelled as requested.</p>";

            await SendEmailAsync(to, subject, body);
        }

    }
}
