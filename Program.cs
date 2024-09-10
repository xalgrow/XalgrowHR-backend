using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using XalgrowHR.Middlewares;  // Ensure you import the ErrorHandlerMiddleware
using XalgrowHR.Services;     // Ensure the namespace for UserService is included
using XalgrowHR.Data;         // Ensure you include the namespace for your DbContext

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// Authorization Policies setup
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PowerUserOnly", policy => policy.RequireRole("PowerUser"));
    options.AddPolicy("HRManagerOnly", policy => policy.RequireRole("HRManager"));
});

// Swagger Configuration
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable middleware for Development environment
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
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
