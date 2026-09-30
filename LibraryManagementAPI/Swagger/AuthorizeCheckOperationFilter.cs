using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LibraryManagementAPI.Swagger;

public class AuthorizeCheckDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument document, DocumentFilterContext context)
    {
        foreach (var apiDescription in context.ApiDescriptions)
        {
            var methodInfo = apiDescription.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor cad
                ? cad.MethodInfo
                : null;

            if (methodInfo is null)
                continue;

            var hasAuthorize = (methodInfo.DeclaringType?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() ?? false)
                || methodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any();

            var hasAllowAnonymous = methodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any();

            if (!hasAuthorize || hasAllowAnonymous)
                continue;

            var path = "/" + apiDescription.RelativePath;
            var httpMethod = apiDescription.HttpMethod;

            if (httpMethod is null || !document.Paths.TryGetValue(path, out var pathItem))
                continue;

            var operation = pathItem.Operations
                .FirstOrDefault(kvp => string.Equals(kvp.Key.ToString(), httpMethod, StringComparison.OrdinalIgnoreCase))
                .Value;

            if (operation is null)
                continue;

            operation.Security = new List<OpenApiSecurityRequirement>
            {
                new()
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer", document),
                        new List<string>()
                    }
                }
            };
        }
    }
}
