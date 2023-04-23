using Contracts;
using Entities;
using Entities.PaginationParametrs;
using Entities.RequestFeature;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository // we have all method from absract class
    {
        public EmployeeRepository(RepositoryContext context) : base(context)
        {

        }

        public PagedList<Employee> GetAllEmployees(Guid companyId,EmployeeParametrs employeeParametrs ,bool trackChanges)
        {
            var employees = FindByCondition(e => e.CompanyId.Equals(companyId), trackChanges)
                            .Filter(employeeParametrs.MinAge, employeeParametrs.MaxAge)
                            .Search(employeeParametrs.SearchTerm)
                            .Sort(employeeParametrs.OrderBy)
                            .ToList();

            return PagedList<Employee>.ToPagedList(employees, employeeParametrs.PageNumber, employeeParametrs.PageSize);
        }

        public Employee GetEmployee(Guid companyId, Guid employeeId, bool trackChanges)
        {
            return FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(employeeId), trackChanges).SingleOrDefault();
        }

        public void DeleteEmployee(Employee employee)
        {
            Delete(employee);
        }

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

    }
}
