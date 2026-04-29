using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;
using personelizin_backend.DTOs;
using personelizin_backend.Models;
using System.ComponentModel.DataAnnotations;

namespace personelizin_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PermissionsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Kullanıcının kendi izinlerini listeleme (GET) – sadece userId’ye ait kayıtlar
        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PermissionListItemDto>>> GetPermissions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            var list = await _context.Permissions
                .Where(p => p.UserId == userId.Value)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new PermissionListItemDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == p.UserId).Select(u => u.Email).FirstOrDefault(),
                    Type = p.Type ?? "Yıllık İzin",
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.Description,
                    Status = p.Status,
                    UnitName = p.UnitId != null ? _context.Units.Where(u => u.Id == p.UnitId).Select(u => u.Name).FirstOrDefault() : null,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        [HttpPost]
        [Consumes("multipart/form-data", "application/json")]
        public async Task<IActionResult> CreatePermission([FromForm] PermissionRequestDto request, IFormFile? document)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            var user = await _context.Users.FindAsync(userId.Value);
            var userEmail = user?.Email ?? "";
            var todayUtc = DateTime.UtcNow.Date;
            if (request.StartDate.Date < todayUtc)
                return BadRequest("İzin başlangıç tarihi bugünden önce olamaz. Geçmiş tarihli izin talebi oluşturamazsınız.");
            if (request.EndDate.Date < todayUtc)
                return BadRequest("İzin bitiş tarihi bugünden önce olamaz. Geçmiş tarihli izin talebi oluşturamazsınız.");

            var requestedDays = (request.EndDate.Date - request.StartDate.Date).Days + 1;
            if (requestedDays < 1)
                return BadRequest("Bitiş tarihi başlangıçtan önce olamaz.");
            if (user != null && user.RemainingLeaveDays < requestedDays)
                return BadRequest($"Kalan izin hakkınız ({user.RemainingLeaveDays} gün) bu talep için yeterli değil. Talep {requestedDays} gün.");

            var unitId = request.UnitId ?? user?.UnitId;
            if (unitId.HasValue && user != null)
            {
                var isInUnit = user.UnitId == unitId.Value || await _context.UserUnits
                    .AnyAsync(uu => uu.UserId == user.Id && uu.UnitId == unitId.Value);
                if (!isInUnit)
                    return BadRequest("Seçilen birimin üyesi değilsiniz.");
            }

            // Beklemede veya onaylı izinlerle tarih çakışması var mı? (Reddedilenler hariç)
            var start = request.StartDate.Date;
            var end = request.EndDate.Date;
            var hasOverlap = await _context.Permissions
                .AnyAsync(p => p.UserId == userId.Value
                    && (p.Status == "Pending" || p.Status == "Approved")
                    && p.StartDate.Date <= end
                    && p.EndDate.Date >= start);
            if (hasOverlap)
                return BadRequest("Seçtiğiniz tarihler içerisinde zaten güncel izniniz bulunmakta, lütfen başka seçim yapınız.");

            if (request.Description != null && request.Description.Length > 280)
                return BadRequest("Açıklama en fazla 280 karakter olabilir.");

            var permission = new Permission
            {
                UserId = userId.Value,
                FullName = userEmail,
                Type = request.Type ?? "Yıllık İzin",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Description = request.Description,
                Status = "Pending",
                UnitId = unitId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            // Kullanıcı belge yüklediyse kaydet
            if (document != null && document.Length > 0)
            {
                var allowedTypes = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
                if (!allowedTypes.Contains(document.ContentType.ToLower()))
                    return BadRequest("Yalnızca PDF, JPEG veya PNG dosyaları kabul edilmektedir.");
                if (document.Length > 10 * 1024 * 1024)
                    return BadRequest("Dosya boyutu en fazla 10 MB olabilir.");

                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsDir);
                var ext = Path.GetExtension(document.FileName);
                var fileName = $"perm_{permission.Id}_{Guid.NewGuid():N}{ext}";
                var filePath = Path.Combine(uploadsDir, fileName);
                using var stream = System.IO.File.Create(filePath);
                await document.CopyToAsync(stream);

                permission.DocumentPath = filePath;
                permission.DocumentOriginalName = document.FileName;
                await _context.SaveChangesAsync();
            }

            return Ok(permission);
        }

        [HttpGet("my-permissions")]
        public async Task<ActionResult<IEnumerable<PermissionListItemDto>>> GetMyPermissions()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return Unauthorized("Geçerli oturum yok.");

            var list = await _context.Permissions
                .Where(p => p.UserId == userId.Value)
                .OrderByDescending(p => p.StartDate)
                .Select(p => new PermissionListItemDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == p.UserId).Select(u => u.Email).FirstOrDefault(),
                    Type = p.Type ?? "Yıllık İzin",
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.Description,
                    Status = p.Status,
                    UnitName = p.UnitId != null ? _context.Units.Where(u => u.Id == p.UnitId).Select(u => u.Name).FirstOrDefault() : null,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(list);
        }

        /// <summary>Yönetici: talebi onaylar (sadece o birimin yöneticisi)</summary>
        [HttpPost("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Geçerli oturum yok.");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || !string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound("İzin bulunamadı");

            if (permission.UnitId.HasValue)
            {
                var unit = await _context.Units.FindAsync(permission.UnitId.Value);
                if (unit == null || unit.CreatedByUserId != userId.Value)
                    return Forbid();
            }

            permission.Status = "Approved";

            // Onaylanan talep kaç gün ise kalan izin hakkından o kadar düşer
            var approvedDays = (permission.EndDate.Date - permission.StartDate.Date).Days + 1;
            if (approvedDays < 1) approvedDays = 1;
            var leaveUser = await _context.Users.FindAsync(permission.UserId);
            if (leaveUser != null)
            {
                leaveUser.RemainingLeaveDays = Math.Max(0, leaveUser.RemainingLeaveDays - approvedDays);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "İzin onaylandı", id });
        }

        /// <summary>Yönetici: talebi reddeder (sadece o odada olan yönetici)</summary>
        [HttpPost("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Geçerli oturum yok.");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || !string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var permission = await _context.Permissions.FindAsync(id);
            if (permission == null) return NotFound("İzin bulunamadı");

            if (permission.UnitId.HasValue)
            {
                var unit = await _context.Units.FindAsync(permission.UnitId.Value);
                if (unit == null || unit.CreatedByUserId != userId.Value)
                    return Forbid();
            }

            permission.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok(new { message = "İzin reddedildi", id });
        }

        /// <summary>Yönetici: belirtilen birimdeki tüm izin taleplerini listeler (sadece o birimin yöneticisi görebilir)</summary>
        [HttpGet("by-unit/{unitId:int}")]
        public async Task<ActionResult<IEnumerable<PermissionListItemDto>>> GetByUnit(int unitId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized("Geçerli oturum yok.");

            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || !string.Equals(user.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var unit = await _context.Units.FindAsync(unitId);
            if (unit == null) return NotFound("Birim bulunamadı.");
            if (unit.CreatedByUserId != userId.Value)
                return Forbid();

            var list = await _context.Permissions
                .Where(p => p.UnitId == unitId)
                .OrderByDescending(p => p.CreatedAt ?? p.StartDate)
                .Join(_context.Users, p => p.UserId, u => u.Id, (p, u) => new PermissionListItemDto
                {
                    Id = p.Id,
                    UserId = p.UserId,
                    UserEmail = u.Email,
                    UserFullName = u.FullName,
                    Type = p.Type ?? "Yıllık İzin",
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Description = p.Description,
                    Status = p.Status,
                    UnitName = unit.Name,
                    CreatedAt = p.CreatedAt,
                    SignedDocumentOperationId = p.SignedDocumentOperationId,
                    DocumentOriginalName = p.DocumentOriginalName,
                    HasDocument = p.DocumentPath != null
                })
                .ToListAsync();

            return Ok(list);
        }
    }
}