using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace CompanyEmployees.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/companies")]
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("allow", "GET, OPTIONS, POST");

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCompanies()
        {

            var companies = await _repository.Company.GetAllCompaniesAsync(trackChanges: false);

            var companiesToRetuen = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesToRetuen);
        }

        [ServiceFilter(typeof(CompanyExistsAttribute))]
        [HttpGet("{id}", Name = "GetCompanyById")]
        public async Task<IActionResult> GetCompany(Guid id) 
        {
            var company = HttpContext.Items["company"] as Company;

            var companyToReturn = _mapper.Map<CompanyDto>(company);

            return Ok(companyToReturn);
        }

        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPost]
        public async Task<IActionResult> CreateCompany([FromBody] CompanyForCreationDto companyForCreation)
        {
            if(companyForCreation == null)
            {
                _logger.LogError("CompanyForCreationDto,which is sent from client, is null");
            }

            if(!ModelState.IsValid)
            {
                _logger.LogError("Wrong data in entity");
                return UnprocessableEntity(ModelState);
            }

            var company = _mapper.Map<Company>(companyForCreation);

            _repository.Company.CreateCompany(company);
            await _repository.SaveAsync();

            var companyDto = _mapper.Map<CompanyDto>(company);
            return CreatedAtRoute("GetCompanyById", new { id = companyDto.Id }, companyDto);
        }

        [HttpGet("collection/({ids})",Name = "CompanyCollection")] // ({ids}) - for an array of IDs
        public async Task<IActionResult> GetCompanyCollection(IEnumerable<Guid> ids)
        {
            if(ids == null)
            {
                _logger.LogError("ids are null");
                return BadRequest("ids are null");
            }

            var companyEntities = await _repository.Company.GetByIdsAsync(ids, false);

            if(ids.Count() != companyEntities.Count())
            {
                _logger.LogError("Some ids are not valid in a collection");
                return NotFound();
            }

            foreach (Company company in companyEntities)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();

            var companyDtos = _mapper.Map<IEnumerable<CompanyDto>>(companyEntities);

            return Ok(companyDtos);
        }

        [HttpPost("collection")]
        public async Task<IActionResult> CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyDtos) 
        {
            if(companyDtos == null)
            {
                _logger.LogError("Company collection sent from client is null.");
                return BadRequest("Company collection is null.");
            }

            var companies = _mapper.Map<IEnumerable<Company>>(companyDtos);

            foreach (Company company in companies)
            {
                _repository.Company.CreateCompany(company);
            }
            await _repository.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            var ids = string.Join(" , ", companies.Select(company => company.Id));

            return CreatedAtRoute("CompanyCollection", new { ids }, companyCollectionToReturn);
        }

        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            var company = HttpContext.Items["company"] as Company;

            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

            return NoContent();
        }

        [ServiceFilter(typeof(ValidateCompanyExistsAttribute))]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromRoute] CompanyForUpdateDto companyUpdateDto)
        {

            var company = HttpContext.Items["company"]as Company;

            _mapper.Map(companyUpdateDto, company);
            await _repository.SaveAsync();

            return NoContent();
        }

    }
}
