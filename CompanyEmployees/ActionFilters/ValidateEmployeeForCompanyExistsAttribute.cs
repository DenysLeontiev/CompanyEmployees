using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Repository;

namespace CompanyEmployees.ActionFilters
{
    public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;

        public ValidateEmployeeForCompanyExistsAttribute(IRepositoryManager context, ILoggerManager logger)
        {
            _repositoryManager = context;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            bool trackChanges = context.HttpContext.Request.Method.Equals("PUT")
                                || context.HttpContext.Request.Method.Equals("PATCH") ? true : false;

            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);

            if(company == null)
            {
                _logger.LogError($"Company with id - {companyId} is not found");
                context.Result = new NotFoundObjectResult($"Company with id - {companyId} is not found");
            }

            var employeeId = (Guid)context.ActionArguments["id"];

            var employee = _repositoryManager.Employee.GetEmployee(companyId, employeeId, trackChanges: trackChanges);

            if(employee == null)
            {
                _logger.LogError($"Employee with id - {employeeId} is not found");
                context.Result = new NotFoundObjectResult($"Employee with id - {employeeId} is not found");
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}
