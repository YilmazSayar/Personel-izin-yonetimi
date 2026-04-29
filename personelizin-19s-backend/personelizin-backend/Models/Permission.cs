using System;
using System.ComponentModel.DataAnnotations;

namespace personelizin_backend.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        /// <summary>Girişte kullanılan e-posta (Kullanıcı sütununda gösterilir)</summary>
        public string? FullName { get; set; }

        /// <summary>İzin türü: Yıllık İzin, Mazeret İzni, Hastalık İzni</summary>
        public string? Type { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; } = "Pending"; // Varsayılan: Beklemede

        /// <summary>Talep hangi birime gitti (kullanıcının birimi)</summary>
        public int? UnitId { get; set; }
        public Unit? Unit { get; set; }

        /// <summary>Talep oluşturulma günü ve saati (UTC)</summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>PrimeAPI'deki imzalı PDF operasyon ID'si (onaylanmış taleplerde)</summary>
        public string? SignedDocumentOperationId { get; set; }

        /// <summary>Kullanıcının yüklediği belgenin sunucu yolu</summary>
        public string? DocumentPath { get; set; }

        /// <summary>Yüklenen belgenin orijinal dosya adı</summary>
        public string? DocumentOriginalName { get; set; }
    }
}