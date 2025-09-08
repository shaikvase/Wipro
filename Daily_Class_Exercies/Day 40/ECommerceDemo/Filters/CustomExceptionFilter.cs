using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceDemo.Filters
{
    public class CustomExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<CustomExceptionFilter> _logger;
        public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger) => _logger = logger;

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Unhandled exception occurred.");
            context.Result = new ViewResult { ViewName = "~/Views/Shared/Error.cshtml" };
            context.ExceptionHandled = true;
        }
    }
}
