using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4.Models
{
    public class Passport
    {
        public string Type { get; set; } = "";
        public string Number { get; set; } = "";

        public Passport() {}

        public Passport(string type, string number)
        {
            Type = type;
            Number = number;
        }
    }
    public class Department
    {
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";

        public Department() { }

        public Department(string name, string phone)
        {
            Name = name;
            Phone = phone;
        }
    }

    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Phone { get; set; } = "";
        public int CompanyId { get; set; }
        public Passport Passport { get; set; }
        public Department Department { get; set; }
    }
}
