using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using LoggerService;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Repository;

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//ServiceExtensions
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntergration();
builder.Services.ConfigureLoggerServices();
builder.Services.ConfigureSqlContext(builder.Configuration);
builder.Services.ConfigureRepositoryManager();

//ActionFilters
builder.Services.AddScoped<ValidationFilterAttribute>(); // validation attribute for PUT,POST Requests
builder.Services.AddScoped<ValidateCompanyExistsAttribute>();
builder.Services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();
builder.Services.AddScoped<CompanyExistsAttribute>();
builder.Services.AddScoped<EmployeeExistsAttribute>();

builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>>();

builder.Services.Configure<ApiBehaviorOptions>(options => // to return 422 in place of 400(Bad Request) when the ModelState is invalid
{
    options.SuppressModelStateInvalidFilter = true;
});

//Configuring AutoMapper for this project
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.All
});

ILoggerManager? loggerManager = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(loggerManager);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
