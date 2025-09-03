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
        CreateMap<CategoryCreateDTO, Category>();
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
        CreateMap<ProductCreateDTO, Product>();
        CreateMap<ProductUpdateDTO, Product>();

        // -----------------
        // User 
        // -----------------
        CreateMap<User, LoginResponseDTO>();
        CreateMap<UserDTO, User>()
            .ForMember(u => u.Password, o => o.Ignore())
            .ForMember(u => u.CreatedAt, o => o.Ignore());

        // -----------------
        // Orders 
        // -----------------
        CreateMap<OrderProducts, OrderLineDTO>()
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.product.ProductName))
            .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
            .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.product.Price))
            .ForMember(d => d.LineTotal, m => m.MapFrom(s => s.Quantity * s.product.Price));

        CreateMap<Order, OrderSummaryDTO>()
            .ForMember(d => d.OrderId, m => m.MapFrom(s => s.OID))
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.user.UName))
            .ForMember(d => d.OrderDate, m => m.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Lines, m => m.MapFrom(s => s.OrderProducts))
            .ForMember(d => d.TotalAmount, m => m.MapFrom(s => s.OrderProducts.Sum(op => op.Quantity * op.product.Price)));

        CreateMap<Order, OrdersOutputDTO>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        // Choose one DTO for status update
        CreateMap<UpdateOrderStatusDTO, Order>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(s => Enum.Parse<OrderStatus>(s.Status, true)));

        // -----------------
        // Review 
        // -----------------
        CreateMap<Review, ReviewDTO>();
        CreateMap<ReviewCreateDTO, Review>();

        // -----------------
        // OrderItem 
        // -----------------
        CreateMap<OrderProducts, OrderItemDTO>().ReverseMap();
    }
}
