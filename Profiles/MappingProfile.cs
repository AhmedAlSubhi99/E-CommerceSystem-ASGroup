using AutoMapper;
using E_CommerceSystem.Models;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // -----------------
        // Category 
        // -----------------
        CreateMap<Category, CategoryDTO>().ReverseMap();
        CreateMap<CategoryCreateDto, Category>();
        CreateMap<CategoryUpdateDto, Category>();

        // -----------------
        // Supplier 
        // -----------------
        CreateMap<Supplier, SupplierDTO>().ReverseMap();
        CreateMap<SupplierCreateDto, Supplier>();
        CreateMap<SupplierUpdateDto, Supplier>();

        // -----------------
        // Product 
        // -----------------
        CreateMap<Product, ProductDTO>().ReverseMap();

        // -----------------
        // User 
        // -----------------
        CreateMap<User, UserDTO>().ReverseMap();

        // -----------------
        // Order 
        // -----------------
        CreateMap<Order, OrdersOutputDTO>().ReverseMap();
        CreateMap<OrderProducts, OrdersOutputDTO>()
            .ForMember(d => d.ProductName, opt => opt.MapFrom(s => s.product.ProductName))
            .ForMember(d => d.Quantity, opt => opt.MapFrom(s => s.Quantity))
            .ForMember(d => d.OrderDate, opt => opt.MapFrom(s => s.Order.OrderDate))
            .ForMember(d => d.TotalAmount, opt => opt.MapFrom(s => s.Quantity * s.product.Price));
        // -----------------
        // OrderProduct 
        // -----------------
        CreateMap<OrderProducts, OrderItemDTO>().ReverseMap();

        // -----------------
        // Review 
        // -----------------
        CreateMap<Review, ReviewDTO>().ReverseMap();
    }
}
