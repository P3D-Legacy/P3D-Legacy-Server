using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

using System.Collections.Generic;

namespace P3D.Legacy.Server.Utils
{
    public class CorrelationIdHeaderSwaggerAttribute : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = CorrelationId.CorrelationIdOptions.DefaultHeader,
                Description = "Correlation Id for the request",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Format = "uuid", Example = new OpenApiString("dd7d8481-81a3-407f-95f0-a2f1cb382a4b") }
            });
        }
    }
}