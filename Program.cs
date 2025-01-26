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

var app = builder.Build();

// Middleware ל-CORS
app.UseCors();

// נקודות קצה ל-Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDo API V1");
});

// לאחר app.UseCors();
Task.Run(async () =>
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<MyschemaContext>();
    
    while (true)
    {
        CleanExpiredSessions(context);
        Console.WriteLine($"Cleaned expired sessions at {DateTime.UtcNow}");
        
        // המתנה של שעה בין ריצות
        await Task.Delay(TimeSpan.FromHours(1));
    }
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
        expires: DateTime.Now.AddMinutes(1),
        signingCredentials: signinCredentials
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    return new { Token = tokenString };
}



// הוספת סשן
void AddOrUpdateSession(MyschemaContext context, Customer customer, HttpRequest request)
{
    var existingSession = context.Sessions
        .FirstOrDefault(s => s.CustomerId == customer.Id);

    if (existingSession != null)
    {
        Console.WriteLine("you enter to update🎉"); 
        Console.WriteLine("id🎉"+ existingSession.Id); 
        // עדכון סשן קיים
        existingSession.CreatedAt = DateTime.UtcNow;
        existingSession.IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString();
        existingSession.IsValid=true;
    }
    else
    {
                Console.WriteLine("you enter to add");

        // יצירת סשן חדש אם אין קיים
        var lastId = context.Sessions?.Max(u => u.Id) ?? 0;
        context.Sessions?.Add(new Session
        {
            Id = lastId + 1,
            CustomerId = customer.Id,
            CreatedAt = DateTime.UtcNow,
            IpAddress = request.HttpContext.Connection.RemoteIpAddress?.ToString(),
            IsValid = true
        });
    }
    context.SaveChanges();
}


//clean sessions
void CleanExpiredSessions(MyschemaContext context)
{
    var expiredSessions = context.Sessions
        .Where(s => s.IsValid && s.CreatedAt.AddMinutes(60) < DateTime.UtcNow);

    foreach (var session in expiredSessions)
    {
        session.IsValid = false;
    }

    context.SaveChanges();
}


// Login
app.MapPost("/login", async (MyschemaContext context, IConfiguration configuration, Customer model, HttpRequest request) =>
{
    Console.WriteLine("enter😁");

   var customer = context.Customers?.FirstOrDefault(u => u.Name == model.Email&&u.Password==model.Password);
 Console.WriteLine("email:",model.Email);
 Console.WriteLine("pass:",model.Password);
    if (customer is not null)
    {
        var jwt = CreateJWT(customer, configuration);
        AddOrUpdateSession(context, customer, request);
        return Results.Ok(jwt);
    }
    return Results.Unauthorized(); 
});

// Register
app.MapPost("/register", async (MyschemaContext context, IConfiguration configuration, Customer newCustomer, HttpRequest request) =>
{
    var lastId = context.Customers?.Max(u => u.Id) ?? 0;
    Console.WriteLine("enter register");
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
    AddOrUpdateSession(context, newCustomerEntity, request);
    return Results.Ok(jwt);
});

// ברוכים הבאים
app.MapGet("/", () => "Welcome to the ToDo API! Use /items to manage tasks.");

// נקודות קצה לניהול לקוחות
app.MapGet("/customers", async (MyschemaContext context) => await context.Customers.ToListAsync());

app.MapPost("/customers", async (MyschemaContext context, Customer newCustomer) =>
{
    await context.Customers.AddAsync(newCustomer);
    await context.SaveChangesAsync();
    return Results.Created($"/customers/{newCustomer.Id}", newCustomer);
});

app.MapPut("/customers/{id}", async (int id, MyschemaContext context, Customer updatedCustomer) =>
{
    var existingCustomer = await context.Customers.FindAsync(id);
    if (existingCustomer == null) return Results.NotFound();

    existingCustomer.Name = updatedCustomer.Name;
    existingCustomer.Email = updatedCustomer.Email;
    existingCustomer.Password = updatedCustomer.Password;
    await context.SaveChangesAsync();
    return Results.Ok(existingCustomer);
});

app.MapDelete("/customers/{id}", async (int id, MyschemaContext context) =>
{
    var customer = await context.Customers.FindAsync(id);
    if (customer == null) return Results.NotFound();

    context.Customers.Remove(customer);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

// נקודות קצה לניהול פריטים
app.MapGet("/items", async (MyschemaContext context) => await context.Items.ToListAsync());

app.MapPost("/items", async (MyschemaContext context, Item newItem) =>
{
    await context.Items.AddAsync(newItem);
    await context.SaveChangesAsync();
    return Results.Created($"/items/{newItem.Id}", newItem);
});

app.MapPut("/items/{id}", async (int id, MyschemaContext context, Item updatedItem) =>
{
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null) return Results.NotFound();

    existingItem.Name = updatedItem.Name;
    existingItem.IsComplete = updatedItem.IsComplete;
    await context.SaveChangesAsync();
    return Results.Ok(existingItem);
});

app.MapDelete("/items/{id}", async (int id, MyschemaContext context) =>
{
    var item = await context.Items.FindAsync(id);
    if (item == null) return Results.NotFound();

    context.Items.Remove(item);
    await context.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();
