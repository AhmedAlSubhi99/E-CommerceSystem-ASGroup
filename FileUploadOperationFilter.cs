using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace E_CommerceSystem
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Check if any parameter is ProductCreateDTO or ProductUpdateDTO (with file)
            var hasFileUpload = context.MethodInfo
                .GetParameters()
                .Any(p => p.ParameterType == typeof(E_CommerceSystem.Models.DTO.ProductCreateDTO) ||
                          p.ParameterType == typeof(E_CommerceSystem.Models.DTO.ProductUpdateDTO) ||
                          p.ParameterType == typeof(IFormFile));

            if (!hasFileUpload) return;

            // Clear default schema and build multipart/form-data schema
            operation.Parameters.Clear();
            operation.RequestBody = new OpenApiRequestBody
            {
                Content =
                {
                    ["multipart/form-data"] = new OpenApiMediaType
                    {
                        Schema = new OpenApiSchema
                        {
                            Type = "object",
                            Properties = new Dictionary<string, OpenApiSchema>
                            {
                                ["productName"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Product name"
                                },
                                ["description"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Description = "Product description"
                                },
                                ["price"] = new OpenApiSchema
                                {
                                    Type = "number",
                                    Format = "decimal",
                                    Description = "Product price"
                                },
                                ["stockQuantity"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Product stock quantity"
                                },
                                ["categoryId"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Category ID"
                                },
                                ["supplierId"] = new OpenApiSchema
                                {
                                    Type = "integer",
                                    Format = "int32",
                                    Description = "Supplier ID"
                                },
                                ["imageFile"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Product image file (JPG/PNG)"
                                }
                            },
                            Required = new HashSet<string>
                            {
                                "productName", "price", "stockQuantity", "categoryId", "supplierId"
                            }
                        }
                    }
                }
            };
        }
    }
}
