namespace E_CommerceSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task SendOrderPlacedEmail(string to, int orderId, decimal totalAmount);
        Task SendOrderCancelledEmail(string to, int orderId);
    }
}
