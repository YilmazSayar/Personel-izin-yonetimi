using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.DTOs;
using personelizin_backend.Models;
using personelizin_backend.Data;

namespace personelizin_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto login)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == login.Email);
            if (user == null)
                return Unauthorized(new { message = "Email veya şifre hatalı!" });

            var passwordValid = (user.PasswordHash ?? "").StartsWith("$2a$") || (user.PasswordHash ?? "").StartsWith("$2b$")
                ? BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash)
                : (user.PasswordHash == login.Password);

            if (!passwordValid)
                return Unauthorized(new { message = "Email veya şifre hatalı!" });

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Kritik Değişiklik: Role claim'ini buraya ekledik ki Manager yetkileri çalışsın
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.FullName ?? ""),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            var response = new LoginResponseDto
            {
                UserId = user.Id,
                Token = tokenString,
                UserName = user.FullName ?? "",
                UserEmail = user.Email ?? "",
                Role = user.Role ?? "User",
                MustChangePassword = user.MustChangePassword
            };
            return Ok(response);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { message = "Yeni şifre boş olamaz." });
            if (dto.NewPassword != dto.NewPasswordConfirm)
                return BadRequest(new { message = "Yeni şifre ve tekrarı eşleşmiyor." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return Unauthorized();

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword, workFactor: 11);
            user.MustChangePassword = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Şifreniz güncellendi." });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto registerDto)
        {
            if (string.IsNullOrEmpty(registerDto.Email) || string.IsNullOrEmpty(registerDto.Password))
                return BadRequest("Email ve şifre boş olamaz.");

            // Karakter Sınırı Kontrolü (100 Karakter)
            if (!string.IsNullOrEmpty(registerDto.FullName) && registerDto.FullName.Length > 100)
                return BadRequest("Ad Soyad alanı 100 karakteri geçemez.");

            var exists = _context.Users.Any(u => u.Email == registerDto.Email);
            if (exists) return BadRequest("Bu email zaten kayıtlı.");

            var fullName = !string.IsNullOrWhiteSpace(registerDto.FullName) ? registerDto.FullName.Trim() : registerDto.Email;
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password, workFactor: 11);

            var newUser = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                FullName = fullName,
                Role = "User",
                UnitId = null,
                CreatedAt = DateTime.UtcNow,
                RemainingLeaveDays = 15 // Varsayılan izin hakkı
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Kayıt başarıyla oluşturuldu!" });
        }

        [HttpPost("join-unit")]
        [Authorize]
        public async Task<IActionResult> JoinUnit([FromBody] JoinUnitDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return Unauthorized();

            var raw = (dto.InviteCode ?? "").Trim();
            if (!string.IsNullOrEmpty(raw))
                raw = raw.Normalize(System.Text.NormalizationForm.FormKC);

            var code = string.IsNullOrEmpty(raw) ? "" : System.Text.RegularExpressions.Regex.Replace(raw, @"[^A-Za-z0-9]", "");
            code = code.ToUpperInvariant();

            if (code.Length > 6) code = code.Substring(0, 6);
            if (string.IsNullOrEmpty(code))
                return BadRequest("Lütfen 6 haneli davet kodunu girin.");

            var unit = await _context.Units
                .Where(u => u.Code != null && u.Code.ToUpper() == code)
                .FirstOrDefaultAsync();

            if (unit == null)
                return BadRequest("Geçersiz davet kodu. Yöneticinizden doğru birim kodunu alın.");

            var alreadyIn = await _context.UserUnits.AnyAsync(uu => uu.UserId == userId.Value && uu.UnitId == unit.Id);
            if (alreadyIn)
                return Ok(new { message = "Zaten bu birimde kayıtlısınız.", unitId = unit.Id });

            _context.UserUnits.Add(new UserUnit { UserId = userId.Value, UnitId = unit.Id });

            if (user.UnitId == null)
                user.UnitId = unit.Id;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Birime başarıyla katıldınız.", unitId = unit.Id });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return Unauthorized();

            var unitIds = await _context.UserUnits
                .Where(uu => uu.UserId == userId.Value)
                .Select(uu => uu.UnitId)
                .ToListAsync();

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Role,
                user.UnitId,
                unitIds,
                RemainingLeaveDays = user.RemainingLeaveDays
            });
        }

        [HttpPatch("me/full-name")]
        [Authorize]
        public async Task<IActionResult> UpdateFullName([FromBody] UpdateFullNameDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var nextFullName = dto?.FullName?.Trim();
            if (string.IsNullOrWhiteSpace(nextFullName))
                return BadRequest(new { message = "Ad soyad boş olamaz." });
            if (nextFullName.Length > 100)
                return BadRequest(new { message = "Ad Soyad alanı en fazla 100 karakter olabilir." });

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null) return Unauthorized();

            user.FullName = nextFullName;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Ad soyad güncellendi.", fullName = user.FullName });
        }

        [HttpGet("users")]
        [Authorize]
        public async Task<IActionResult> GetAllUsers()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var currentUser = await _context.Users.FindAsync(userId.Value);
            if (currentUser == null) return Unauthorized();

            // Sadece Manager rolü kendi birimindekileri görebilir
            if (!string.Equals(currentUser.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var myUnitIds = await _context.Units
                .Where(u => u.CreatedByUserId == userId.Value)
                .Select(u => u.Id)
                .ToListAsync();

            var userIdsInMyUnits = await _context.UserUnits
                .Where(uu => myUnitIds.Contains(uu.UnitId))
                .Select(uu => uu.UserId)
                .Distinct()
                .ToListAsync();

            var users = await _context.Users
                .Where(u => userIdsInMyUnits.Contains(u.Id) && u.Role == "User")
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.FullName,
                    u.Role,
                    u.UnitId,
                    u.RemainingLeaveDays,
                    UnitName = _context.Units.Where(t => t.Id == u.UnitId).Select(t => t.Name).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var currentUser = await _context.Users.FindAsync(userId.Value);
            if (currentUser == null || !string.Equals(currentUser.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound(new { message = "Kullanıcı bulunamadı." });

            var myUnitIds = await _context.Units
                .Where(u => u.CreatedByUserId == userId.Value)
                .Select(u => u.Id)
                .ToListAsync();

            var isInMyUnit = await _context.UserUnits
                .AnyAsync(uu => uu.UserId == id && myUnitIds.Contains(uu.UnitId));

            if (!isInMyUnit) return Forbid();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kullanıcı başarıyla silindi." });
        }
    }
}