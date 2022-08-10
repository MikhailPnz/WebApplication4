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
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionString = "Host=pg_container;Username=root;Password=root;Database=test_db";
            _repo = new EmployeeRepository(connectionString);
            //using (var connection = new NpgsqlConnection(connectionString))
            //{
            //    connection.Open();
            //    connection.Execute("CREATE TABLE Employee (id serial primary key, Name CHARACTER VARYING(30), Surname CHARACTER VARYING(30)," +
            //                       "Phone  CHARACTER VARYING(30), PasportName CHARACTER VARYING(30), PasportNumber CHARACTER VARYING(30)," +
            //                       "DepName CHARACTER VARYING(30), DepPhone CHARACTER VARYING(30));");
            //}
            //connection.Execute("CREATE TABLE Users (Id INTEGER, Name CHARACTER VARYING(30), Age INTEGER);");
            //services.AddTransient<IEmployeeRepository, EmployeeRepository>(provider => new EmployeeRepository(connectionString));
            //services.AddControllersWithViews();
        }

        // начальные данные
        List<Employee> employes = new List<Employee>
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
            Employee? emploe = employes.FirstOrDefault((u) => u.Id == id);
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

            foreach (var employe in employes)
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
            // получаем пользовател¤ по id
            Employee? employe = employes.FirstOrDefault((u) => u.Id == id);
            // если пользователь найден, удал¤ем его
            if (employe != null)
            {
                employes.Remove(employe);
                await response.WriteAsJsonAsync(employe);
            }
            // если не найден, отправл¤ем статусный код и сообщение об ошибке
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "—отрудник не найден" });
            }
        }

        async Task CreateEmployee(HttpResponse response, HttpRequest request)
        {
            try
            {
                var employe = await request.ReadFromJsonAsync<Employee>();
                if (employe != null)
                {
                    var emp = _repo.Create(employe);
                    //employes.Add(employe);
                    await response.WriteAsJsonAsync(emp.Id);
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
                    var employe = employes.FirstOrDefault(u => u.Id == userData.Id);
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
                
                //string expressionForNumber = "^/api/users/([0 - 9]+)$";   // если id представл¤ет число

                // 2e752824-1657-4c7f-844b-6ec2e168e99c
                //string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
                string expressionForGuid = @"^/api/users/$";
                if (request.Method == "GET")
                {
                    if (path == "/api/employee")
                    {
                        await GetAllEmployee(response);
                    }

                    //if (Regex.IsMatch(path, @"^/api/employee/\d{3}$"))
                    //{
                    if (path == "/api/employee/id/")
                    {
                        string? id = path.Value?.Split("/")[3];
                        await GetPerson(int.Parse(id), response);
                    }
                    
                   
                        
                    //}

                    //if (Regex.IsMatch(path, @"^/api/users/\d{3}$"))
                    //{
                        //string? departmentName = path.Value?.Split("/")[3];
                        //await GetEmployeForDepName(departmentName, response);
                    //}


                }
                //else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
                //{
                //    // получаем id из адреса url
                //    string? id = path.Value?.Split("/")[3];
                //    await GetPerson(int.Parse(id), response);
                //}
                // Выводить список сотрудников для указанной компании.
                //else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
                //{
                //    // получаем id из адреса url
                //    string? departmentName = path.Value?.Split("/")[3];
                //    await GetEmployeForDepName(departmentName, response);
                //}
                // ƒобавл¤ть сотрудников, в ответ должен приходить Id добавленного сотрудника.
                else if (path == "/api/users" && request.Method == "POST")
                {
                    await CreateEmployee(response, request);
                }
                else if (path == "/api/users" && request.Method == "PUT")
                {
                    await UpdatePerson(response, request);
                }
                // ”дал¤ть сотрудников по Id.
                //else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
                else if (request.Method == "DELETE")
                {
                    //if (Regex.IsMatch(path, expressionForGuid))
                    //{

                    //}
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
