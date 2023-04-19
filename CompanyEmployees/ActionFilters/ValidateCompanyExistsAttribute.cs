using Contracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;

        public ValidateCompanyExistsAttribute(IRepositoryManager context, ILoggerManager logger)
        {
            _repositoryManager = context;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool trackChanges = context.HttpContext.Request.Method.Equals("PUT");
            Guid companyId = (Guid)context.ActionArguments["id"];

            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: trackChanges);

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
