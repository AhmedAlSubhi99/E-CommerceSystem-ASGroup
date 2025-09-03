using E_CommerceSystem.Models;

namespace E_CommerceSystem.Services
{
    public interface IReportsService
    {
        Task<IReadOnlyList<BestSellingProductDTO>> GetBestSellingProductsAsync(DateTime? from, DateTime? to, int take = 10);
        Task<IReadOnlyList<RevenueByDayDTO>> GetRevenueByDayAsync(DateTime from, DateTime to);
        Task<IReadOnlyList<RevenueByMonthDTO>> GetRevenueByMonthAsync(int year);
        Task<IReadOnlyList<TopRatedProductDTO>> GetTopRatedProductsAsync(int minReviews = 3, int take = 10);
        Task<IReadOnlyList<MostActiveCustomerDTO>> GetMostActiveCustomersAsync(DateTime? from, DateTime? to, int take = 10);
    }
}
