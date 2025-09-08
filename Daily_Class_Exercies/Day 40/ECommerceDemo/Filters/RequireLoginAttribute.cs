using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceDemo.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireLoginAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var isAuth = context.HttpContext.Session.GetString("IsAuthenticated");
            if (string.IsNullOrEmpty(isAuth))
            {
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
            }
        }
    }
}
