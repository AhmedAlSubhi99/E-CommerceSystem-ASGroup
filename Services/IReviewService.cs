using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IReviewService
    {
        void AddReview(int uid, int pid, ReviewDTO reviewDTO);
        void DeleteReview(int rid);
        IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int pid);
        Review GetReviewById(int rid);
        IEnumerable<Review> GetReviewByProductId(int pid);
        Review GetReviewsByProductIdAndUserId(int pid, int uid);
        void UpdateReview(int rid, ReviewDTO reviewDTO);
    }
}