using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksPrakticodeServer.Data;
using TasksPrakticodeServer.Models;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(Customer customer)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == customer.Email))
            return Conflict("Email already exists");

        customer.Id = (_context.Customers.Max(c => (int?)c.Id) ?? 0) + 1;
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(Customer customer)
    {
        Console.WriteLine("start login..");
        var found = await _context.Customers
            .FirstOrDefaultAsync(c => c.Name == customer.Name && c.Password == customer.Password);

        return found is null ? Unauthorized() : Ok(found);
    }
}
