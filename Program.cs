using TodoApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;



var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();


// הוספת הממשק של Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToDo API",
        Version = "v1",
        Description = "A simple API for managing tasks"
    });
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure DbContext עם MySQL
builder.Services.AddDbContext<MyschemaContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.40-mysql")
    )
);

// הוספת אימות JWT
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"]))
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return context.Response.WriteAsync("Token expired.");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return context.Response.WriteAsync("Authentication failed.");
                }
            },
            OnMessageReceived = context =>
            {
                return Task.CompletedTask;
            }
        };
    });


builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API V1");
});

// פונקציה ליצירת JWT
object CreateJWT(Customer customer, IConfiguration configuration)
{
    var claims = new List<Claim>
    {
        new Claim("id", customer.Id.ToString()),
        new Claim("name", customer.Name),
        new Claim("email", customer.Email),
    };

    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Secret"]));
    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
    var tokenOptions = new JwtSecurityToken(
        issuer: configuration["JwtSettings:Issuer"],
        audience: configuration["JwtSettings:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(1), // תוקף הטוקן
        signingCredentials: signinCredentials
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    return new { Token = tokenString };
}

// Login
app.MapPost("/login", async (MyschemaContext context, IConfiguration configuration, Customer model) =>
{
    var customer = context.Customers?.FirstOrDefault(u => u.Name == model.Email && u.Password == model.Password);
    if (customer is not null)
    {
        var jwt = CreateJWT(customer, configuration);
        return Results.Ok(jwt);
    }
    return Results.Unauthorized();
});

// Register
app.MapPost("/register", async (MyschemaContext context, IConfiguration configuration, Customer newCustomer) =>
{
    var lastId = context.Customers?.Max(u => u.Id) ?? 0;
    var newCustomerEntity = new Customer
    {
        Id = lastId + 1,
        Name = newCustomer.Name,
        Email = newCustomer.Email,
        Password = newCustomer.Password,
    };
    await context.Customers!.AddAsync(newCustomerEntity);
    await context.SaveChangesAsync();

    var jwt = CreateJWT(newCustomerEntity, configuration);
    Console.WriteLine("💦💦💦");
    return Results.Ok(jwt);
});

// ברוכים הבאים
app.MapGet("/", () => "Welcome to the ToDo API! Use /items to manage tasks.");

app.MapGet("/items", async (MyschemaContext context) => {
    var items=await context.Items.ToListAsync();
        return Results.Ok(items); 
}).RequireAuthorization();

app.MapPost("/items", async (MyschemaContext context, Item newItem) =>
{
    await context.Items.AddAsync(newItem);
    await context.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
}).RequireAuthorization();

app.MapPut("/items/{id}", async (int id, MyschemaContext context, Item updatedItem) =>
{
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null) 
        return Results.NotFound();

    existingItem.Name = updatedItem.Name;
    existingItem.IsComplete = updatedItem.IsComplete;
    await context.SaveChangesAsync();
    return Results.Ok(existingItem);
}).RequireAuthorization();

app.MapDelete("/items/{id}", async (int id, MyschemaContext context) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null) 
        return Results.NotFound();

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
}).RequireAuthorization();

app.Run();
