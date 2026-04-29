namespace personelizin_backend.DTOs
{
    public class PermissionApprovalDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}