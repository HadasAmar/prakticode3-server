using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TasksPrakticodeServer.Data;
using TasksPrakticodeServer.Models;


namespace TodoApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ItemsController(AppDbContext context)
    {
        _context = context;
    }

    //[HttpGet]
    //public async Task<ActionResult<IEnumerable<Item>>> GetItems()
    //    => await _context.Items.ToListAsync();

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Item>>> GetItemsByUser(int userId)
    {
        return await _context.Items
            .Where(i => i.UserId == userId)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Item>> PostItem(Item item)
    {
        Console.WriteLine("start post item..");
        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        //return CreatedAtAction(nameof(GetItemsByUser), new { id = item.Id }, item);
        return Ok(item);

    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutItem(int id, Item updatedItem)
    {
        var existingItem = await _context.Items.FindAsync(id);
        if (existingItem is null) return NotFound();

        existingItem.Name = updatedItem.Name;
        existingItem.IsComplete = updatedItem.IsComplete;
        await _context.SaveChangesAsync();
        return Ok(existingItem);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(int id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item is null) return NotFound();

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
