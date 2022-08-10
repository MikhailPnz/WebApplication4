using Dapper;
using Npgsql;
using System;
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
                var sqlQuery = "INSERT INTO Employee (Name, Surname, Phone, CompanyId, PasportType, PasportNumber, DepName, DepPhone) " +
                               "VALUES(@Name, @Surname, @Phone, @CompanyId, @PasportType, @PasportNumber, @DepName, @DepPhone) RETURNING id";
                var userId = connection.ExecuteScalar(sqlQuery, employee);
                employee.Id = (int)userId;
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
