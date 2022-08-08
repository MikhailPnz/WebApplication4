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

namespace WebApplication4
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // начальные данные
        List<Person> users = new List<Person>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Tom", Age = 37 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 41 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 24 }
        };

        // получение всех пользователей
        async Task GetAllPeople(HttpResponse response)
        {
            await response.WriteAsJsonAsync(users);
        }
        // получение одного пользовател€ по id
        async Task GetPerson(string? id, HttpResponse response)
        {
            // получаем пользовател€ по id
            Person? user = users.FirstOrDefault((u) => u.Id == id);
            // если пользователь найден, отправл€ем его
            if (user != null)
                await response.WriteAsJsonAsync(user);
            // если не найден, отправл€ем статусный код и сообщение об ошибке
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
            }
        }

        async Task DeletePerson(string? id, HttpResponse response)
        {
            // получаем пользовател€ по id
            Person? user = users.FirstOrDefault((u) => u.Id == id);
            // если пользователь найден, удал€ем его
            if (user != null)
            {
                users.Remove(user);
                await response.WriteAsJsonAsync(user);
            }
            // если не найден, отправл€ем статусный код и сообщение об ошибке
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "ѕользователь не найден" });
            }
        }

        async Task CreatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                // получаем данные пользовател€
                var user = await request.ReadFromJsonAsync<Person>();
                if (user != null)
                {
                    // устанавливаем id дл€ нового пользовател€
                    user.Id = Guid.NewGuid().ToString();
                    // добавл€ем пользовател€ в список
                    users.Add(user);
                    await response.WriteAsJsonAsync(user);
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

        async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                // получаем данные пользовател€
                Person? userData = await request.ReadFromJsonAsync<Person>();
                if (userData != null)
                {
                    // получаем пользовател€ по id
                    var user = users.FirstOrDefault(u => u.Id == userData.Id);
                    // если пользователь найден, измен€ем его данные и отправл€ем обратно клиенту
                    if (user != null)
                    {
                        user.Age = userData.Age;
                        user.Name = userData.Name;
                        await response.WriteAsJsonAsync(user);
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

        public class Passport
        {
            string Type { get; set; } = "";
            string Number { get; set; } = "";
        }
        public class Department
        {
            string Name { get; set; } = "";
            string Phone { get; set; } = "";
        }

        public class Employee
        {
            int Id { get; set; }
            string Name { get; set; } = "";
            string Surname { get; set; } = "";
            private string Phone { get; set; } = "";
            int CompanyId { get; set; }
            Passport Passport { get; set; }
            Department Department { get; set; }
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
                //string expressionForNumber = "^/api/users/([0 - 9]+)$";   // если id представл€ет число

                // 2e752824-1657-4c7f-844b-6ec2e168e99c
                string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
                if (path == "/api/users" && request.Method == "GET")
                {
                    await GetAllPeople(response);
                }
                else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
                {
                    // получаем id из адреса url
                    string? id = path.Value?.Split("/")[3];
                    await GetPerson(id, response);
                }
                else if (path == "/api/users" && request.Method == "POST")
                {
                    await CreatePerson(response, request);
                }
                else if (path == "/api/users" && request.Method == "PUT")
                {
                    await UpdatePerson(response, request);
                }
                else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
                {
                    string? id = path.Value?.Split("/")[3];
                    await DeletePerson(id, response);
                }
                else
                {
                    response.ContentType = "text/html; charset=utf-8";
                    //await response.SendFileAsync("html/index.html");
                }
            });
        }
    }
}
