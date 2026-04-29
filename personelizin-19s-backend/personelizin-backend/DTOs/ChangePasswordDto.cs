namespace personelizin_backend.DTOs
{
    public class ChangePasswordDto
    {
        public string NewPassword { get; set; } = string.Empty;
        public string NewPasswordConfirm { get; set; } = string.Empty;
    }
}
