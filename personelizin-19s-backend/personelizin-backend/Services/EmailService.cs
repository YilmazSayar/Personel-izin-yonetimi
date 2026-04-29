using System.Net;
using System.Net.Mail;

namespace personelizin_backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        private bool IsEmailConfigured()
        {
            var from = _config["Email:FromAddress"];
            var password = _config["Email:Password"];
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(password)) return false;
            if (from.Contains("BURAYA_") || password.Contains("BURAYA_")) return false;
            return true;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (!IsEmailConfigured()) return;

            var from = _config["Email:FromAddress"]?.Trim() ?? "";
            var fromName = _config["Email:FromDisplayName"]?.Trim() ?? "?zinlerim";
            var password = _config["Email:Password"]?.Trim() ?? "";
            var host = _config["Email:SmtpHost"]?.Trim() ?? "smtp.gmail.com";
            var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(from, password)
            };
            var msg = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            msg.To.Add(toEmail);
            await client.SendMailAsync(msg);
        }

        /// <summary>Yonetici tarafindan olusturulan kullaniciya hesap bilgilerini iceren hos geldin maili gonderir.</summary>
        public async Task SendNewUserWelcomeAsync(string toEmail, string managerName, string unitName, string loginEmail, string loginPassword)
        {
            var intro = string.IsNullOrWhiteSpace(unitName)
                ? $"{managerName} yoneticisi tarafindan kaydiniz olusturulmustur."
                : $"{managerName} yoneticisi tarafindan {unitName.Trim()} isimli birime kaydiniz olusturulmustur.";
            var body = intro + " Lutfen asagidaki hesap bilgileri ile giris yapiniz.\r\n\r\n" +
                       $"E-posta: {loginEmail}\r\nSifre: {loginPassword}\r\n\r\nù Izinlerim";
            var subject = "Izinlerim ù Hesabiniz olusturuldu";
            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
