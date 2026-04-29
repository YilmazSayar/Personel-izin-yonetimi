namespace personelizin_backend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        /// <summary>Çal??an?n ba?l? oldu?u birim (davet kodu ile kay?t olunca atan?r).</summary>
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        /// <summary>Kalan y?ll?k izin hakk? (gün). Sadece izin onayland???nda dü?er.</summary>
        public int RemainingLeaveDays { get; set; } = 14;

        /// <summary>Hesab?n olu?turulma (kay?t) tarihi.</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Yönetici taraf?ndan olu?turulan kullan?c?lar ilk giri?te ?ifre de?i?tirmek zorunda.</summary>
        public bool MustChangePassword { get; set; }

        public ICollection<PermissionRequest> PermissionRequests { get; set; } = new List<PermissionRequest>();
    }
}