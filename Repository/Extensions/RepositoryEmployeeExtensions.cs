using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


namespace Repository.Extensions
{
    public static class RepositoryEmployeeExtensions
    {
        public static IQueryable<Employee> Filter(this IQueryable<Employee> employees, uint minAge, uint maxAge)
        {
            return employees.Where(e => e.Age >= minAge && e.Age <= maxAge);
        }

        public static IQueryable<Employee> Search(this IQueryable<Employee> employees, string searchTerm)
        {
            if(string.IsNullOrEmpty(searchTerm))
            {
                return employees;
            }

            var searchTermToLower = searchTerm.ToLower();

            return employees.Where(e => e.Name.ToLower().Contains(searchTermToLower.Trim()));
        }

        public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        {
            if(string.IsNullOrWhiteSpace(orderByQueryString))
            {
                return employees.OrderBy(e => e.Name);
            }

            var orderParams = orderByQueryString.Trim().Split(',');
            var employeeObjectPropInfo = typeof(Employee).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var queryBuilder = new StringBuilder();

            foreach (var param in orderParams)
            {
                if(string.IsNullOrWhiteSpace(param))
                {
                    continue;
                }

                var paramName = param.Split(" ")[0];
                var queryParamName = employeeObjectPropInfo.FirstOrDefault(e => e.Name.ToLower() == paramName.ToLower());

                if(queryParamName == null)
                {
                    continue;
                }

                var direction = param.EndsWith(" desc") ? "descending" : "ascending";
                queryBuilder.Append($"{queryParamName.Name.ToString()} {direction},");
            }

            var orderQuery = queryBuilder.ToString().Trim(',', ' ');
            if(string.IsNullOrWhiteSpace(orderQuery))
            {
                return employees.OrderBy(e => e.Name);
            }

            return employees.OrderBy(e => orderQuery);
        }





        //public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
        //{
        //    if(string.IsNullOrWhiteSpace(orderByQueryString))
        //    {
        //        return employees.OrderBy(e => e.Name);
        //    }

        //    var orderParams = orderByQueryString.Trim().Split(',');
        //    var propertyInfos = typeof(Employee).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        //    var queryBuilder = new StringBuilder();

        //    foreach (var param in orderParams)
        //    {
        //        if(string.IsNullOrWhiteSpace(param))
        //        {
        //            continue;
        //        }
        //        var properyFromQueryName = param.Split(" ")[0];

        //        var objectPropertyName = propertyInfos.FirstOrDefault(x => x.Name.ToLower() == properyFromQueryName.ToLower());

        //        if(objectPropertyName == null)
        //        {
        //            continue;
        //        }

        //        var direction = param.EndsWith(" desc") ? "descending" : "ascending";

        //        queryBuilder.Append($"{objectPropertyName.Name.ToString()} {direction},");
        //    }

        //    string orderQuery = queryBuilder.ToString().TrimEnd(',', ' ');

        //    if (string.IsNullOrWhiteSpace(orderQuery))
        //    {
        //        return employees.OrderBy(x => x.Name);
        //    }

        //    return employees.OrderBy(x => orderQuery);
        //}
    }
}
