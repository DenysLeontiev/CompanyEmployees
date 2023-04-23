using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.PaginationParametrs
{
    public class EmployeeParametrs : RequestParameters
    {
        public EmployeeParametrs()
        {
            OrderBy = "Name";
        }

        public uint MinAge { get; set; }
        public uint MaxAge { get; set; } = int.MaxValue;

        public string? SearchTerm { get; set; }

        public bool ValidateAgeRange => MaxAge > MinAge;
    }
}
