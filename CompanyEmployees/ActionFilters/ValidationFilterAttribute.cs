using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        private readonly ILoggerManager _logger;

        public ValidationFilterAttribute(ILoggerManager logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var action = context.RouteData.Values["action"];
            var controller = context.RouteData.Values["controller"];

            var param = context.ActionArguments.SingleOrDefault(dto => dto.Value.ToString().ToLower().Contains("Dto")).Value;

            if(param == null)
            {
                _logger.LogError($"Object send form client is null.Controller: {controller}. Action: {action}");
                context.Result = new BadRequestObjectResult("$Object send form client is null.Controller: {controller}. Action: {action}");
                return;
            }

            if(!context.ModelState.IsValid)
            {
                _logger.LogError("Wrong entity.Cant bind properties");
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            throw new NotImplementedException();
        }
    }
}
