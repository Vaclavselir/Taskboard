using System;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskBoard.Api.Filters;

public sealed class AdminApiKey : IOperationFilter
{

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {

        string relPath = context.ApiDescription.RelativePath ?? "";
        
        string path = relPath.TrimStart('/');

        var isAdmin = path.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin) return;

        operation.Security ??= new List<OpenApiSecurityRequirement>();

        operation.Security.Add(new OpenApiSecurityRequirement
        {

            [new OpenApiSecurityScheme
            {

                Reference = new OpenApiReference
                {

                    Type = ReferenceType.SecurityScheme,
                    Id = "ApiKey"

                }

            }] = Array.Empty<string>()

        });

    }


}
