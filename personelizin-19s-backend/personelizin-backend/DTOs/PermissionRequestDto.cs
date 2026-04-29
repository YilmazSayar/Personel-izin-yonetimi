using System;
using System.Text.Json.Serialization;

namespace personelizin_backend.DTOs
{
    // İSMİ DEĞİŞTİRDİK: Artık Controller'ın aradığı isimle birebir aynı!
    public class PermissionRequestDto
    {
        public int UserId { get; set; }
        public int? UnitId { get; set; }
        public string? Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
    }

    public class PermissionResponseDto
    {
        public int Id { get; set; }
        public string? PersonName { get; set; }
        public string? Status { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>Liste için: Kullanıcı e-postası ve izin türü dahil</summary>
    public class PermissionListItemDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserEmail { get; set; }
        /// <summary>Kullanıcı adı soyadı (arama için).</summary>
        public string? UserFullName { get; set; }
        public string? Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        /// <summary>Talep gönderilen birim adı (varsa).</summary>
        public string? UnitName { get; set; }
        /// <summary>Talep oluşturulma günü ve saati (UTC). Yöneticiler çalışanın ne zaman gönderdiğini görebilir.</summary>
        [JsonPropertyName("createdAt")]
        public DateTime? CreatedAt { get; set; }

        /// <summary>PrimeAPI imzalı belge operasyon ID'si (varsa indir butonu gösterilir)</summary>
        public string? SignedDocumentOperationId { get; set; }

        /// <summary>Kullanıcının yüklediği belgenin orijinal adı</summary>
        public string? DocumentOriginalName { get; set; }

        /// <summary>Kullanıcı belge yüklediyse true</summary>
        public bool HasDocument { get; set; }
    }
}