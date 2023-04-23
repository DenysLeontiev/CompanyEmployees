using AutoMapper;
using Azure;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.PaginationParametrs;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CompanyEmployees.Controllers
{
    [ApiController]
    [Route("api/companies/{companyId}/employees")]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repositoryManager;
        private readonly IMapper _mapper;
        private readonly ILoggerManager _logger;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeesController(IRepositoryManager repositoryManager, IMapper mapper, ILoggerManager logger, IDataShaper<EmployeeDto> dataShaper)
        {
            _repositoryManager = repositoryManager;
            _mapper = mapper;
            _logger = logger;
            _dataShaper = dataShaper;
        }

        [HttpGet]
        [HttpHead]
        public async Task<ActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParametrs employeeParametrs)
        {
            if(employeeParametrs.ValidateAgeRange == false)
            {
                return BadRequest("MaxAge can't be less that MinAge");
            }

            var employeeCompany = await _repositoryManager.Company.GetCompanyAsync(companyId, false);
            if(employeeCompany == null)
            {
                return NotFound();
            }

            var employees = _repositoryManager.Employee.GetAllEmployees(employeeCompany.Id, employeeParametrs,false);

            if (employees == null)
            {
                return NotFound();
            }

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(employees.MetaData));

            var employeesToReturn = _mapper.Map<IEnumerable<EmployeeDto>>(employees);

            return Ok(_dataShaper.ShapeData(employeesToReturn, employeeParametrs.Fields));

            return Ok(employeesToReturn);
        }

        [ServiceFilter(typeof(EmployeeExistsAttribute))]
        [HttpGet("{id}", Name = "GetEmployeeById")]
        public IActionResult GetEmployee(Guid companyId, Guid id)
        {
            var employee = HttpContext.Items["employee"];

            var employeeToReturn = _mapper.Map<EmployeeDto>(employee);
            return Ok(employeeToReturn);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(Guid companyId, Guid id)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);

            if(company == null)
            {
                _logger.LogError($"Company with id - {companyId} - is not found");
                return NotFound();
            }

            var employee = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: false);

            if(employee == null)
            {
                _logger.LogError($"Company with id - {companyId} - is not found");
                return NotFound();
            }

            _repositoryManager.Employee.DeleteEmployee(employee);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(Guid companyId, Guid id, [FromBody] EmployeeForUpdateDto employeeUpdateDto)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false); // false,because we won`t apply any changes here, so to make query faster - we use AsNoTracking();
            if(company == null)
            {
                _logger.LogError($"Company with id - {companyId} - is not found");
                return NotFound();
            }

            if(!ModelState.IsValid)
            {
                _logger.LogError("Wrong enity type.Cant bind propertis");
                return UnprocessableEntity(ModelState);
            }

            var employee = _repositoryManager.Employee.GetEmployee(companyId, id, trackChanges: true); // here we track changes, because we then modify that property.Otherwise, changes won`t be applied

            if(employee == null)
            {
                _logger.LogError($"Employee with id - {id} is not found");
                return NotFound();
            }

            _mapper.Map(employeeUpdateDto, employee);
            await _repositoryManager.SaveAsync();
            return NoContent();
        }

        //[ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPost]
        public async Task<IActionResult> CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employeeForCreationDto)
        {
            var company = await _repositoryManager.Company.GetCompanyAsync(companyId, trackChanges: false);

            //if(!ModelState.IsValid)
            //{
            //    _logger.LogError("Wrong entity value.Cant bind entity");
            //    return UnprocessableEntity("Cant bind entity");
            //}

            if(company == null)
            {
                _logger.LogError($"Company with id - {companyId} - is not found");
                return NotFound();
            }

            var employee = _mapper.Map<Employee>(employeeForCreationDto);
            var employeeDto = _mapper.Map<EmployeeDto>(employee);
            _repositoryManager.Employee.CreateEmployeeForCompany(companyId, employee);
            await _repositoryManager.SaveAsync();

            employeeDto.Id = employee.Id;
            return CreatedAtRoute("GetEmployeeById", new { companyId, id = employeeDto.Id }, employeeDto);
        }

        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateEmployeePartially(Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDocument)
        {
            if(patchDocument == null)
            {
                _logger.LogError("PatchDocumant is null");
                return BadRequest();
            }

            var employee = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employee);
            patchDocument.ApplyTo(employeeToPatch); // we apply pathcDocument(which is type of EmployeeForUpdateDto) to employeeToPatch

            patchDocument.ApplyTo(employeeToPatch, ModelState); // for validation purposes // (objectApplyTo, modelState) // here we validate path doc

            //In summary, the ApplyTo method applies the changes specified in the patchDocument to the employeeToPatch object and captures any errors that occur during the process in the ModelState object.

            // If there is an error(for example, age is less than 18), so TryValidateModel(employeeToPatch) is invalid;
            TryValidateModel(employeeToPatch); // if there is an error, so ModelState is invalid

            if(!ModelState.IsValid)
            {
                _logger.LogError("\"Invalid model state for the patch document");
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employee);
            await _repositoryManager.SaveAsync();

            return NoContent();
        }
    }
}
