namespace personelizin_backend.Models
{
    public class PermissionRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // KRÝTÝK EKLENTÝ: Bu satýr sayesinde r.User.Email diyebileceðiz
        public User? User { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public int CurrentApproverId { get; set; }
    }
}