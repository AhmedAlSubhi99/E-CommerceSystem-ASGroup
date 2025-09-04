using E_CommerceSystem.Models.DTO;

namespace E_CommerceSystem.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDTO>> GetAllReviewsAsync(int pageNumber, int pageSize, int productId);
        Task<ReviewDTO?> GetReviewByIdAsync(int reviewId);

        Task<ReviewDTO> AddReviewAsync(int userId, int productId, ReviewCreateDTO dto);
        Task<ReviewDTO> UpdateReviewAsync(int reviewId, ReviewUpdateDTO dto);
        Task DeleteReviewAsync(int reviewId, int requesterUserId, bool isAdmin);
    }
}
