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
                    connection.Execute("CREATE TABLE Employee (id serial primary key, Name CHARACTER VARYING(30), Surname CHARACTER VARYING(30)," +
                                       "Phone  CHARACTER VARYING(30), CompanyId integer, PasportType CHARACTER VARYING(30), PasportNumber CHARACTER VARYING(30)," +
                                       "DepName CHARACTER VARYING(30), DepPhone CHARACTER VARYING(30));");
                }
                catch
                {
                    Console.WriteLine("Таблица Emploee уже существует!");
                }
            }

            if (_repo.GetEmployees().Count == 0)
            {
                foreach (var employe in _employes)
                {
                    _repo.Create(employe);
                }
            }
        }

        // начальные данные
        List<Employee> _employes = new List<Employee>
        {
            new()
            {
                Id = 25,
                Name = "Дима",
                Surname = "Петров",
                Phone = "354353",
                CompanyId = 345353,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Ozon", "67868684")
            },

            new()
            {
                Id = 32,
                Name = "Алла",
                Surname = "Сергеева",
                Phone = "8687687",
                CompanyId = 343242,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Oriflame", "456345645")
            },

            new()
            {
                Id = 41,
                Name = "Иван",
                Surname = "Иванов",
                Phone = "7865789",
                CompanyId = 34234324,
                Passport = new Passport("Ordinary Passport", Guid.NewGuid().ToString()),
                Department = new  Department("Ozon", "654645654")
            }
        };

        // получение всех пользователей
        async Task GetAllEmployee(HttpResponse response)
        {
            var employees = _repo.GetEmployees();
            if (employees != null)
            {
                await response.WriteAsJsonAsync(_repo.GetEmployees());
            }
            else
            {
                //response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Список пуст" });
            }
            
        }
        // получение одного пользовател¤ по id
        async Task GetPerson(int? id, HttpResponse response)
        {
            // получаем пользовател¤ по id
            Employee? emploe = _employes.FirstOrDefault((u) => u.Id == id);
            // если пользователь найден, отправл¤ем его
            if (emploe != null)
                await response.WriteAsJsonAsync(emploe);
            // если не найден, отправл¤ем статусный код и сообщение об ошибке
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "Пользователь не найден" });
            }
        }

        // получение списка работников по названию компании
        async Task GetEmployeForDepName(string? depName, HttpResponse response)
        {
            bool search = false;
            List<Employee> employesTemp = new List<Employee>();

            foreach (var employe in _employes)
            {
                if (employe.Department.Name == depName)
                {
                    employesTemp.Add(employe);
                    search = true;
                }
            }

            if (search)
            {
                await response.WriteAsJsonAsync(employesTemp);
                employesTemp.Clear();
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
                var id_ = (int)id;

                Employee? employe = _repo.Get(id_);

                if (employe != null)
                {
                    _repo.Delete(id_);
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
                await response.WriteAsJsonAsync(new { message = "Неверный запрос, укажите id" });
            }
        }

        async Task CreateEmployee(HttpResponse response, HttpRequest request)
        {
            try
            {
                var employe = await request.ReadFromJsonAsync<Employee>();
                if (employe != null) // проверить существует ли уже такой сотрудник
                {
                    var emp = _repo.Create(employe);
                    //_employes.Add(employe);

                    var id = emp.Id;

                    await response.WriteAsJsonAsync(id);
                }
                else
                {
                    throw new Exception("Некорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Ќекорректные данные" });
            }
        }

        async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                // получаем данные пользовател¤
                Employee? userData = await request.ReadFromJsonAsync<Employee>();
                if (userData != null)
                {
                    // foreach (var param in context.Request.Query)
                    // получаем пользовател¤ по id
                    var employe = _employes.FirstOrDefault(u => u.Id == userData.Id);
                    // если пользователь найден, измен¤ем его данные и отправл¤ем обратно клиенту
                    if (employe != null)
                    {
                        employe.Id = userData.Id;
                        employe.Name = userData.Name;
                        employe.Surname = userData.Surname;
                        employe.Phone = userData.Phone;
                        employe.CompanyId = userData.CompanyId;
                        employe.Passport = userData.Passport; // исправить
                        employe.Department = userData.Department;

                        await response.WriteAsJsonAsync(employe.Id);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
                    }
                }
                else
                {
                    throw new Exception("Ќекорректные данные");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "Ќекорректные данные" });
            }
        }







        //    public string Id { get; set; } = "";
        //    public string Name { get; set; } = "";
        //    public int Age { get; set; }
        //}

        
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
                
                string expressionForNumber = "^/api/employee/([0 - 9]+)$"; // id
                string expressionForString = "^/api/employee/([a - z]+)$"; // name
                // 2e752824-1657-4c7f-844b-6ec2e168e99c
                //string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
                string expressionForGuid = @"^/api/users/$";
                if (request.Method == "GET")
                {
                    // вывод всех сотрудников
                    if (path == "/api/employee")
                    {
                        await GetAllEmployee(response);
                    }

                    // вывод сотрудника по id
                    if (Regex.IsMatch(path, expressionForNumber))
                    {
                        string? id = path.Value?.Split("/")[3];
                        await GetPerson(int.Parse(id), response);
                    }

                    // вывод сотрудников по наименованию отдела
                    if (Regex.IsMatch(path, expressionForString))
                    {
                        string? departmentName = path.Value?.Split("/")[3];
                        await GetEmployeForDepName(departmentName, response);
                    }
                }
                else if (Regex.IsMatch(path, expressionForNumber) && request.Method == "GET")
                {
                    string? id = path.Value?.Split("/")[3];
                    await GetPerson(int.Parse(id), response);
                }
                // Выводить список сотрудников для указанной компании.
                //else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
                //{
                //    // получаем id из адреса url
                //    string? departmentName = path.Value?.Split("/")[3];
                //    await GetEmployeForDepName(departmentName, response);
                //}

                // Добавление сотрудника, в ответ приходит его Id
                else if (path == "/api/employee" && request.Method == "POST")
                {
                    await CreateEmployee(response, request);
                }

                else if (path == "/api/users" && request.Method == "PUT")
                {
                    await UpdatePerson(response, request);
                }
                // Удаление сотрудников по Id
                else if (request.Method == "DELETE")
                {
                    string? id = path.Value?.Split("/")[3];
                    await DeletePerson(int.Parse(id), response);
                }
                //else
                //{
                //    response.ContentType = "text/html; charset=utf-8";
                //    //await response.SendFileAsync("html/index.html");
                //}
            });
        }
    }
}
