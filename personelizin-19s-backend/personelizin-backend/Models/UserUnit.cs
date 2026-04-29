namespace personelizin_backend.Models
{
    /// <summary>Kullanıcı–birim çoklu üyelik (bir kullanıcı birden fazla birime katılabilir).</summary>
    public class UserUnit
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public int UnitId { get; set; }
        public Unit Unit { get; set; } = null!;
    }
}
