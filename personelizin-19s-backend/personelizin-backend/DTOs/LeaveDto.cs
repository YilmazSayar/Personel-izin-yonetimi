namespace personelizin_backend.DTOs
{
    public class LeaveDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string Status { get; set; } = "Beklemede";
    }
}