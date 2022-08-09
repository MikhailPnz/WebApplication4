using Dapper;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication4.Models
{
    public interface IEmployeeRepository
    {
        void Create(Employee employee);
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

        public void Create(Employee employee)
        {
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                //Id int
                //    Name string
                //    Surname string
                //    Phone string
                //    CompanyId int
                //Passport {
                //    Type string
                //        Number string
                //}
                //Department {
                //    Name string
                //        Phone string

                //create table users(
                //    id serial primary key,
                //    name varchar(100) not null unique-- ?
                //);

                var sqlQuery = "INSERT INTO Employee (Id, Name, Surname, Phone, CompanyId) VALUES(@Id, @Name, @Surname, @Phone, @CompanyId) RETURNING id";
                int? userId = connection.Execute(sqlQuery, employee);
                employee.Id = userId.Value;
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
