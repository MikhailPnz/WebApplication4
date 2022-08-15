using Dapper;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NpgsqlTypes;

namespace WebApplication4.Models
{
    public interface IEmployeeRepository
    {
        Employee Create(Employee employee);
        void Delete(int id);
        Employee Get(int id);
        List<Employee> GetEmployees();
        void Update(Employee employee);
    }

    public class EmployeeRepository : IEmployeeRepository
    {
        string connectionString = null;
        public EmployeeRepository(string conn)
        {
            connectionString = conn;
        }

        public Employee Create(Employee employee)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                // добавление 1 сотрудника

                // проверить, есть ли такой сотрудник
                //var employeeSQL = "SELECT * FROM Employee WHERE Employee.EmployeeName = @EmployeeName AND " +
                //                  "Employee.Surname = @Surname AND Employee.Phone = @Phone AND Employee.CompanyId = Department.Id AND " +
                //                  "Employee.PassportId=Passport.Id";
                //var namEmployee = connection.Query<Employee>(employeeSQL, new { employee.EmployeeName, employee.Surname }).FirstOrDefault();

                // получить список компаний, если такой компании нет создать
                var nameDep = connection.Query<Department>("SELECT * FROM Department WHERE DepartmentName = @DepartmentName", 
                    new { employee.Department.DepartmentName }).FirstOrDefault();
                // если такой компании нет, добавляем ее
                if (nameDep == null)
                {
                    var insertDepartment = "INSERT INTO Department (DepartmentName, DepartmentPhone) VALUES(@DepartmentName, @DepartmentPhone)";
                    connection.ExecuteScalar(insertDepartment, new { employee.Department.DepartmentName, employee.Department.DepartmentPhone });
                }

                // получить список типа паспортов, если такого нет добавить
                var typePassport = connection.Query<Passport>("SELECT * FROM Passport WHERE Type = @Type", new { employee.Passport.Type }).FirstOrDefault();
                // если такого типа паспорта нет, добавить
                if (typePassport == null)
                {
                    var insertDepartment = "INSERT INTO Passport (Type, Number) VALUES(@Type, @Number)";
                    connection.ExecuteScalar(insertDepartment, new { employee.Passport.Type, employee.Passport.Number });
                }

                // проверить, есть ли такой сотрудник
                //var employeeSQL = "SELECT * FROM Department, Passport, Employee WHERE Employee.EmployeeName = @EmployeeName AND " +
                //                                           "Employee.Surname = @Surname AND Employee.CompanyId = Department.Id AND " +
                //                                           "Employee.PassportId=Passport.Id";
                //var namEmployee = connection.Query<object>(employeeSQL, new { employee.EmployeeName, employee.Surname }).FirstOrDefault();
                

                // добавить сотрудника + соединить id департамента и id паспорта
                var sqlQgfhuery = "INSERT INTO Employee (EmployeeName, Surname, Phone, CompanyId, PassportId) VALUES ('Дима','Петров', '786786', " +
                                  "(SELECT Id FROM Department WHERE DepartmentName='Ozon'), (SELECT Id FROM Passport WHERE Type='Ordinary Passport')) RETURNING id;";
                var namffgDep = connection.ExecuteScalar(sqlQgfhuery);


                var employeeSQL = "SELECT * FROM Employee WHERE Employee.EmployeeName = @EmployeeName AND " +
                                  "Employee.Surname = @Surname AND Employee.Phone = @Phone AND Employee.CompanyId = Department.Id AND " +
                                  "Employee.PassportId=Passport.Id";
                var namEmployee = connection.Query<Employee>(employeeSQL, new { employee.EmployeeName, employee.Surname }).FirstOrDefault();


                //foreach (var o in namffgDep)
                //{


                //}

                //var namEsdfsmployee = connection.Query<object>(employeeSQL, new { employee.EmployeeName, employee.Surname }).FirstOrDefault();
                //var dhdh = (Department)namEmployee;
                //Department? Depdsfsdfartment = namEsdfsmployee as Department;
                //ICollection<KeyValuePair<string, object>> gfhfgh = (ICollection<KeyValuePair<string, object>>)(ICollection)namEsdfsmployee;
                //Employee empl = new Employee();
                //foreach (var dfgd in gfhfgh)
                //{
                //    switch (dfgd.Key)
                //    {
                //        case "EmployeeName":
                //            empl.EmployeeName = (string)dfgd.Value;
                //            break;
                //        case "Surname":
                //            empl.Surname = (string)dfgd.Value;
                //            break;
                //        case "Phone":
                //            empl.Phone = (string)dfgd.Value;
                //            break;
                //        case "CompanyId":
                //            empl.CompanyId = (int)dfgd.Value;
                //            break;
                //        case "DepartmentName":
                //            empl.Department.DepartmentName = (string)dfgd.Value;
                //            break;
                //        case "DepartmentPhone":
                //            empl.Department.DepartmentPhone = (string)dfgd.Value;
                //            break;
                //        case "Type":
                //            empl.Passport.Type = (string)dfgd.Value;
                //            break;
                //        case "Number":
                //            empl.Passport.Number = (string)dfgd.Value;
                //            break;
                //    }
                //}




                //Employee? employe_e = namEsdfsmployee as Employee;
                //var namEmвыаывployee = connection.Query<Employee>(employeeSQL, new { employee.Name, employee.Surname }).FirstOrDefault();







                //var sqlQuery = "INSERT INTO Employee (Name, Surname, Phone, CompanyId, PasportType, PasportNumber, DepName, DepPhone) " +
                //               "VALUES(@Name, @Surname, @Phone, @CompanyId, @PasportType, @PasportNumber, @DepName, @DepPhone) RETURNING id";
                //var userId = connection.ExecuteScalar(sqlQuery, employee);
                employee.Id = (int)10;
                return employee;
            }
        }

        public void Delete(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var sqlQuery = "DELETE FROM Employee WHERE Id = @id";
                connection.Execute(sqlQuery, new { id });
            }
        }

        public Employee Get(int id)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                return connection.Query<Employee>("SELECT * FROM Employee WHERE Id = @id", new { id }).FirstOrDefault();
            }
        }

        public List<Employee> GetEmployees()
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                return connection.Query<Employee>("SELECT * FROM Employee").ToList();
            }
        }

        public void Update(Employee employee)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                var sqlQuery = "UPDATE Employee SET Name = @Name, Age = @Age WHERE Id = @Id";
                connection.Execute(sqlQuery, employee);
            }
        }
    }
}
