using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using WebApplication4.Models;

namespace WebApplication4
{
    public class Startup
    {
        IEmployeeRepository _repo;
        
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Host=pg_container;Username=root;Password=root;Database=test_db";
            Init(connectionString);
        }

        void Init(string connectionString)
        {
            _repo = new EmployeeRepository(connectionString);
            using (var connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();
                try
                {
                    connection.Execute("CREATE TABLE Department(Id SERIAL PRIMARY KEY,DepartmentName VARCHAR(30) NOT NULL," +
                                       "DepartmentPhone VARCHAR(30) NOT NULL);");

                    connection.Execute("CREATE TABLE Passport(Id SERIAL PRIMARY KEY, Type VARCHAR(30) NOT NULL, Number CHARACTER VARYING(100) NOT NULL);");

                    connection.Execute("CREATE TABLE Employee(Id SERIAL PRIMARY KEY, EmployeeName CHARACTER VARYING(30) NOT NULL," +
                                       "Surname CHARACTER VARYING(30) NOT NULL, Phone  CHARACTER VARYING(30) NOT NULL," +
                                       "CompanyId INTEGER NOT NULL REFERENCES Department(Id) ON DELETE CASCADE," +
                                       "PassportId INTEGER NOT NULL REFERENCES Passport(Id) ON DELETE CASCADE);");
                }
                catch
                {
                    // исключение
                    //throw new Exception();
                    Console.WriteLine("Таблица Emploee уже существует!");
                }
            }

            // test
            foreach (var employe in _employes)
            {
                _repo.Create(employe);
            }
        }

        // начальные данные
        List<Employee> _employes = new List<Employee>
        {
            new()
            {
                Id = 25,
                EmployeeName = "Дима",
                Surname = "Петров",
                Phone = "354353",
                CompanyId = 345353,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Ozon", "67868684")
            },

            new()
            {
                Id = 32,
                EmployeeName = "Алла",
                Surname = "Сергеева",
                Phone = "8687687",
                CompanyId = 343242,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Oriflame", "456345645")
            },

            new()
            {
                Id = 41,
                EmployeeName = "Иван",
                Surname = "Иванов",
                Phone = "7865789",
                CompanyId = 34234324,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Ozon", "654645654")
            }
        };

        // получение списка работников по названию компании
        async Task GetListEmployesForDepName(string? depName, HttpResponse response)
        {
            var employesForDepName = _repo.GetEmployesForDepName(depName);
            if (employesForDepName != null)
            {
                await response.WriteAsJsonAsync(employesForDepName);
            }
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Совпадений не найдено" });
            }
            
        }

        async Task DeletePerson(int? id, HttpResponse response)
        {
            if (id != null)
            {
                var employeeId = (int)id;

                Employee? employe = _repo.Get(employeeId);

                if (employe != null)
                {
                    _repo.Delete(employeeId);
                    await response.WriteAsJsonAsync(employe);
                }
                else
                {
                    response.StatusCode = 404;
                    await response.WriteAsJsonAsync(new { message = "Сотрудник не найден" });
                }
            }
            else
            {
                await response.WriteAsJsonAsync(new { message = "Неверный запрос, укажите id сотрудника" });
            }
        }

        async Task CreateEmployee(HttpResponse response, HttpRequest request)
        {
            try
            {
                var employe = await request.ReadFromJsonAsync<Employee>();
                if (employe != null)
                {
                    var id = _repo.Create(employe);
                    if (id != null)
                    {
                        // сотрудник добавлен
                        await response.WriteAsJsonAsync(id);
                    }
                    else
                    {
                        throw new Exception("Сотрудник уже существует");
                    }
                }
                else
                {
                    throw new Exception("Некорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
            }
        }

        async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                Employee? userData = await request.ReadFromJsonAsync<Employee>();
                if (userData != null)
                {
                    // получить сотрудника
                    var employe = _repo.Get(userData.Id);
                    // если пользователь найден, изменяем его данные и отправляем обратно клиенту
                    if (employe != null)
                    {
                        // изменить поля
                        if (!string.IsNullOrEmpty(userData.EmployeeName))
                        {
                            employe.EmployeeName = userData.EmployeeName;
                        }

                        if (!string.IsNullOrEmpty(userData.Surname))
                        {
                            employe.Surname = userData.Surname;
                        }

                        if (!string.IsNullOrEmpty(userData.Phone))
                        {
                            employe.Phone = userData.Phone;
                        }

                        if (!string.IsNullOrEmpty(userData.Department.DepartmentName))
                        {
                            employe.Department.DepartmentName = userData.Department.DepartmentName;
                        }

                        if (!string.IsNullOrEmpty(userData.Department.DepartmentPhone))
                        {
                            employe.Department.DepartmentPhone = userData.Department.DepartmentPhone;
                        }

                        if (!string.IsNullOrEmpty(userData.Passport.Type)) // ссылка на нулевой объект
                        {
                            employe.Passport.Type = userData.Passport.Type;
                        }

                        if (!string.IsNullOrEmpty(userData.Passport.Number))
                        {
                            employe.Passport.Number = userData.Passport.Number;
                        }

                        // записать в бд
                        _repo.Update(employe);

                        await response.WriteAsJsonAsync(employe.Id);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        await response.WriteAsJsonAsync(new { message = "пользователь не найден" });
                    }
                }
                else
                {
                    throw new Exception("Некорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            //app.UseRouting();

            app.Run(async (context) =>
            {
                var response = context.Response;
                var request = context.Request;
                var path = request.Path;
                /*
                string expressionForString = "^/api/employee/(.+?)"; // отделение
                //string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
                if (request.Method == "GET")
                {
                    // вывод сотрудников по наименованию компании
                    if (Regex.IsMatch(path, expressionForString))
                    {
                        string? departmentName = path.Value?.Split("/")[3];
                        await GetListEmployesForDepName(departmentName, response);
                    }
                }
                // Добавление сотрудника, в ответ приходит его Id
                if (path == "/api/employee" && request.Method == "POST")
                {
                    await CreateEmployee(response, request);
                }
                // изменение сотрудника по Id
                else if (path == "/api/employee" && request.Method == "PUT")
                {
                    await UpdatePerson(response, request);
                }
                // Удаление сотрудников по Id
                else if (request.Method == "DELETE")
                {
                    string? id = path.Value?.Split("/")[3];
                    await DeletePerson(int.Parse(id), response);
                }*/

                // JSON

                // вывод сотрудников по наименованию компании
                if (request.Method == "GET")
                {
                    var employe = await request.ReadFromJsonAsync<Employee>();
                    if (employe != null)
                    {
                        await GetListEmployesForDepName(employe.Department.DepartmentName, response);
                    }
                }

                // Добавление сотрудника, в ответ приходит его Id
                else if (request.Method == "POST")
                {
                    await CreateEmployee(response, request);
                }
                // изменение сотрудника по Id
                else if (request.Method == "PUT")
                {
                    await UpdatePerson(response, request);
                }
                // Удаление сотрудников по Id
                else if (request.Method == "DELETE")
                {
                    var employe = await request.ReadFromJsonAsync<Employee>();
                    if (employe != null)
                    {
                        await DeletePerson(employe.Id, response);
                    }
                }

            });
        }
    }
}
