using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Repository;

namespace CompanyEmployees.ActionFilters
{
    public class CompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;

        public CompanyExistsAttribute(IRepositoryManager context, ILoggerManager logger)
        {
            _repositoryManager = context;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var companyId = (Guid)context.ActionArguments["id"];
            var company = _repositoryManager.Company.GetCompanyAsync(companyId, false);

            if(company == null)
            {
                _logger.LogError($"Company with id - {companyId} is not found");
                context.Result = new NotFoundObjectResult($"Company with id - {companyId} is not found");
            }
            else
            {
                context.HttpContext.Items.Add("company", company);
                await next();
            }
        }
    }
}
