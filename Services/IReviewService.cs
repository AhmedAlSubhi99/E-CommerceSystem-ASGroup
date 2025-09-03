using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IReviewService
    {
        Review AddReview(int userId, int productId, ReviewDTO dto);
        void DeleteReview(int reviewId, int requesterUserId, bool isAdmin);
        IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int pid);
        Review? GetReviewById(int rid);
        IEnumerable<Review> GetReviewByProductId(int pid);
        Review GetReviewsByProductIdAndUserId(int pid, int uid);
        Review? UpdateReview(int rid, ReviewDTO reviewDTO);
    }
}