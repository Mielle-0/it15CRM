using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using AmazonReviewsCRM.Security;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireRoleAccessAttribute : Attribute, IAsyncActionFilter
{
    private readonly string _menuKey;

    public RequireRoleAccessAttribute(string menuKey)
    {
        _menuKey = menuKey;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var user = context.HttpContext.User;
        var role = user.FindFirst("role")?.Value;

        if (string.IsNullOrEmpty(role) || !RolePerms.HasAccess(role, _menuKey))
        {
            context.Result = new ForbidResult(); // 403 Forbidden
            return;
        }

        await next(); // proceed if allowed
    }
}
