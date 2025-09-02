using E_CommerceSystem.Models;

namespace E_CommerceSystem.Repositories
{
    public interface IReviewRepo
    {
        void AddReview(Review review);
        void DeleteReview(int rid);
        IEnumerable<Review> GetAllReviews();
        Review GetReviewById(int rid);
        void UpdateReview(Review review);
        Review GetReviewsByProductIdAndUserId(int pid, int uid);
        IEnumerable<Review> GetReviewByProductId(int pid);
    }
}