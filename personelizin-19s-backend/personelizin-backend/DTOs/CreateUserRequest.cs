namespace personelizin_backend.DTOs
{
    /// <summary>Yönetici/Manager tarafından yeni kullanıcı oluşturmak için istek.</summary>
    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Role { get; set; }
        public int? UnitId { get; set; }
        public int RemainingLeaveDays { get; set; } = 14;
    }
}
