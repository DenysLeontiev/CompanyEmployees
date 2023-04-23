using Contracts;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class DataShaper<T> : IDataShaper<T> where T : class
    {
        public PropertyInfo[] Properties { get; set; }

        public DataShaper()
        {
            Properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance); // here we get all the properties of T entity
        }

        public ExpandoObject ShapeData(T entity, string fieldsString)
        {
            var requierdProperties = GetRequiredProperties(fieldsString); // here we get all required properties using fieldsString
            return FetchDataForEntity(entity, requierdProperties); // here we get back a new ExpandoObject but with fields,we have pointed out
        }

        public IEnumerable<ExpandoObject> ShapeData(IEnumerable<T> entities, string fieldsString) // that's the same as ShapeData,but for many ExpandoObjects
        {
            var requiredProperties = GetRequiredProperties(fieldsString);
            return FetchData(entities, requiredProperties);
        }

        private IEnumerable<PropertyInfo> GetRequiredProperties(string fieldsString)
        {
            var requiredProperties = new List<PropertyInfo>(); // what we return(list of properties we want)

            if(!string.IsNullOrWhiteSpace(fieldsString)) // check, whether fieldsString(where we get out properties) is empty
            {
                var fields = fieldsString.Trim().Split(','); // split it into comma(example: "age,name,year" => ["age", "name", "year"])
                foreach (var field in fields) // iterata through that array
                {
                    if (string.IsNullOrWhiteSpace(field)) // check
                    {
                        continue;
                    }

                    var objectProperty = Properties.FirstOrDefault(p => p.Name.Trim().ToLower() == field.ToLower()); // get property by that name
                    if(objectProperty == null) // check whether such property exists
                    {
                        continue;
                    }

                    requiredProperties.Add(objectProperty); // add that property if that actually exists
                }
            }
            else
            {
                requiredProperties = Properties.ToList(); // if fieldsString is empty, we assume,that all properties are required
            }

            return requiredProperties; // here we return all the required properties
        }

        private IEnumerable<ExpandoObject> FetchData(IEnumerable<T> entities, IEnumerable<PropertyInfo> requiredProperties) // same as FetchData(), 
        {                                                                                                                   // but for multiple objects
            List<ExpandoObject> shappedData = new List<ExpandoObject>();

            foreach (var entity in entities)
            {
                var shapedObject = FetchDataForEntity(entity, requiredProperties);
                shappedData.Add(shapedObject);
            }

            return shappedData;
        }

        private ExpandoObject FetchDataForEntity(T entity, IEnumerable<PropertyInfo> requiredProperties) // here we fetch data for one specific entity
        {
            var shappedObject = new ExpandoObject(); // new object with fields we want to return
            foreach (var requiredProp in requiredProperties) // iterate through all the properties we want 
            {
                var objectPropValue = requiredProp.GetValue(entity); // get the value of a particular property
                shappedObject.TryAdd(requiredProp.Name, objectPropValue); // add new object with Property Name as a Key and Property Value as a Value to shappedObject
            }

            return shappedObject; // return new object with properties we have speciefied
        }
    }
}
