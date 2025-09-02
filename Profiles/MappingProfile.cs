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

        // ── Order → OrderSummaryDTO
        CreateMap<Order, OrderSummaryDTO>()
            .ForMember(d => d.OrderId, m => m.MapFrom(s => s.OID))
            .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status))
            .ForMember(d => d.Total, m => m.MapFrom(s => s.TotalAmount))
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.user))                
            .ForMember(d => d.Lines, m => m.MapFrom(s => s.OrderProducts));       

        // ── OrderProducts → OrderLineDTO
        CreateMap<OrderProducts, OrderLineDTO>()
            .ForMember(d => d.ProductId, m => m.MapFrom(s => s.PID))
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.product))
            .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
            .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.UnitPrice))
            .ForMember(d => d.LineTotal, m => m.MapFrom(s => s.TotalPrice));

    }
}
