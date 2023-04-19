using Contracts;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly RepositoryContext _repositoryContext;
        private ICompanyRepository? companyRepository;
        private IEmployeeRepository? employeeRepository;

        public RepositoryManager(RepositoryContext repositoryContext)
        {
            _repositoryContext = repositoryContext;
        }

        public ICompanyRepository Company
        {
            get
            {
                if(companyRepository == null)
                {
                    companyRepository = new CompanyRepository(_repositoryContext);
                }

                return companyRepository;
            }
        }

        public IEmployeeRepository Employee
        {
            get
            {
                if(employeeRepository == null)
                {
                    employeeRepository = new EmployeeRepository(_repositoryContext);
                }

                return employeeRepository;
            }
        }


        public async Task SaveAsync()
        {
            await _repositoryContext.SaveChangesAsync(); // saves all changes for all entities
        }
    }
}
