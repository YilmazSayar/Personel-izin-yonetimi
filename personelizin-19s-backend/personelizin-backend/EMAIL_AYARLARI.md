# İzinlerim e-posta ayarları

Yönetici yeni kullanıcı oluşturduğunda, o kullanıcının e-posta adresine hesap bilgileri maili gider. Mail **İzinlerim** hesabından gönderilir.

## Ne yapmalısınız?

1. **İzinlerim** adında bir e-posta hesabı oluşturun (örn. Gmail: `izinlerim@gmail.com`).

2. Gmail kullanıyorsanız **Uygulama şifresi** alın:
   - Google Hesabı → Güvenlik → 2 adımlı doğrulama (açık olmalı)
   - "Uygulama şifreleri" bölümünden yeni şifre oluşturun.

3. `appsettings.json` dosyasındaki **Email** bölümünü doldurun:

```json
"Email": {
  "FromAddress": "izinlerim@gmail.com",
  "FromDisplayName": "İzinlerim",
  "Password": "BURAYA_OLUSTURDUGUNUZ_16_HANELI_UYGULAMA_SIFRESI",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": 587
}
```

- **FromAddress:** İzinlerim mail adresiniz (tam adres).
- **Password:** Gmail için 16 haneli uygulama şifresi (boşluksuz). Henüz oluşturmadıysanız `BURAYA_UYGULAMA_SIFRESI_GIRIN` yazılı kalsın; mail gönderilmez, kullanıcı kaydı yine yapılır.

Şifreyi hazır olunca `appsettings.json` içindeki `BURAYA_UYGULAMA_SIFRESI_GIRIN` ifadesini bu şifreyle değiştirmeniz yeterli.
