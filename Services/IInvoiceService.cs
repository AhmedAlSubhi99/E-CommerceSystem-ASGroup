using System.Threading.Tasks;

namespace E_CommerceSystem.Services
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates a PDF invoice for an order (authorized for owner or admin).
        /// Returns (bytes, filename) or null if unauthorized/not found.
        /// </summary>
        Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId, bool isAdmin);
    }
}
