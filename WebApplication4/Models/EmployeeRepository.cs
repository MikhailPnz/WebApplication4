using Dapper;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NpgsqlTypes;

namespace WebApplication4.Models
{
    public interface IEmployeeRepository
    {
        int? Create(Employee employee);
        void Delete(int id);
        Employee Get(int id);
        List<Employee> GetEmployesForDepName(string name);
        void Update(Employee employee);
    }

    public class EmployeeBD
    {
        public int Id { get; set; }
        public string EmployeeName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Phone { get; set; } = "";
        public int CompanyId { get; set; }
        public int PassportId { get; set; }
        public string DepartmentName { get; set; } = "";
        public string DepartmentPhone { get; set; } = "";
        public string Type { get; set; } = "";
        public string Number { get; set; } = "";
    }

    public class EmployeeRepository : IEmployeeRepository
    {
        string connectionString = null;
        public EmployeeRepository(string conn)
        {
            connectionString = conn;
        }
        public static bool Equals(object objA, object objB)
        {
            return objA == objB || (objA != null && objB != null && objA.Equals(objB));
        }

        
        public int? Create(Employee employee)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // добавление 1 сотрудника

                // у каждого сотрудника свой номер паспорта и он уникален
                var typePassport = connection.Query<Passport>("SELECT * FROM Passport WHERE Number = @Number", new { employee.Passport.Number }).FirstOrDefault();
                // если такого номера паспорта нет, то и сотрудника нет
                if (typePassport == null)
                {
                    // получить список компаний, если такой компании нет создать
                    var nameDep = connection.Query<Department>("SELECT * FROM Department WHERE DepartmentName = @DepartmentName",
                        new { employee.Department.DepartmentName }).FirstOrDefault();
                    // если такой компании нет, добавляем ее
                    if (nameDep == null)
                    {
                        var insertDepartment = "INSERT INTO Department (DepartmentName, DepartmentPhone) VALUES(@DepartmentName, @DepartmentPhone)";
                        connection.ExecuteScalar(insertDepartment, new { employee.Department.DepartmentName, employee.Department.DepartmentPhone });
                    }

                    var insertPassport = "INSERT INTO Passport (Type, Number) VALUES(@Type, @Number)";
                    connection.ExecuteScalar(insertPassport, new { employee.Passport.Type, employee.Passport.Number });

                    StringBuilder createSQL = new StringBuilder();
                    // добавить сотрудника + соединить id департамента и id паспорта
                    createSQL.Append("INSERT INTO Employee (EmployeeName, Surname, Phone, CompanyId, PassportId) VALUES ('");
                    createSQL.Append(employee.EmployeeName);
                    createSQL.Append("','");
                    createSQL.Append(employee.Surname);
                    createSQL.Append("','");
                    createSQL.Append(employee.Phone);
                    createSQL.Append("', (SELECT Id FROM Department WHERE DepartmentName='");
                    createSQL.Append(employee.Department.DepartmentName);
                    createSQL.Append("'), (SELECT Id FROM Passport WHERE Number='");
                    createSQL.Append(employee.Passport.Number);
                    createSQL.Append("')) RETURNING id;");

                    var id = connection.ExecuteScalar(createSQL.ToString());

                    return (int)id;
                }
                else
                {
                    return null;
                }

                // проверить, есть ли такой сотрудник
                //var employeeSQL = "SELECT * FROM Department, Passport, Employee WHERE Employee.EmployeeName = @EmployeeName AND Employee.Surname = @Surname AND Passport.Number = @Number";
                //var employeeGet = connection.Query<EmployeeBD>(employeeSQL, new { employee.EmployeeName, employee.Surname, employee.Passport.Number}).FirstOrDefault();

                //bool addEmployee = false;

                // если совпадений нет добавить сотрудника
                //if (employeeGet != null)
                //{
                //    // проверить на соответствие
                //    var newEmployee = new Employee()
                //    {
                //        Id = employeeGet.Id,
                //        EmployeeName = employeeGet.EmployeeName,
                //        Surname = employeeGet.Surname,
                //        Phone = employeeGet.Phone,
                //        CompanyId = employeeGet.CompanyId,
                //        PassportId = employeeGet.PassportId,
                //        Passport = new Passport(employeeGet.Type, employeeGet.Number),
                //        Department = new Department(employeeGet.DepartmentName, employeeGet.DepartmentPhone)
                //    };

