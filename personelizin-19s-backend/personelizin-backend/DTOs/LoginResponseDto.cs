using System.Text.Json.Serialization;

namespace personelizin_backend.DTOs
{
    public class LoginResponseDto
    {
        // [JsonPropertyName] sayesinde Frontend'e küçük harfle gitmesini garanti ediyoruz
        [JsonPropertyName("token")]
        public string Token { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        [JsonPropertyName("userEmail")]
        public string UserEmail { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("mustChangePassword")]
        public bool MustChangePassword { get; set; }
    }
}