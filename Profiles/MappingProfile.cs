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

        // -----------------
        // Order 
        // -----------------
        CreateMap<Order, OrderSummaryDTO>();
        CreateMap<OrderProducts, OrderLineDTO>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.product.ProductName))
            .ForMember(d => d.UnitPrice, o => o.MapFrom(s => s.product.Price))
            .ForMember(d => d.Quantity, o => o.MapFrom(s => s.Quantity))
            .ForMember(d => d.LineTotal, o => o.MapFrom(s => s.Quantity * s.product.Price));

        CreateMap<OrderStatusUpdateDTO, Order>();

        // -----------------
        // OrderProduct 
        // -----------------
        CreateMap<OrderProducts, OrderItemDTO>().ReverseMap();

        // -----------------
        // Review 
        // -----------------
        CreateMap<Review, ReviewDTO>();
        CreateMap<ReviewCreateDTO, Review>();

        // ── Order → OrderSummaryDTO
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

        CreateMap<UpdateOrderStatusDTO, Order>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(s => Enum.Parse<OrderStatus>(s.Status, true)));

        // User with ignore
        CreateMap<UserDTO, User>()
            .ForMember(u => u.Password, o => o.Ignore())
            .ForMember(u => u.CreatedAt, o => o.Ignore());
    }
}
