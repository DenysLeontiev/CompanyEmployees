﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class Company
    {
        [Column("CompanyId")]
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Name is a required field.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Address is a required field.")]
        public string? Address { get; set; }

        public string? Country { get; set; }
        public ICollection<Employee>? Employees { get; set; }
    }
}
