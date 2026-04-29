namespace personelizin_backend.DTOs
{
    public class UploadPdfResponse
    {
        public string? UploadOperationId { get; set; }
        public string? Error { get; set; }
    }

    public class CreateStateRequest
    {
        public string Certificate { get; set; } = "";
        public string UploadOperationId { get; set; } = "";
        public string SignatureLevel { get; set; } = "B-B";
        public string Profile { get; set; } = "None";
        public string HashAlgorithm { get; set; } = "SHA256";
    }

    public class CreateStateResponse
    {
        public string? State { get; set; }
        public string? KeyID { get; set; }
        public string? KeySecret { get; set; }
        public string? SignOperationId { get; set; }
        public string? Error { get; set; }
    }

    public class FinishSignRequest
    {
        public string SignedData { get; set; } = "";
        public string KeyId { get; set; } = "";
        public string KeySecret { get; set; } = "";
        public string SignOperationId { get; set; } = "";
    }

    public class FinishSignResponse
    {
        public bool IsSuccess { get; set; }
        public string? DownloadOperationId { get; set; }
        public string? Error { get; set; }
    }
}
