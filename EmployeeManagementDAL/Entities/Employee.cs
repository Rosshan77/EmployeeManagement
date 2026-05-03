using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmployeeManagementDAL.Models
{
    public class Employee
    {
        public int Id { get; set; }   // 👈 Primary Key (by convention)

        public string? Name { get; set; }

        public string? Email { get; set; }

        public string? Department { get; set; }

        public decimal Salary { get; set; }
    }
}
