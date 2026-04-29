using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;
using personelizin_backend.DTOs;
using personelizin_backend.Services;

namespace personelizin_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SignatureController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PrimeApiService _primeApi;

        public SignatureController(AppDbContext context, PrimeApiService primeApi)
        {
            _context = context;
            _primeApi = primeApi;
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claim, out var id) ? id : null;
        }

        /// <summary>İzin talebi için PDF oluştur ve PrimeAPI'ye yükle. UploadOperationId döner.</summary>
        [HttpGet("upload-pdf/{permissionId:int}")]
        public async Task<IActionResult> UploadPdf(int permissionId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var manager = await _context.Users.FindAsync(userId.Value);
            if (manager == null || !string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var permission = await _context.Permissions
                .Include(p => p.Unit)
                .FirstOrDefaultAsync(p => p.Id == permissionId);
            if (permission == null) return NotFound("İzin talebi bulunamadı.");

            if (permission.UnitId.HasValue)
            {
                var unit = await _context.Units.FindAsync(permission.UnitId.Value);
                if (unit == null || unit.CreatedByUserId != userId.Value)
                    return Forbid();
            }

            var employee = await _context.Users.FindAsync(permission.UserId);
            var employeeEmail = employee?.Email ?? permission.FullName ?? "—";
            var employeeFullName = employee?.FullName;
            var unitName = permission.Unit?.Name;

            byte[] pdfBytes;
            string fileName;

            // Kullanıcı belge yüklediyse onu kullan; yoksa PDF üret
            if (!string.IsNullOrEmpty(permission.DocumentPath) && System.IO.File.Exists(permission.DocumentPath))
            {
                pdfBytes = await System.IO.File.ReadAllBytesAsync(permission.DocumentPath);
                fileName = permission.DocumentOriginalName ?? $"belge_{permission.Id}.pdf";
            }
            else
            {
                try
                {
                    pdfBytes = PdfGeneratorService.GenerateLeaveRequestPdf(permission, employeeEmail, employeeFullName, unitName);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new UploadPdfResponse { Error = $"PDF oluşturulamadı: {ex.Message}" });
                }
                fileName = $"izin_raporu_{permission.Id}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";
            }
            var (opId, error) = await _primeApi.UploadFileAsync(pdfBytes, fileName);

            if (!string.IsNullOrEmpty(error))
                return StatusCode(502, new UploadPdfResponse { Error = error });

            return Ok(new UploadPdfResponse { UploadOperationId = opId });
        }

        /// <summary>PAdES V4 imza adım 1: sertifika gönder, hash/state döner.</summary>
        [HttpPost("create-state")]
        public async Task<IActionResult> CreateState([FromBody] CreateStateRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var manager = await _context.Users.FindAsync(userId.Value);
            if (manager == null || !string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var (state, keyId, keySecret, signOpId, error) = await _primeApi.CreateStateForPadesAsync(
                request.Certificate,
                request.UploadOperationId,
                request.SignatureLevel,
                request.Profile,
                request.HashAlgorithm
            );

            if (!string.IsNullOrEmpty(error))
                return StatusCode(502, new CreateStateResponse { Error = error });

            return Ok(new CreateStateResponse
            {
                State = state,
                KeyID = keyId,
                KeySecret = keySecret,
                SignOperationId = signOpId
            });
        }

        /// <summary>PAdES V4 imza adım 3: imzalı veriyi gönder, PDF tamamlanır ve izin onaylanır.</summary>
        [HttpPost("finish-sign/{permissionId:int}")]
        public async Task<IActionResult> FinishSign(int permissionId, [FromBody] FinishSignRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var manager = await _context.Users.FindAsync(userId.Value);
            if (manager == null || !string.Equals(manager.Role, "Manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            var permission = await _context.Permissions.FindAsync(permissionId);
            if (permission == null) return NotFound("İzin talebi bulunamadı.");

            if (permission.UnitId.HasValue)
            {
                var unit = await _context.Units.FindAsync(permission.UnitId.Value);
                if (unit == null || unit.CreatedByUserId != userId.Value)
                    return Forbid();
            }

            var (isSuccess, downloadOpId, error) = await _primeApi.FinishSignForPadesAsync(
                request.SignedData,
                request.KeyId,
                request.KeySecret,
                request.SignOperationId
            );

            if (!string.IsNullOrEmpty(error))
                return StatusCode(502, new FinishSignResponse { Error = error });

            if (!isSuccess)
                return StatusCode(502, new FinishSignResponse { Error = "İmzalama başarısız oldu." });

            // İzni onayla ve imzalı belge operationId'yi kaydet
            permission.Status = "Approved";
            permission.SignedDocumentOperationId = downloadOpId;

            var approvedDays = (permission.EndDate.Date - permission.StartDate.Date).Days + 1;
            if (approvedDays < 1) approvedDays = 1;

            var employee = await _context.Users.FindAsync(permission.UserId);
            if (employee != null)
                employee.RemainingLeaveDays = Math.Max(0, employee.RemainingLeaveDays - approvedDays);

            await _context.SaveChangesAsync();

            return Ok(new FinishSignResponse
            {
                IsSuccess = true,
                DownloadOperationId = downloadOpId
            });
        }

        /// <summary>İmzalı PDF'i indir.</summary>
        [HttpGet("download/{operationId}")]
        public async Task<IActionResult> DownloadSignedPdf(string operationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var (data, fileName, error) = await _primeApi.DownloadFileAsync(operationId);

            if (!string.IsNullOrEmpty(error))
                return StatusCode(502, new { error });

            return File(data!, "application/pdf", fileName ?? "imzali_izin_raporu.pdf");
        }
    }
}
