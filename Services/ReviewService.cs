using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;

namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepo _reviewRepo;
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IOrderProductsService _orderProductsService;
        private readonly IMapper _mapper;

        public ReviewService(
            IReviewRepo reviewRepo,
            IProductService productService,
            IOrderProductsService orderProductsService,
            IOrderService orderService,
            IMapper mapper)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
            _mapper = mapper;
        }

        // Keep return type as in entities.
        public IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize, int pid)
        {
            var query = _reviewRepo.GetReviewByProductId(pid);

            var pagedReviews = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedReviews;
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
            => _reviewRepo.GetReviewsByProductIdAndUserId(pid, uid);

        public ReviewDTO GetReviewById(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            return _mapper.Map<ReviewDTO>(review);
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
            => _reviewRepo.GetReviewByProductId(pid);

        // === Uses AutoMapper (DTO -> Entity -> DTO) ===
        public ReviewDTO AddReview(int uid, int pid, ReviewDTO reviewDTO)
        {
            // 1) Verify the user purchased this product
            var orders = _orderService.GetOrderByUserId(uid);
            var purchased = false;

            foreach (var order in orders)
            {
                var items = _orderProductsService.GetOrdersByOrderId(order.OID);
                if (items.Any(it => it != null && it.PID == pid))
                {
                    purchased = true;
                    break;
                }
            }

            if (!purchased)
                throw new InvalidOperationException("You can only review products you have purchased.");

            // 2) Enforce single review per (user, product)
            var existingReview = GetReviewsByProductIdAndUserId(pid, uid);
            if (existingReview != null)
                throw new InvalidOperationException("You have already reviewed this product.");

            // 3) Map DTO -> Entity
            var review = _mapper.Map<Review>(reviewDTO);
            review.PID = pid;
            review.UID = uid;
            review.ReviewDate = DateTime.Now;

            _reviewRepo.AddReview(review);

            // 4) Recalculate product rating for this product only
            RecalculateProductRating(pid);

            // 5) Return the created review as DTO
            return _mapper.Map<ReviewDTO>(review);
        }

        public ReviewDTO UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            // copy incoming fields to tracked entity
            _mapper.Map(reviewDTO, review);
            review.ReviewDate = DateTime.Now;

            _reviewRepo.UpdateReview(review);

            // Recalculate rating for this product
            RecalculateProductRating(review.PID);

            // return updated DTO
            return _mapper.Map<ReviewDTO>(review);
        }

        public bool DeleteReview(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null) return false;

            _reviewRepo.DeleteReview(rid);

            // refresh product rating
            RecalculateProductRating(review.PID);

            return true;
        }

        private void RecalculateProductRating(int pid)
        {
            // Only reviews for this product
            var reviews = _reviewRepo.GetReviewByProductId(pid).ToList();

            var product = _productService.GetProductById(pid);
            if (product == null)
                throw new KeyNotFoundException($"Product with ID {pid} not found.");

            var averageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0.0;

            // If OverallRating is decimal in your Product model, convert safely
            product.OverallRating = Convert.ToDecimal(averageRating);

            var dto = _mapper.Map<ProductDTO>(product);
            _productService.UpdateProduct(product.PID, dto, null);
        }
    }
}
