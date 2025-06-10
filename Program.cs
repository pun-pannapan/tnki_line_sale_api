using Serilog;
using System.Net;
using tnki_line_sale_api.Models;
using tnki_line_sale_api.Services;
using tnki_line_sale_api.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add serilog
builder.Host.UseSerilog((context, config) =>
{
    config.WriteTo.File("Logs/rtd_priviledge_log-.log", rollingInterval: RollingInterval.Day);
});

// Injection
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ConnectionString>(builder.Configuration.GetSection("ConnectionStrings"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

// Protocol config
ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

// Add cors policy
var corsPolicy = "corsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy,
                      policy =>
                      {
                          policy.WithOrigins("https://localhost:7014"
                              , "https://localhost:7015"
                              , "https://localhost:7016"
                              )
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                      });
});

builder.Services.AddAutoMapper(typeof(Startup));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//comment of to always show
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseCors(corsPolicy);

app.UseAuthorization();

app.UseMiddleware<JwtMiddleware>();

app.UseJsonApiWrapper();

app.MapControllers();

app.Run();
