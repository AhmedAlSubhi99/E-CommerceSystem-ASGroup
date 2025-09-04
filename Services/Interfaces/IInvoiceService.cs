using System.Threading.Tasks;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates a PDF invoice asynchronously.
        /// Returns (file bytes, filename) or null if unauthorized or order not found.
        /// </summary>
        Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId, bool isAdmin);
    }
}
