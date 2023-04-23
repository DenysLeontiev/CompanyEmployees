using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CompanyEmployees.ActionFilters
{
    public class EmployeeExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly ILoggerManager _logger;

        public EmployeeExistsAttribute(IRepositoryManager repositoryManager, ILoggerManager logger)
        {
            _repositoryManager = repositoryManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            Guid companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, false);

            if (company == null)
            {
                _logger.LogError($"Company with id - {companyId} -  is not found");
                context.Result = new NotFoundObjectResult($"Company with id - {companyId} -  is not found");
            }

            var employeeId = (Guid)context.ActionArguments["id"];
            var employee = _repositoryManager.Employee.GetEmployee(companyId, employeeId, trackChanges: false);

            if (employee == null)
            {
                _logger.LogError($"Employee with id - {employee} -  is not found");
                context.Result = new NotFoundObjectResult($"Employee with id - {employeeId} -  is not found");
            }
            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}
