using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        private readonly RepositoryContext _repositoryContext;

        public RepositoryBase(RepositoryContext context)
        {
            _repositoryContext = context; 
        }

        public void Create(T entity)
        {
            _repositoryContext.Set<T>().Add(entity); // Set<T>() creates DbSet<T> that can be used to query and create new instances
        }
        public void Update(T entity)
        {
            _repositoryContext.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _repositoryContext.Set<T>().Remove(entity);
        }

        public IQueryable<T> FindAll(bool trackChanges)
        {
            if(trackChanges == false)
            {
                return _repositoryContext.Set<T>().AsNoTracking();
            }
            else
            {
                return _repositoryContext.Set<T>();
            }
        }

        public IQueryable<T> FindByCondition(System.Linq.Expressions.Expression<Func<T, bool>> expression, bool trackChanges)
        {
            if(trackChanges == false)
            {
                return _repositoryContext.Set<T>().AsNoTracking().Where(expression);
            }
            else
            {
                return _repositoryContext.Set<T>().Where(expression);
            }
        }
    }
}
