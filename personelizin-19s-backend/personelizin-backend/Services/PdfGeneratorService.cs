using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using personelizin_backend.Models;

namespace personelizin_backend.Services
{
    public static class PdfGeneratorService
    {
        static PdfGeneratorService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public static byte[] GenerateLeaveRequestPdf(Permission permission, string employeeEmail, string? employeeFullName, string? unitName)
        {
            var days = (permission.EndDate.Date - permission.StartDate.Date).Days + 1;
            var createdAt = permission.CreatedAt?.ToString("dd.MM.yyyy HH:mm") ?? permission.StartDate.ToString("dd.MM.yyyy");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.DefaultTextStyle(t => t.FontFamily("Arial").FontSize(11).FontColor(Colors.Grey.Darken3));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("İZİNLERİM").Bold().FontSize(22).FontColor(Colors.Blue.Darken3);
                                c.Item().Text("İzin Talep Raporu").FontSize(13).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(120).Column(c =>
                            {
                                c.Item().AlignRight().Text($"Rapor No: #{permission.Id}").FontSize(9).FontColor(Colors.Grey.Medium);
                                c.Item().AlignRight().Text(DateTime.Now.ToString("dd.MM.yyyy")).FontSize(9).FontColor(Colors.Grey.Medium);
                            });
                        });
                        col.Item().PaddingTop(8).LineHorizontal(2).LineColor(Colors.Blue.Darken3);
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Çalışan bilgileri
                        col.Item().Background(Colors.Blue.Lighten5).Padding(14).Column(info =>
                        {
                            info.Item().Text("ÇALIŞAN BİLGİLERİ").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                            info.Item().PaddingTop(8).Row(r =>
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(t =>
                                    {
                                        t.Span("Ad Soyad: ").SemiBold();
                                        t.Span(employeeFullName ?? employeeEmail);
                                    });
                                    c.Item().PaddingTop(4).Text(t =>
                                    {
                                        t.Span("E-posta: ").SemiBold();
                                        t.Span(employeeEmail);
                                    });
                                });
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text(t =>
                                    {
                                        t.Span("Birim: ").SemiBold();
                                        t.Span(unitName ?? "—");
                                    });
                                    c.Item().PaddingTop(4).Text(t =>
                                    {
                                        t.Span("Talep Tarihi: ").SemiBold();
                                        t.Span(createdAt);
                                    });
                                });
                            });
                        });

                        col.Item().PaddingTop(16);

                        // İzin detayları
                        col.Item().Text("İZİN DETAYLARI").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                        col.Item().PaddingTop(8).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(160);
                                c.RelativeColumn();
                            });

                            void Row(string label, string value)
                            {
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(label).SemiBold().FontColor(Colors.Grey.Darken2);
                                t.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Text(value);
                            }

                            Row("İzin Türü", permission.Type ?? "Yıllık İzin");
                            Row("Başlangıç Tarihi", permission.StartDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")));
                            Row("Bitiş Tarihi", permission.EndDate.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("tr-TR")));
                            Row("Toplam Süre", $"{days} gün");
                            Row("Durum", "Onay Bekliyor");
                        });

                        if (!string.IsNullOrWhiteSpace(permission.Description))
                        {
                            col.Item().PaddingTop(16).Text("AÇIKLAMA").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                            col.Item().PaddingTop(6).Background(Colors.Grey.Lighten4).Padding(12).Text(permission.Description);
                        }

                        // İmza alanı
                        col.Item().PaddingTop(32).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Height(48);
                                c.Item().PaddingTop(6).AlignCenter().Text("Çalışan İmzası").FontSize(9).FontColor(Colors.Grey.Medium);
                                c.Item().AlignCenter().Text(employeeFullName ?? employeeEmail).FontSize(9);
                            });
                            row.ConstantItem(40);
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().BorderBottom(1).BorderColor(Colors.Grey.Darken1).Height(48);
                                c.Item().PaddingTop(6).AlignCenter().Text("Yönetici E-İmzası").FontSize(9).FontColor(Colors.Grey.Medium);
                                c.Item().AlignCenter().Text("(PAdES Elektronik İmza)").FontSize(8).Italic().FontColor(Colors.Blue.Medium);
                            });
                        });
                    });

                    page.Footer().PaddingTop(12).BorderTop(1).BorderColor(Colors.Grey.Lighten2).Row(row =>
                    {
                        row.RelativeItem().Text("Bu belge İzinlerim sistemi tarafından oluşturulmuştur.").FontSize(8).FontColor(Colors.Grey.Medium);
                        row.ConstantItem(120).AlignRight().Text(t =>
                        {
                            t.Span("Sayfa ").FontSize(8).FontColor(Colors.Grey.Medium);
                            t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            }).GeneratePdf();
        }
    }
}
