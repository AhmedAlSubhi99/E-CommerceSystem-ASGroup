using E_CommerceSystem.Models;
using E_CommerceSystem.Services;

namespace E_CommerceSystem.Repositories
{
    public class ReviewRepo : IReviewRepo
    {
        public ApplicationDbContext _context;
        public ReviewRepo(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Review> GetAllReviews()
        {
            try
            {
                return _context.Reviews.ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public Review GetReviewById(int rid)
        {
            try
            {
                return _context.Reviews.FirstOrDefault(r => r.ReviewID == rid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
        public Review GetReviewsByProductIdAndUserId(int pid,int uid)
        {
            try
            {
                return _context.Reviews.FirstOrDefault(r => r.PID == pid && r.UID ==uid);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void AddReview(Review review)
        {
            try
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void UpdateReview(Review review)
        {
            try
            {

                _context.Reviews.Update(review);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public void DeleteReview(int rid)
        {
            try
            {
                var review = GetReviewById(rid);
                if (review != null)
                {
                    _context.Reviews.Remove(review);
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
        {
            try
            {
                return _context.Reviews.Where(r => r.PID ==pid).ToList();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Database error: {ex.Message}");
            }
        }
    }
}
