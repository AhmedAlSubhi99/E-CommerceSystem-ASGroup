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
        CreateMap<OrderProducts, OrderLineDTO>()
            .ForMember(d => d.ProductId, m => m.MapFrom(s => s.PID))
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.product.ProductName))
            .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
            .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.product.Price))
            .ForMember(d => d.LineTotal, m => m.MapFrom(s => s.Quantity * s.product.Price));


        CreateMap<Order, OrderSummaryDTO>()
            .ForMember(d => d.OrderId, m => m.MapFrom(s => s.OID))
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.user.UName))
            .ForMember(d => d.CreatedAt, m => m.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Lines, m => m.MapFrom(s => s.OrderProducts))
            .ForMember(d => d.Subtotal, m => m.MapFrom(s => s.OrderProducts.Sum(op => op.Quantity * op.product.Price)))
            .ForMember(d => d.Total, m => m.MapFrom(s => s.OrderProducts.Sum(op => op.Quantity * op.product.Price)));


        CreateMap<Order, OrderDTO>()
       .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<UpdateOrderStatusDTO, Order>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => Enum.Parse<OrderStatus>(s.Status, true)))
            .ForAllOtherMembers(opt => opt.Ignore());
    }
}
