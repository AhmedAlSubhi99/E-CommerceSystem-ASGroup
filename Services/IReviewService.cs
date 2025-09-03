using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IReviewService
    {
        Review AddReview(int userId, int productId, ReviewDTO dto);
        bool DeleteReview(int rid);
        IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int pid);
        ReviewDTO? GetReviewById(int rid);
        IEnumerable<Review> GetReviewByProductId(int pid);
        Review GetReviewsByProductIdAndUserId(int pid, int uid);
        ReviewDTO? UpdateReview(int rid, ReviewDTO reviewDTO);
    }
}