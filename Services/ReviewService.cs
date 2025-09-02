using E_CommerceSystem.Models;
using E_CommerceSystem.Repositories;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Security.Cryptography;

namespace E_CommerceSystem.Services
{
    public class ReviewService : IReviewService
    {
        public IReviewRepo _reviewRepo;
        public IProductService _productService;
        public IOrderService _orderService;
        public IOrderProductsService _orderProductsService;
        public ReviewService(IReviewRepo reviewRepo, IProductService productService, IOrderProductsService orderProductsService, IOrderService orderService)
        {
            _reviewRepo = reviewRepo;
            _productService = productService;
            _orderProductsService = orderProductsService;
            _orderService = orderService;
        }
        public IEnumerable<Review> GetAllReviews(int pageNumber, int pageSize,int pid)
        {
            // Base query
            var query = _reviewRepo.GetReviewByProductId(pid);

            // Pagination
            var pagedProducts = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return pagedProducts;
        }

        public Review GetReviewsByProductIdAndUserId(int pid, int uid)
        {
            return _reviewRepo.GetReviewsByProductIdAndUserId(pid,uid);
        }
        public Review GetReviewById(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");
            return review;
        }

        public IEnumerable<Review> GetReviewByProductId(int pid)
        {
            return _reviewRepo.GetReviewByProductId(pid);
        }
        public void AddReview(int uid, int pid, ReviewDTO reviewDTO)
        {
            // Get all orders for the user
            var orders = _orderService.GetOrderByUserId(uid);
            foreach (var order in orders)
            {
                // Check if the product exists in any of the user's orders
                var products = _orderProductsService.GetOrdersByOrderId(order.OID);
                foreach (var product in products)
                {
                    if (product != null && product.PID == pid)
                    {
                        // Check if the user has already added a review for this product
                        var existingReview = GetReviewsByProductIdAndUserId(pid,uid);
                        
                        if (existingReview != null)
                            throw new InvalidOperationException($"You have already reviewed this product.");

                        //add review
                        var review = new Review
                        {
                            PID = pid,
                            UID = uid,
                            Comment = reviewDTO.Comment,
                            Rating = reviewDTO.Rating,
                            ReviewDate = DateTime.Now
                        };
                        _reviewRepo.AddReview(review);

                        // Recalculate and update the product's overall rating
                        RecalculateProductRating(pid);
                    }
                    //else
                    //    throw new KeyNotFoundException($"You have not ordered this product");

                }
            }
        }
        public void UpdateReview(int rid, ReviewDTO reviewDTO)
        {
            var review = GetReviewById(rid);

            review.ReviewDate = DateTime.Now;
            review.Rating = reviewDTO.Rating;
            review.Comment = reviewDTO.Comment;

            _reviewRepo.UpdateReview(review);
            RecalculateProductRating(review.Rating);
        }
        public void DeleteReview(int rid)
        {
            var review = _reviewRepo.GetReviewById(rid);
            if (review == null)
                throw new KeyNotFoundException($"Review with ID {rid} not found.");

            _reviewRepo.DeleteReview(rid);
            RecalculateProductRating(review.PID);
        }

        private void RecalculateProductRating(int pid)
        {
            // get all reviews for the product
            var reviews = _reviewRepo.GetAllReviews();
            
            var product = _productService.GetProductById(pid);

            // Calculate the average rating
            var averageRating = reviews.Average(r => r.Rating);

            // Update the product's overall rating (convert double to decimal)
            product.OverallRating = Convert.ToDecimal(averageRating);

            // Save the updated product
            _productService.UpdateProduct(product);
        }
    }
}
