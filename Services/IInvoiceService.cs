using System.Threading.Tasks;

namespace E_CommerceSystem.Services
{
    public interface IInvoiceService
    {
        /// <summary>
        /// Generates a PDF invoice (sync).
        /// </summary>
        byte[]? GenerateInvoice(int orderId, int requestUserId, bool isAdmin);

        /// <summary>
        /// Generates a PDF invoice (async).
        /// Returns (bytes, filename) or null.
        /// </summary>
        Task<(byte[] Bytes, string FileName)?> GeneratePdfAsync(int orderId, int requestUserId);
    }
}
