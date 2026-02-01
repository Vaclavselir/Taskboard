using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TaskBoard.Api.Filters;

public sealed class TaskPatchSchemaFilter : ISchemaFilter
{

     public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type.Name != "UpdateTaskRequest") return;

        schema.Example = new OpenApiObject
        {
            ["title"] = new OpenApiNull(),
            ["description"] = new OpenApiNull(),
            ["dueDate"] = new OpenApiNull(),
            ["status"] = new OpenApiNull(),
            ["priority"] = new OpenApiNull()
        };
    }

}
