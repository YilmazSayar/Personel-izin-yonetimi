using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;
using personelizin_backend.DTOs;
using personelizin_backend.Models;
using personelizin_backend.Services;

namespace personelizin_backend.Controllers
{
    /// <summary>Sistem yönetimi: Kullanıcı listeleme, oluşturma ve yetkilendirme işlemleri.</summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public AdminController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        // Admin veya Manager olup olmadığını kontrol eden merkezi metot
        private async Task<bool> IsAdminOrManagerAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value);
            if (user == null) return false;

            return string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase);
        }

        // Sadece Admin kontrolü (Silme ve Kritik yetki değişimleri için)
        private async Task<bool> IsOnlyAdminAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId.Value);
            return user != null && string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Tüm kullanıcıları listele. Admin ve Manager rollerine açıktır.</summary>
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            if (!await IsAdminOrManagerAsync())
                return Forbid();

            var list = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.FullName)
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.UnitId,
                    u.RemainingLeaveDays,
                    u.CreatedAt,
                    UnitName = u.UnitId != null ? _context.Units.Where(t => t.Id == u.UnitId).Select(t => t.Name).FirstOrDefault() : null
                })
                .ToListAsync();

            var result = list.Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.Role,
                u.UnitId,
                u.RemainingLeaveDays,
                CreatedAt = u.CreatedAt.Kind == DateTimeKind.Utc ? u.CreatedAt : DateTime.SpecifyKind(u.CreatedAt, DateTimeKind.Utc),
                u.UnitName
            }).ToList();

            return Ok(result);
        }

        /// <summary>Yeni kullanıcı oluştur. Admin ve Manager rollerine açıktır.</summary>
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            if (!await IsAdminOrManagerAsync())
                return Forbid();

            var userId = GetCurrentUserId();
            var currentUser = await _context.Users.FindAsync(userId);
            string role = "User";
            if (currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                role = request.Role ?? "User";
            // Manager sadece User oluşturabilir
            if (currentUser != null && string.Equals(currentUser.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                role = "User";

            if (string.IsNullOrEmpty(request.Email))
                return BadRequest(new { message = "E-posta adresi boş olamaz." });
            if (string.IsNullOrEmpty(request.Password))
                return BadRequest(new { message = "Şifre boş olamaz." });

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return BadRequest(new { message = "Bu e-posta adresi zaten kayıtlı." });

            if (!string.IsNullOrEmpty(request.FullName) && request.FullName.Length > 100)
                return BadRequest(new { message = "Ad Soyad alanı en fazla 100 karakter olabilir." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 11);
            var fullName = !string.IsNullOrWhiteSpace(request.FullName) ? request.FullName.Trim() : request.Email;

            var model = new User
            {
                Email = request.Email.Trim(),
                FullName = fullName,
                PasswordHash = passwordHash,
                Role = role,
                UnitId = request.UnitId,
                RemainingLeaveDays = request.RemainingLeaveDays,
                CreatedAt = DateTime.UtcNow,
                MustChangePassword = true
            };
            _context.Users.Add(model);
            await _context.SaveChangesAsync();

            if (request.UnitId.HasValue)
            {
                _context.UserUnits.Add(new UserUnit { UserId = model.Id, UnitId = request.UnitId.Value });
                await _context.SaveChangesAsync();
            }

            try
            {
                var managerName = currentUser?.FullName?.Trim() ?? "Yonetici";
                string? unitName = null;
                if (request.UnitId.HasValue)
                {
                    var unit = await _context.Units.AsNoTracking().FirstOrDefaultAsync(u => u.Id == request.UnitId.Value);
                    unitName = unit?.Name;
                }
                await _emailService.SendNewUserWelcomeAsync(
                    request.Email.Trim(),
                    managerName,
                    unitName ?? "",
                    request.Email.Trim(),
                    request.Password);
            }
            catch { /* E-posta gonderilemezse islem yine basarili sayilir */ }

            return Ok(new { message = "Kullanıcı başarıyla oluşturuldu.", userId = model.Id });
        }

        /// <summary>E-posta ile kullanıcı ara. Admin ve Manager rollerine açıktır.</summary>
        [HttpGet("user-by-email")]
        public async Task<IActionResult> GetUserByEmail([FromQuery] string? email)
        {
            if (!await IsAdminOrManagerAsync())
                return Forbid();

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("E-posta adresi girin.");

            var user = await _context.Users
                .AsNoTracking()
                .Where(u => u.Email != null && u.Email.Trim().ToLower() == email.Trim().ToLower())
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.UnitId,
                    u.CreatedAt,
                    UnitName = u.UnitId != null ? _context.Units.Where(t => t.Id == u.UnitId).Select(t => t.Name).FirstOrDefault() : null
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(new { message = "Bu e-posta ile kayıtlı hesap bulunamadı." });

            var createdAtUtc = user.CreatedAt.Kind == DateTimeKind.Utc
                ? user.CreatedAt
                : DateTime.SpecifyKind(user.CreatedAt, DateTimeKind.Utc);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.FullName,
                user.Role,
                user.UnitId,
                CreatedAt = createdAtUtc,
                user.UnitName
            });
        }

        /// <summary>Kullanıcıyı yönetici (Manager) olarak yükselt. Sadece Admin yapabilir.</summary>
        [HttpPost("promote-to-manager/{userId:int}")]
        public async Task<IActionResult> PromoteToManager(int userId)
        {
            if (!await IsOnlyAdminAsync())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Ok(new { message = "Bu kullanıcı zaten yönetici." });

            user.Role = "Manager";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı yönetici olarak atandı.", userId = user.Id });
        }

        /// <summary>Yöneticiyi kullanıcı (User) rolüne düşür. Sadece Admin yapabilir.</summary>
        [HttpPost("demote-to-user/{userId:int}")]
        public async Task<IActionResult> DemoteToUser(int userId)
        {
            if (!await IsOnlyAdminAsync())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sistem yöneticisi hesabının yetkisi düşürülemez.");

            user.Role = "User";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı yetkisi düşürüldü.", userId = user.Id });
        }

        /// <summary>Kullanıcıyı sil. Sadece Admin yapabilir. Kendi hesabı ve sistem yöneticisi silinemez.</summary>
        [HttpDelete("users/{userId:int}")]
        public async Task<IActionResult> DeleteUser(int userId)
        {
            if (!await IsOnlyAdminAsync())
                return Forbid();

            var currentId = GetCurrentUserId();
            if (currentId == userId)
                return BadRequest("Kendi hesabınızı silemezsiniz.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            if (string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Sistem yöneticisi hesabı silinemez.");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı silindi." });
        }

        /// <summary>Kullanıcının kalan izin gününü günceller. Admin ve Manager yapabilir.</summary>
        [HttpPatch("users/{userId:int}/remaining-leave-days")]
        public async Task<IActionResult> SetUserRemainingLeaveDays(int userId, [FromBody] SetRemainingLeaveDaysRequestAdmin body)
        {
            if (!await IsAdminOrManagerAsync())
                return Forbid();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Kullanıcı bulunamadı." });

            var days = body?.RemainingLeaveDays;
            if (days == null || days < 0)
                return BadRequest("Geçerli bir gün sayısı (0 veya üzeri) girin.");

            user.RemainingLeaveDays = days.Value;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kalan izin günü güncellendi.", remainingLeaveDays = user.RemainingLeaveDays });
        }
    }

    public class SetRemainingLeaveDaysRequestAdmin
    {
        public int? RemainingLeaveDays { get; set; }
    }
}