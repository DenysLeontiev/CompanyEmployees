using AspNetCoreRateLimit;
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

builder.Services.AddControllers(config =>
{
    config.CacheProfiles.Add("120SecondsDuration", new CacheProfile { Duration = 120 }); // caching duration
}).AddNewtonsoftJson();

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
builder.Services.ConfigureVersioning();
builder.Services.ConfigureResponseCaching(); // for caching
builder.Services.ConfigureHttpCacheHeaders(); // for caching

builder.Services.AddMemoryCache(); // Throttling and Rate Liminting relies on that
builder.Services.AddInMemoryRateLimiting(); // Throttling and Rate Liminting relies on that
builder.Services.ConfigureRateLimitOptions();
builder.Services.AddHttpContextAccessor(); // we can gain access to current HttpContext
builder.Services.AddInMemoryRateLimiting();

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

//app.UseIpRateLimiting(); // for Throttling and Rate Limiting

app.UseResponseCaching(); // for caching
app.UseHttpCacheHeaders(); // for caching

app.UseIpRateLimiting();

ILoggerManager? loggerManager = builder.Services.BuildServiceProvider().CreateScope().ServiceProvider.GetRequiredService<ILoggerManager>();
app.ConfigureExceptionHandler(loggerManager);
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
