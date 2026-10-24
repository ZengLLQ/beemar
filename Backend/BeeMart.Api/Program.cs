using BeeMart.Api.Data;
using BeeMart.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure Database
var provider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "Sqlite";
var connectionString = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
    ? builder.Configuration.GetConnectionString("SqlServerConnection")
    : builder.Configuration.GetConnectionString("SqliteConnection");

builder.Services.AddDbContext<BeeMartDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// 2. Register Application Services
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();

// 3. Configure CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (corsOrigins.Length > 0)
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
        else
        {
            policy.SetIsOriginAllowed(_ => true)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// 4. Add Controllers
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var action = context.ActionDescriptor.DisplayName ?? "";
            if (action.Contains("Login", StringComparison.OrdinalIgnoreCase))
            {
                return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                    BeeMart.Api.DTOs.ApiResponse<BeeMart.Api.DTOs.LoginResponse>.Fail("Invalid login"));
            }
            var errors = context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
                BeeMart.Api.DTOs.ApiResponse<object>.Fail(errors.FirstOrDefault() ?? "Validasi gagal.", errors));
        };
    });

// 5. Configure Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BeeMart POS API",
        Version = "v1",
        Description = "RESTful API backend for Binus Point of Sale (POS) Web App BeeMart"
    });
});

var app = builder.Build();

// Auto-migrate & seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<BeeMartDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        await DbInitializer.InitializeAsync(context, hasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

// HTTP request pipeline
if (app.Environment.IsDevelopment() || true) // enable Swagger for demonstration/testing
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeeMart POS API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseCors("AllowFrontend");

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