                //    // сравнить поля без Id
                //    if (!Equals(employee, newEmployee))
                //    {
                //        addEmployee = true;
                //    }
                //}
                //else
                //{
                //    addEmployee = true;
                //}
            }
        }

        Employee EmployeeBDtoEmployee(EmployeeBD employeeBd)
        {
            var newEmployee = new Employee()
            {
                Id = employeeBd.Id,
                EmployeeName = employeeBd.EmployeeName,
                Surname = employeeBd.Surname,
                Phone = employeeBd.Phone,
                CompanyId = employeeBd.CompanyId,
                PassportId = employeeBd.PassportId,
                Passport = new Passport(employeeBd.Type, employeeBd.Number),
                Department = new Department(employeeBd.DepartmentName, employeeBd.DepartmentPhone)
            };

            return newEmployee;
        }

        public void Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                // удалили паспорт сотрудника и сотрудника
                var sqlQuery = "DELETE FROM Passport WHERE Passport.Id = (SELECT PassportId FROM Employee WHERE Id = @id); DELETE FROM Employee WHERE Id = @id";
                connection.Execute(sqlQuery, new { id });
            }
        }

        public Employee Get(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var getEmployee = connection.Query<EmployeeBD>("SELECT * FROM Department, Passport, Employee WHERE Employee.Id = @id", new { id }).FirstOrDefault();
                if (getEmployee != null)
                {
                    return EmployeeBDtoEmployee(getEmployee);
                }
                else
                {
                    return null;
                }
            }
        }

        public List<Employee> GetEmployesForDepName(string departmentName)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var getEmployee = connection.Query<EmployeeBD>("SELECT * FROM Department, Passport, Employee " +
                                                               "WHERE Department.DepartmentName = @DepartmentName", new { departmentName });
                if (getEmployee != null)
                {
                    var listEmployes = new List<Employee>();
                    // проверить на соответствие
                    foreach (var empl in getEmployee)
                    {
                        listEmployes.Add(EmployeeBDtoEmployee(empl));
                    }
                    return listEmployes;
                }
                else
                {
                    return null;
                }
            }
        }

        public void Update(Employee employee)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                var departSQL= "UPDATE Department SET DepartmentName = @DepartmentName, DepartmentPhone = @DepartmentPhone " +
                               "WHERE Department.Id = (SELECT CompanyId FROM Employee WHERE Id = @id);";
                connection.Execute(departSQL, new { employee.Department.DepartmentName, employee.Department.DepartmentPhone, employee.Id });

                //var sqlQuery = "DELETE FROM Passport WHERE Passport.Id = (SELECT PassportId FROM Employee WHERE Id = @id); DELETE FROM Employee WHERE Id = @id";
                //connection.Execute(sqlQuery, new { id });

                //var sqlQuery = "UPDATE Employee SET Name = @Name, Age = @Age WHERE Id = @Id";

                //StringBuilder updateSQL = new StringBuilder();
                //// изменить сотрудника + соединить id департамента и id паспорта
                //updateSQL.Append("INSERT INTO Employee (EmployeeName, Surname, Phone, CompanyId, PassportId) VALUES ('");
                //updateSQL.Append(employee.EmployeeName);
                //updateSQL.Append("','");
                //updateSQL.Append(employee.Surname);
                //updateSQL.Append("','");
                //updateSQL.Append(employee.Phone);
                //updateSQL.Append("', (SELECT Id FROM Department WHERE DepartmentName='");
                //updateSQL.Append(employee.Department.DepartmentName);
                //updateSQL.Append("'), (SELECT Id FROM Passport WHERE Number='");
                //updateSQL.Append(employee.Passport.Number);
                //updateSQL.Append("')) RETURNING id;");

                //connection.Execute(updateSQL.ToString(), employee);
            }
        }
    }
}
