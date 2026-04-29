namespace personelizin_backend.DTOs
{
    public class UserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        /// <summary>Yönetici için "Yonetici" veya "Manager", kullanıcı için "Kullanici" veya "User"</summary>
        public string? Role { get; set; }
        /// <summary>Yönetici kaydı için 6 haneli kod (060606)</summary>
        public string? AdminCode { get; set; }
        /// <summary>Çalışan kaydı için birim davet kodu (yönetici birim oluşturup kodu paylaşır)</summary>
        public string? InviteCode { get; set; }
    }
}