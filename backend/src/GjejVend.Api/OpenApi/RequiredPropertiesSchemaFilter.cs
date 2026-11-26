using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GjejVend.Api.OpenApi;

internal sealed class RequiredPropertiesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        foreach (var (name, property) in schema.Properties.Where(p => !p.Value.Nullable))
        {
            schema.Required.Add(name);
        }
    }
}
