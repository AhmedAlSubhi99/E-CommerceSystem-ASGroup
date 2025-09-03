namespace E_CommerceSystem.Services
{
    public interface IEmailService
    {
        void SendOrderPlacedEmail(string toEmail, int orderId, decimal total);
        void SendOrderCancelledEmail(string toEmail, int orderId);
    }
}
