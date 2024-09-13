using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using XalgrowHR.Middlewares;  // Ensure you import the ErrorHandlerMiddleware
using XalgrowHR.Services;     // Ensure the namespace for UserService is included
using XalgrowHR.Data;         // Ensure you include the namespace for your DbContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger Configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "XalgrowHR API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT token into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        Scheme = "Bearer"
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
            new string[] { }
        }
    });
});

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder => policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// Register AppDbContext with PostgreSQL connection string
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register UserService
builder.Services.AddScoped<UserService>(); // Register the UserService for dependency injection

// Register EmailService
builder.Services.AddScoped<EmailService>(); // Add the EmailService for dependency injection

// Register FileStorageService
builder.Services.AddScoped<FileStorageService>(); // Add the FileStorageService for file handling

// Ensure the JWT Key is present in the configuration or throw an exception if missing
var key = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing in the configuration.");

// JWT Authentication setup
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],   // Ensure this is not empty
            ValidAudience = builder.Configuration["Jwt:Audience"], // Ensure this is not empty
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)) // Use the `key` from configuration
        };
    });

// Authorization Policies setup
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PowerUserOnly", policy => policy.RequireRole("PowerUser"));
    options.AddPolicy("HRManagerOnly", policy => policy.RequireRole("HRManager"));
});

var app = builder.Build();

// Enable middleware for Development environment
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "XalgrowHR API v1");
        c.RoutePrefix = string.Empty;
    });
}

// Global Error Handling Middleware
app.UseMiddleware<ErrorHandlerMiddleware>(); // Register the global error handler

// CORS Middleware
app.UseCors("AllowAll"); // Apply the CORS policy

// Ensure correct order for Authentication and Authorization
app.UseAuthentication(); // Add JWT Authentication middleware
app.UseAuthorization();  // Add Authorization middleware

// Map all Controllers
app.MapControllers();

app.Run();
