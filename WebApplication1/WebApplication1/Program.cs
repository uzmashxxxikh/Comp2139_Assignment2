using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net;
using WebApplication1.Services;
using Swashbuckle.AspNetCore.Swagger;

namespace WebApplication1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Smart Inventory Management System API",
                    Version = "v1"
                });
            });

            // Register email services
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ISmtpClient, SmtpClientWrapper>(sp => 
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                var smtpSettings = configuration.GetSection("SmtpSettings");
                var smtpClient = new System.Net.Mail.SmtpClient(smtpSettings["Server"])
                {
                    Port = int.Parse(smtpSettings["Port"] ?? "587"),
                    Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]),
                    EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true")
                };
                return new SmtpClientWrapper(smtpClient);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Smart Inventory Management System API V1");
                });
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
} 