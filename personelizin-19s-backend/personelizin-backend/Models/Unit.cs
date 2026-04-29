using System.ComponentModel.DataAnnotations;

namespace personelizin_backend.Models
{
    /// <summary>Yöneticinin oluşturduğu birim; çalışanlar davet kodu ile bu birime kayıt olur.</summary>
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        /// <summary>Davet kodu – çalışanlar kayıt olurken bu kodu girer.</summary>
        public string Code { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CreatedByUserId { get; set; }
    }
}
