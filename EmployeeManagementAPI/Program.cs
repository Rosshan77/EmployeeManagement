using Asp.Versioning;
using Azure;
using EmployeeManagementAPI.BackgroundServices;
using EmployeeManagementAPI.Middleware;
using EmployeeManagementAPI.Validators;
using EmployeeManagementBLL.Interfaces;
using EmployeeManagementBLL.Mappings;
using EmployeeManagementBLL.Services;
using EmployeeManagementDAL.Context;
using EmployeeManagementDAL.Interfaces;
using EmployeeManagementDAL.Models;
using EmployeeManagementDAL.Repositories;
using EmployeeManagementModel.Responses;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;

// ================= SERILOG CONFIGURATION =================
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .ReadFrom.Configuration(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build())
    .CreateLogger();



var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();



// ================= ADD CONTROLLERS =================
builder.Services.AddControllers();


// ================= CUSTOM VALIDATION RESPONSE FORMAT =================

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new
            {
                Field = x.Key,
                Errors = x.Value.Errors.Select(e => e.ErrorMessage).ToArray()
            })
            .ToList();

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "Validation Failed",
            Data = errors
        };

        return new BadRequestObjectResult(response);
    };
});



// ================= SQL SERVER DB CONTEXT =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
    b => b.MigrationsAssembly("EmployeeManagementDAL")));



// ================= DEPENDENCY INJECTION =================
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<PasswordHasherService>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

// =================  IMEMORYCACHE =================
//builder.Services.AddMemoryCache();


// ================= REDIS CACHE =================
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "EmployeeManagementAPI_";
});
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379"));


builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDTOValidator>();



// ================= JWT AUTHENTICATION =================
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });



// ================= AUTHORIZATION =================
builder.Services.AddAuthorization();



// ================= SWAGGER + JWT SUPPORT =================
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Employee Management API V1",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Token only"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ================= RATE LIMITING / API THROTTLING =================
builder.Services.AddRateLimiter(options =>
{
    // ===== AUTH ENDPOINT STRICT LIMIT =====
    options.AddFixedWindowLimiter("AuthPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 5;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

// ===== EMPLOYEE ENDPOINT NORMAL LIMIT =====
    options.AddFixedWindowLimiter("EmployeePolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 30;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429;
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false,
            Message = "Too many requests. Please try again later.",
            Data = null
        };

        var json = System.Text.Json.JsonSerializer.Serialize(response);

        await context.HttpContext.Response.WriteAsync(json, token);
    };
});

// =================== API VERSIONING ====================

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

// =================== RESPONSE COMPRESSION (GZIP / BROTLI) ====================
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

// =================== CORS CONFIGURATION ==================== 
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

// ================= SERILOG REQUEST LOGGING =================
app.UseSerilogRequestLogging();


// ================= GLOBAL EXCEPTION MIDDLEWARE =================
app.UseMiddleware<ExceptionMiddleware>();


// ================= RESPONSE COMPRESSION =================
app.UseResponseCompression();


// =================== CORS ==================== 
app.UseCors("FrontendPolicy");


// ================= SWAGGER =================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee Management API V1");
});

// ================= HTTPS =================
app.UseHttpsRedirection();

// ================= RATE LIMITING =================
app.UseRateLimiter();

// ================= AUTH =================
app.UseAuthentication();
app.UseAuthorization();



// ================= MAP CONTROLLERS =================
app.MapControllers();

app.Run();