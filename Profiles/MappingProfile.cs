using AutoMapper;
using E_CommerceSystem.Models;
using E_CommerceSystem.Models.DTO;

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

        // Entity -> DTO (response includes OverallRating)
        CreateMap<Product, ProductDTO>().ReverseMap();

        // DTO -> Entity for creating products
        CreateMap<ProductCreateDTO, Product>()
            .ForMember(dest => dest.PID, opt => opt.Ignore()) // PK set by DB
            .ForMember(dest => dest.OverallRating, opt => opt.Ignore()) // calculated, not client input
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // handled by service

        // DTO -> Entity for updating products
        CreateMap<ProductUpdateDTO, Product>()
            .ForMember(dest => dest.PID, opt => opt.Ignore()) // don’t overwrite PK
            .ForMember(dest => dest.OverallRating, opt => opt.Ignore()) // still calculated
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore()); // still handled by service


        // -----------------
        // User 
        // -----------------
        CreateMap<User, UserDTO>().ReverseMap();
        CreateMap<RegisterUserDTO, User>();
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
        // Entity -> DTO
        CreateMap<Review, ReviewDTO>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UID))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UName : null))
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.PID));

        // DTO -> Entity
        CreateMap<ReviewCreateDTO, Review>()
            .ForMember(dest => dest.ReviewID, opt => opt.Ignore())   // DB generates
            .ForMember(dest => dest.UID, opt => opt.Ignore())        // set in service
            .ForMember(dest => dest.PID, opt => opt.Ignore())        // set in service
            .ForMember(dest => dest.ReviewDate, opt => opt.Ignore());

        CreateMap<ReviewUpdateDTO, Review>()
            .ForMember(dest => dest.ReviewID, opt => opt.Ignore())   // never overwrite PK
            .ForMember(dest => dest.UID, opt => opt.Ignore())
            .ForMember(dest => dest.PID, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewDate, opt => opt.Ignore()); // set in service



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
