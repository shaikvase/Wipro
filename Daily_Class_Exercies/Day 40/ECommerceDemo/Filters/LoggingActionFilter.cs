using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerceDemo.Filters
{
    public class LoggingActionFilter : IActionFilter
    {
        private readonly ILogger<LoggingActionFilter> _logger;
        public LoggingActionFilter(ILogger<LoggingActionFilter> logger) => _logger = logger;

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var route = context.HttpContext.Request.Path;
            _logger.LogInformation("Executing action at {Route}", route);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var route = context.HttpContext.Request.Path;
            _logger.LogInformation("Executed action at {Route}", route);
        }
    }
}
