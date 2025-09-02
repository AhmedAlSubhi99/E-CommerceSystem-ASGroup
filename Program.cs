
using E_CommerceSystem.Repositories;
using E_CommerceSystem.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace E_CommerceSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();

            // Add services to the container.
            builder.Services.AddScoped<IUserRepo,UserRepo>();
            builder.Services.AddScoped<IUserService,UserService>();


            builder.Services.AddScoped<IProductRepo, ProductRepo>();
            builder.Services.AddScoped<IProductService, ProductService>();

            builder.Services.AddScoped<IOrderProductsRepo, OrderProductsRepo>();
            builder.Services.AddScoped<IOrderProductsService, OrderProductsService>();

            builder.Services.AddScoped<IOrderRepo, OrderRepo>();
            builder.Services.AddScoped<IOrderService, OrderService>();

            builder.Services.AddScoped<IReviewRepo, ReviewRepo>();
            builder.Services.AddScoped<IReviewService, ReviewService>();

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                 options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add JWT Authentication
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                    .AddJwtBearer(options =>
                    {
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false, // You can set this to true if you want to validate the issuer.
                            ValidateAudience = false, // You can set this to true if you want to validate the audience.
                            ValidateLifetime = true, // Ensures the token hasn't expired.
                            ValidateIssuerSigningKey = true, // Ensures the token is properly signed.
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)) // Match with your token generation key.
                        };
                    });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer <token>')",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
            });
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication(); //jwt check middleware
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
