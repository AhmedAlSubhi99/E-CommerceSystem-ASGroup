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
        CreateMap<CategoryUpdateDto, Category>()
            .ForMember(dest => dest.CategoryId, opt => opt.Ignore());

        // -----------------
        // Supplier 
        // -----------------
        CreateMap<Supplier, SupplierDTO>().ReverseMap();
        CreateMap<SupplierCreateDto, Supplier>();
        CreateMap<SupplierUpdateDto, Supplier>()
            .ForMember(dest => dest.SupplierId, opt => opt.Ignore());

        // -----------------
        // Product 
        // -----------------
        CreateMap<ProductCreateDTO, Product>();
        CreateMap<ProductUpdateDTO, Product>()
            .ForMember(dest => dest.PID, opt => opt.Ignore());

        // -----------------
        // User 
        // -----------------
        CreateMap<User, UserDTO>().ReverseMap();
        CreateMap<UserDTO, User>()
            .ForMember(u => u.Password, o => o.Ignore())
            .ForMember(u => u.CreatedAt, o => o.Ignore());
        CreateMap<User, LoginResponseDTO>();

        // -----------------
        // Orders 
        // -----------------
        CreateMap<OrderProducts, OrderLineDTO>()
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.Product.ProductName))
            .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
            .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.UnitPrice)) // use snapshot price
            .ForMember(d => d.LineTotal, m => m.MapFrom(s => s.Quantity * s.UnitPrice));

        CreateMap<Order, OrderSummaryDTO>()
            .ForMember(d => d.OrderId, m => m.MapFrom(s => s.OID))
            .ForMember(d => d.CustomerName, m => m.MapFrom(s => s.User.UName))
            .ForMember(d => d.CustomerEmail, m => m.MapFrom(s => s.User.Email))
            .ForMember(d => d.OrderDate, m => m.MapFrom(s => s.OrderDate))
            .ForMember(d => d.Status, m => m.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Lines, m => m.MapFrom(s => s.OrderProducts))
            .ForMember(d => d.TotalAmount, m => m.MapFrom(s => s.OrderProducts.Sum(op => op.Quantity * op.UnitPrice)));

        CreateMap<Order, OrdersOutputDTO>()
            .ForMember(d => d.Status, opt => opt.MapFrom(s => s.Status.ToString()));

        CreateMap<UpdateOrderStatusDTO, Order>()
            .ForMember(d => d.Status, opt =>
                opt.MapFrom(s => Enum.Parse<OrderStatus>(s.Status, true)))
            .ForMember(d => d.OID, opt => opt.Ignore());

        // -----------------
        // Review 
        // -----------------
        CreateMap<Review, ReviewDTO>();
        CreateMap<ReviewCreateDTO, Review>()
            .ForMember(d => d.ReviewID, opt => opt.Ignore());

        // -----------------
        // OrderItem 
        // -----------------
        CreateMap<OrderProducts, OrderItemDTO>()
            .ForMember(d => d.ProductName, m => m.MapFrom(s => s.Product.ProductName))
            .ForMember(d => d.Quantity, m => m.MapFrom(s => s.Quantity))
            .ForMember(d => d.UnitPrice, m => m.MapFrom(s => s.UnitPrice))
            .ForMember(d => d.LineTotal, m => m.MapFrom(s => s.Quantity * s.UnitPrice));
    }
}
