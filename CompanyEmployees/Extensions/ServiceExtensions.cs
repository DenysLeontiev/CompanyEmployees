using Contracts;
using Entities;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Repository;
using System.Reflection.PortableExecutable;

namespace CompanyEmployees.Extensions
{
    public static class ServiceExtensions
    {
        public static void  ConfigureCors(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin()  // allows requests from any source(WithOrigins("https://example.com") - allows prom particular source);
                           .AllowAnyMethod()  // allows any type of method(POST, GET, etc.);
                           .AllowAnyHeader(); // allows any type of header;
                });
            });
        }

        public static void ConfigureIISIntergration(this IServiceCollection services)
        {
            services.Configure<IISOptions>(options =>
            {
                // Here we use default options to configure IISOptios
            });
        }

        public static void ConfigureLoggerServices(this IServiceCollection services)
        {
            services.AddScoped<ILoggerManager, LoggerManager>();
        }

        public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RepositoryContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("SqlConnection"), b =>
                {
                    b.MigrationsAssembly("CompanyEmployees"); // because our migration assemly is not in the current project,but in the Entities project
                });
            });
        }

        public static void ConfigureRepositoryManager(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryManager, RepositoryManager>();
        }

        public static void ConfigureVersioning(this IServiceCollection services)
        {
            services.AddApiVersioning(opts =>
            {
                opts.ReportApiVersions = true; // adds the API version to the response header
                opts.AssumeDefaultVersionWhenUnspecified = true; // It specifies the default API version if the client doesn’t send one.
                opts.DefaultApiVersion = new ApiVersion(1, 0); // self explanatory
                opts.ApiVersionReader = new HeaderApiVersionReader("api-version"); // if we don’t want to change the URI of the API, we can send the version in the HTTP Header
            });
        }
    }
}
