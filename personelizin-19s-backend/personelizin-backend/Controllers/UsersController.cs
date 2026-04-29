using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;

namespace SenekaEBDYS.Controllers
{
    [Authorize] // Sadece giriş yapmış kişiler erişebilir
    [ApiController]
    [Route("api/[controller]")] // Bu satır URL'yi 'api/Users' yapar
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context; // 'AppDbContext' yazan yeri kendi Context adınla değiştir

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Veritabanındaki Users tablosuna gider ve listeyi çeker
            var users = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.FullName, // Veritabanındaki sütun adın Name değilse burayı düzelt (örn: FullName)
                    u.Role
                })
                .ToListAsync();

            return Ok(users);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Kullanıcı bulunamadı.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}