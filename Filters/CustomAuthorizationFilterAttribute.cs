using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookCatalogApi.Filters;
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]

public class CustomAuthorizationFilterAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public CustomAuthorizationFilterAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.User.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var permissionClaim = context.HttpContext.User.Claims
            .FirstOrDefault(c => c.Type == "permission" && c.Value == _permission);

        if (permissionClaim == null)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
