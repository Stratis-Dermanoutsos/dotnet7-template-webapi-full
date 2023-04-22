global using Serilog;

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using webapi_full;
using webapi_full.Enums;
using webapi_full.Extensions;
using webapi_full.IServices;
using webapi_full.IUtils;
using webapi_full.Middleware;
using webapi_full.Services;
using webapi_full.Utils;

var builder = WebApplication.CreateBuilder(args);

//? Setup logs using Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

// Add services to the container.

builder.Services.AddControllers();
//? Lowercase URLs
builder.Services.AddRouting(options => options.LowercaseUrls = true);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.SwaggerDoc("v1", new() { Title = "API application generated with Stratis' template.", Version = "v1" });

    //? Add Bearer token authentication to swagger
    options.AddSecurityDefinition("Bearer", new() {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        In = ParameterLocation.Header,
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
    });

    //? Tell swagger to use the Bearer token header
    options.AddSecurityRequirement(new() {
        {
            new() {
                Reference = new() {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            new string[] { }
        }
    });
});

//? Versioning
Log.Information("Versioning the API...");
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver"));
});
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey
                (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
        //? Custom extension method to handle JWT errors (401 & 403)
        options.SetupJwtBearerEvents();
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("admin", policy => policy.Requirements.Add(new RoleRequirement(Role.Admin)));
    options.AddPolicy("user", policy => policy.Requirements.Add(new RoleRequirement(Role.User)));
});

//? Register custom utilities for injection
builder.Services.AddScoped<IUserUtils, UserUtils>();
builder.Services.AddScoped<IPasswordUtils, PasswordUtils>();
builder.Services.AddSingleton<IAuthorizationHandler, RoleAuthorizationHandler>();
builder.Services.AddScoped<IUserService, UserService>();

//? Load PasswordValidator settings and register for injection
builder.Services.AddSingleton<PasswordValidator>(
    builder.Configuration.GetSection("PasswordValidator")
        .Get<PasswordValidator>() ??
    new PasswordValidator());

//? Register custom middleware for injection
builder.Services.AddTransient<ExceptionMiddleware>();

//? Add DbContext and its settings
Log.Information("Connecting to database...");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Demo"))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//? Add my custom exception middleware
app.UseExceptionMiddleware();

Log.Information("Server is running.");

app.Run();
