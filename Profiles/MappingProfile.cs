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
        CreateMap<Order, OrdersOutputOTD>().ReverseMap();

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
