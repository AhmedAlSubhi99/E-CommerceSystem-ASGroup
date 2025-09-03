using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace E_CommerceSystem
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Detect endpoints with ProductDTO + IFormFile
            var hasFileUpload = context.MethodInfo
                .GetParameters()
                .Any(p => p.ParameterType == typeof(IFormFile)
                       || p.ParameterType.Name.Contains("ProductDTO"));

            if (hasFileUpload)
            {
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
                                // ✅ ProductDTO fields
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
                                // ✅ Image upload field
                                ["imageFile"] = new OpenApiSchema
                                {
                                    Type = "string",
                                    Format = "binary",
                                    Description = "Product image file"
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
}
