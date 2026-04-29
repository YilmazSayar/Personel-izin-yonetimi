using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;
using personelizin_backend.Models;

namespace personelizin_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UnitsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UnitsController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        private bool IsManager()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = _context.Users.Find(userId.Value);
            return string.Equals(user?.Role, "Manager", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAdmin()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return false;
            var user = _context.Users.Find(userId.Value);
            return string.Equals(user?.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>6 karakterlik davet kodu üretir (harf ve rakam)</summary>
        private static string GenerateUnitCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var sb = new StringBuilder(6);
            var rnd = new Random();
            for (int i = 0; i < 6; i++)
                sb.Append(chars[rnd.Next(chars.Length)]);
            return sb.ToString();
        }

        /// <summary>Yönetici: birim oluşturur, davet kodu atanır. Çalışanlar bu kodla kayıt olur.</summary>
        [HttpPost]
        public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
        {
            if (!IsManager())
                return Forbid();

            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            var name = (request?.Name ?? "").Trim();
            if (string.IsNullOrEmpty(name))
                return BadRequest("Birim adı girin.");

            var code = GenerateUnitCode();
            var unit = new Unit
            {
                Name = name,
                Code = code,
                CreatedByUserId = userId.Value
            };
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();

            return Ok(new { unit.Id, unit.Name, unit.Code, unit.CreatedAt });
        }

        /// <summary>Admin: tüm birimler. Yönetici (Manager): kendi oluşturduğu birimler. Çalışan: kendi birimini görür.</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UnitListItem>>> GetMyUnits()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            if (IsAdmin())
            {
                var list = await _context.Units
                    .OrderBy(u => u.Name)
                    .Select(u => new UnitListItem
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Code = u.Code,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();
                return Ok(list);
            }

            if (IsManager())
            {
                var list = await _context.Units
                    .Where(u => u.CreatedByUserId == userId.Value)
                    .OrderByDescending(u => u.CreatedAt)
                    .Select(u => new UnitListItem
                    {
                        Id = u.Id,
                        Name = u.Name,
                        Code = u.Code,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync();
                return Ok(list);
            }

            var unitIds = await _context.UserUnits
                .Where(uu => uu.UserId == userId.Value)
                .Select(uu => uu.UnitId)
                .ToListAsync();
            if (unitIds.Count == 0)
                return Ok(new List<UnitListItem>());

            var userUnitsList = await _context.Units
                .Where(u => unitIds.Contains(u.Id))
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UnitListItem
                {
                    Id = u.Id,
                    Name = u.Name,
                    Code = u.Code,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
            return Ok(userUnitsList);
        }

        /// <summary>Birimdeki çalışanları listeler (sadece o birimin yöneticisi veya aynı birimdeki kullanıcı görebilir). canEditLeaveDays: sadece birim yöneticisi izin günü güncelleyebilir.</summary>
        [HttpGet("{unitId:int}/members")]
        public async Task<ActionResult<UnitMembersResponse>> GetUnitMembers(int unitId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                return NotFound("Birim bulunamadı.");

            var isManagerOfUnit = unit.CreatedByUserId == userId.Value;
            var currentUser = await _context.Users.FindAsync(userId.Value);
            var isInUnit = await _context.UserUnits.AnyAsync(uu => uu.UserId == userId.Value && uu.UnitId == unitId);
            var isSystemAdmin = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isManagerOfUnit && !isInUnit && !isSystemAdmin)
                return Forbid();

            var canEditLeaveDays = isManagerOfUnit || isSystemAdmin;

            var memberUserIds = await _context.UserUnits.Where(uu => uu.UnitId == unitId).Select(uu => uu.UserId).ToListAsync();
            var creatorId = unit.CreatedByUserId;
            if (!memberUserIds.Contains(creatorId))
                memberUserIds.Add(creatorId);
            var members = await _context.Users
                .Where(u => memberUserIds.Contains(u.Id))
                .Select(u => new UnitMemberDto
                {
                    UserId = u.Id,
                    Email = u.Email ?? "",
                    FullName = u.FullName ?? "",
                    IsCreator = u.Id == unit.CreatedByUserId,
                    RemainingLeaveDays = u.RemainingLeaveDays
                })
                .ToListAsync();

            return Ok(new UnitMembersResponse { Members = members, CanEditLeaveDays = canEditLeaveDays });
        }

        /// <summary>Yönetici: birimdeki bir çalışanın kalan izin gününü günceller (sadece o birimin yöneticisi).</summary>
        [HttpPatch("{unitId:int}/users/{userId:int}/remaining-leave-days")]
        public async Task<IActionResult> SetMemberRemainingLeaveDays(int unitId, int userId, [FromBody] SetRemainingLeaveDaysRequest body)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Geçerli oturum yok.");

            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                return NotFound("Birim bulunamadı.");

            var currentUser = await _context.Users.FindAsync(currentUserId.Value);
            var isUnitManager = unit.CreatedByUserId == currentUserId.Value;
            var isSystemAdmin = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isUnitManager && !isSystemAdmin)
                return StatusCode(403, new { message = "Sadece birim yöneticisi veya sistem yöneticisi izin gününü güncelleyebilir." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");
            var isInUnit = await _context.UserUnits.AnyAsync(uu => uu.UserId == userId && uu.UnitId == unitId);
            if (!isInUnit && user.Id != unit.CreatedByUserId)
                return BadRequest("Kullanıcı bu birimde değil.");

            var days = body?.RemainingLeaveDays;
            if (days == null || days < 0)
                return BadRequest("Geçerli bir gün sayısı (0 veya üzeri) girin.");

            user.RemainingLeaveDays = days.Value;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Kalan izin günü güncellendi.", remainingLeaveDays = user.RemainingLeaveDays });
        }

        /// <summary>Yönetici: çalışanı birimden çıkarır (kullanıcı silinmez, sadece UnitId null yapılır). Birim yöneticisini (creator) birimden çıkaramazsınız.</summary>
        [HttpDelete("{unitId:int}/users/{userId:int}")]
        public async Task<IActionResult> RemoveUserFromUnit(int unitId, int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Geçerli oturum yok.");

            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                return NotFound("Birim bulunamadı.");

            var currentUser = await _context.Users.FindAsync(currentUserId.Value);
            var isUnitManager = unit.CreatedByUserId == currentUserId.Value;
            var isSystemAdmin = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isUnitManager && !isSystemAdmin)
                return StatusCode(403, new { message = "Sadece birim yöneticisi veya sistem yöneticisi çalışanı birimden çıkarabilir." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Kullanıcı bulunamadı.");
            var userUnit = await _context.UserUnits.FirstOrDefaultAsync(uu => uu.UserId == userId && uu.UnitId == unitId);
            if (userUnit == null)
                return BadRequest("Kullanıcı bu birimde değil.");
            if (user.Id == unit.CreatedByUserId)
                return BadRequest("Birim yöneticisini birimden çıkaramazsınız. Önce birimi kapatabilir veya başka bir yönetici atayabilirsiniz.");

            _context.UserUnits.Remove(userUnit);
            if (user.UnitId == unitId)
            {
                var other = await _context.UserUnits.FirstOrDefaultAsync(uu => uu.UserId == userId && uu.UnitId != unitId);
                user.UnitId = other?.UnitId;
            }
            await _context.SaveChangesAsync();
            return Ok(new { message = "Çalışan birimden çıkarıldı." });
        }

        /// <summary>Yönetici: birimi kapatır (siler). Birimdeki tüm çalışanların UnitId'si null yapılır, izin taleplerindeki UnitId null yapılır, sonra birim silinir.</summary>
        [HttpDelete("{unitId:int}")]
        public async Task<IActionResult> DeleteUnit(int unitId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized("Geçerli oturum yok.");

            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null)
                return NotFound("Birim bulunamadı.");

            var currentUser = await _context.Users.FindAsync(currentUserId.Value);
            var isUnitManager = unit.CreatedByUserId == currentUserId.Value;
            var isSystemAdmin = currentUser != null && string.Equals(currentUser.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            if (!isUnitManager && !isSystemAdmin)
                return StatusCode(403, new { message = "Sadece birim yöneticisi veya sistem yöneticisi birimi kapatabilir." });

            var userUnitsInUnit = await _context.UserUnits.Where(uu => uu.UnitId == unitId).ToListAsync();
            _context.UserUnits.RemoveRange(userUnitsInUnit);
            foreach (var u in await _context.Users.Where(u => u.UnitId == unitId).ToListAsync())
                u.UnitId = null;

            var permissionsInUnit = await _context.Permissions.Where(p => p.UnitId == unitId).ToListAsync();
            foreach (var p in permissionsInUnit)
                p.UnitId = null;

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Birim kapatıldı." });
        }
    }

    public class UnitMemberDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = "";
        public string FullName { get; set; } = "";
        public bool IsCreator { get; set; }
        public int RemainingLeaveDays { get; set; }
    }

    public class SetRemainingLeaveDaysRequest
    {
        public int? RemainingLeaveDays { get; set; }
    }

    public class UnitMembersResponse
    {
        public List<UnitMemberDto> Members { get; set; } = new();
        public bool CanEditLeaveDays { get; set; }
    }

    public class CreateUnitRequest
    {
        public string? Name { get; set; }
    }

    public class UnitListItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Code { get; set; } = "";
        public DateTime CreatedAt { get; set; }
    }
}
