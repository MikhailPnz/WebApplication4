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

        // ��������� ������
        List<Person> users = new List<Person>
        {
            new() { Id = Guid.NewGuid().ToString(), Name = "Tom", Age = 37 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 41 },
            new() { Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 24 }
        };

        // ��������� ���� �������������
        async Task GetAllPeople(HttpResponse response)
        {
            await response.WriteAsJsonAsync(users);
        }
        // ��������� ������ ������������ �� id
        async Task GetPerson(string? id, HttpResponse response)
        {
            // �������� ������������ �� id
            Person? user = users.FirstOrDefault((u) => u.Id == id);
            // ���� ������������ ������, ���������� ���
            if (user != null)
                await response.WriteAsJsonAsync(user);
            // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
            }
        }

        async Task DeletePerson(string? id, HttpResponse response)
        {
            // �������� ������������ �� id
            Person? user = users.FirstOrDefault((u) => u.Id == id);
            // ���� ������������ ������, ������� ���
            if (user != null)
            {
                users.Remove(user);
                await response.WriteAsJsonAsync(user);
            }
            // ���� �� ������, ���������� ��������� ��� � ��������� �� ������
            else
            {
                response.StatusCode = 404;
                await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
            }
        }

        async Task CreatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                // �������� ������ ������������
                var user = await request.ReadFromJsonAsync<Person>();
                if (user != null)
                {
                    // ������������� id ��� ������ ������������
                    user.Id = Guid.NewGuid().ToString();
                    // ��������� ������������ � ������
                    users.Add(user);
                    await response.WriteAsJsonAsync(user);
                }
                else
                {
                    throw new Exception("������������ ������");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "������������ ������" });
            }
        }

        async Task UpdatePerson(HttpResponse response, HttpRequest request)
        {
            try
            {
                // �������� ������ ������������
                Person? userData = await request.ReadFromJsonAsync<Person>();
                if (userData != null)
                {
                    // �������� ������������ �� id
                    var user = users.FirstOrDefault(u => u.Id == userData.Id);
                    // ���� ������������ ������, �������� ��� ������ � ���������� ������� �������
                    if (user != null)
                    {
                        user.Age = userData.Age;
                        user.Name = userData.Name;
                        await response.WriteAsJsonAsync(user);
                    }
                    else
                    {
                        response.StatusCode = 404;
                        await response.WriteAsJsonAsync(new { message = "������������ �� ������" });
                    }
                }
                else
                {
                    throw new Exception("������������ ������");
                }
            }
            catch (Exception)
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "������������ ������" });
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
                //string expressionForNumber = "^/api/users/([0 - 9]+)$";   // ���� id ������������ �����

                // 2e752824-1657-4c7f-844b-6ec2e168e99c
                string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";
                if (path == "/api/users" && request.Method == "GET")
                {
                    await GetAllPeople(response);
                }
                else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
                {
                    // �������� id �� ������ url
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
