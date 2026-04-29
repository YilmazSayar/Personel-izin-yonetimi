using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace personelizin_backend.Services
{
    public class PrimeApiService
    {
        private readonly HttpClient _http;
        private readonly string _apiKey;
        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = null // PascalCase – PrimeAPI bunu bekliyor
        };

        public PrimeApiService(HttpClient http, IConfiguration config)
        {
            _http = http;
            var baseUrl = config["PrimeApi:BaseUrl"] ?? "https://apitest.onaylarim.com";
            _apiKey = config["PrimeApi:ApiKey"] ?? "";
            _http.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        }

        // PDF'i PrimeAPI'ye yükler, uploadOperationId döner
        public async Task<(string? OperationId, string? Error)> UploadFileAsync(byte[] pdfBytes, string fileName)
        {
            using var content = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(pdfBytes);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
            content.Add(fileContent, "file", fileName);

            using var req = new HttpRequestMessage(HttpMethod.Post, "V2/CoreApiFile/UploadFile");
            req.Headers.Add("X-API-KEY", _apiKey);
            req.Content = content;

            var resp = await _http.SendAsync(req);
            var body = await resp.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var error = GetString(root, "Error");
            if (!string.IsNullOrEmpty(error)) return (null, error);

            var opId = GetString(root, "OperationId");
            return (opId, null);
        }

        // PAdES V4 imza adım 1: hash hesapla, state döner
        public async Task<(string? State, string? KeyID, string? KeySecret, string? OperationId, string? Error)>
            CreateStateForPadesAsync(string certificate, string uploadOperationId, string signatureLevel, string profile, string hashAlgorithm)
        {
            var payload = new
            {
                Certificate = certificate,
                OperationId = uploadOperationId,
                SignatureLevel = signatureLevel,
                Profile = profile,
                HashAlgorithm = hashAlgorithm,
                SignatureWidgetInfo = (object?)null,
                RequestId = GenerateRequestId(),
                DisplayLanguage = "tr"
            };

            var result = await PostJsonAsync("V4/CoreApiPades/SignStepOnePadesCore", payload);
            if (result is null) return (null, null, null, null, "PrimeAPI bağlantı hatası");

            var error = GetString(result.Value, "Error");
            if (!string.IsNullOrEmpty(error)) return (null, null, null, null, error);

            return (
                GetString(result.Value, "State"),
                GetString(result.Value, "KeyID"),
                GetString(result.Value, "KeySecret"),
                GetString(result.Value, "OperationId"),
                null
            );
        }

        // PAdES V4 imza adım 3: imzayı tamamla
        public async Task<(bool IsSuccess, string? OperationId, string? Error)>
            FinishSignForPadesAsync(string signedData, string keyId, string keySecret, string signOperationId)
        {
            var payload = new
            {
                SignedData = signedData,
                KeyId = keyId,
                KeySecret = keySecret,
                OperationId = signOperationId,
                RequestId = GenerateRequestId(),
                DisplayLanguage = "tr"
            };

            var result = await PostJsonAsync("V4/CoreApiPades/SignStepThreePadesCore", payload);
            if (result is null) return (false, null, "PrimeAPI bağlantı hatası");

            var error = GetString(result.Value, "Error");
            if (!string.IsNullOrEmpty(error)) return (false, null, error);

            var isSuccess = result.Value.TryGetProperty("IsSuccess", out var s) && s.GetBoolean();
            var opId = GetString(result.Value, "OperationId");
            return (isSuccess, opId, null);
        }

        // İmzalı PDF'i indir
        public async Task<(byte[]? Data, string? FileName, string? Error)> DownloadFileAsync(string operationId)
        {
            var payload = new
            {
                OperationId = operationId,
                RequestId = GenerateRequestId(),
                DisplayLanguage = "tr"
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "v2/CoreApiFile/DownloadCore");
            req.Headers.Add("X-API-KEY", _apiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return (null, null, $"HTTP {(int)resp.StatusCode}");

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            var root = doc.RootElement;

            var error = GetString(root, "Error");
            if (!string.IsNullOrEmpty(error)) return (null, null, error);

            // Dosya verisi Result.FileData içinde base64
            if (!root.TryGetProperty("Result", out var resultEl))
                return (null, null, "Geçersiz yanıt");

            var fileDataB64 = GetString(resultEl, "FileData");
            var fileName = GetString(resultEl, "FileName") ?? "imzali_izin_raporu.pdf";
            if (string.IsNullOrEmpty(fileDataB64)) return (null, null, "Dosya verisi boş");

            return (Convert.FromBase64String(fileDataB64), fileName, null);
        }

        private async Task<JsonElement?> PostJsonAsync(string path, object payload)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, path);
                req.Headers.Add("X-API-KEY", _apiKey);
                req.Content = new StringContent(JsonSerializer.Serialize(payload, _json), Encoding.UTF8, "application/json");

                var resp = await _http.SendAsync(req);
                var body = await resp.Content.ReadAsStringAsync();
                var doc = JsonDocument.Parse(body);
                return doc.RootElement.Clone();
            }
            catch
            {
                return null;
            }
        }

        private static string? GetString(JsonElement el, string key)
        {
            if (el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String)
                return v.GetString();
            return null;
        }

        private static string GenerateRequestId()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Range(0, 21).Select(_ => chars[Random.Shared.Next(chars.Length)]).ToArray());
        }
    }
}
