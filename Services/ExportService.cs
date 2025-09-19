using OfficeOpenXml;

using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.Services;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using iText.IO.Font;
using QRCodeManagerRelease2.ViewModels;
using iText.Layout.Element;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Borders;

namespace QRCodeManagerRelease2.Services
{
    public class ExportService : IExportService
    {
        public async Task<byte[]> ExportUsersToExcel(IEnumerable<ApplicationUser> users)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Utenti");
            
            // Headers
            worksheet.Cells[1, 1].Value = "Email";
            worksheet.Cells[1, 2].Value = "Nome";
            worksheet.Cells[1, 3].Value = "Cognome";
            worksheet.Cells[1, 4].Value = "Tipo";
            worksheet.Cells[1, 5].Value = "Azienda";
            worksheet.Cells[1, 6].Value = "Stato";
            worksheet.Cells[1, 7].Value = "Data Registrazione";
            
            // Data
            int row = 2;
            foreach (var user in users)
            {
                worksheet.Cells[row, 1].Value = user.Email;
                worksheet.Cells[row, 2].Value = user.FirstName;
                worksheet.Cells[row, 3].Value = user.LastName;
                worksheet.Cells[row, 4].Value = user.IsAzienda ? "Azienda" : "Privato";
                worksheet.Cells[row, 5].Value = user.Company?.RagioneSociale ?? user.NomeAzienda ?? "-";
                worksheet.Cells[row, 6].Value = user.Abilitato ? "Abilitato" : "Da Abilitare";
                worksheet.Cells[row, 7].Value = user.DataRegistrazione.ToString("dd/MM/yyyy");
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        public async Task<byte[]> ExportUsersToPdf(IEnumerable<ApplicationUser> users)
        {
            using var stream = new MemoryStream();

            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf, PageSize.A4.Rotate());

            // Font
 
            PdfFont titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, PdfEncodings.WINANSI);
            PdfFont textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);



            // Titolo
            document.Add(new Paragraph("Lista Utenti").SetFont(titleFont).SetFontSize(16));
            document.Add(new Paragraph(" "));

            // Tabella con 7 colonne
            var table = new Table(7).UseAllAvailableWidth();

            // Intestazioni
            table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nome").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Cognome").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tipo").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Azienda").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Stato").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Data Reg.").SetFont(titleFont)));

            // Dati
            foreach (var user in users)
            {
                table.AddCell(new Cell().Add(new Paragraph(user.Email ?? "").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.FirstName ?? "").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.LastName ?? "").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.IsAzienda ? "Azienda" : "Privato").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.Company?.RagioneSociale ?? user.NomeAzienda ?? "-").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.Abilitato ? "Abilitato" : "Da Abilitare").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(user.DataRegistrazione.ToString("dd/MM/yyyy")).SetFont(textFont)));
            }

            document.Add(table);
            document.Close();

            return await Task.FromResult(stream.ToArray());
        }


        public async Task<byte[]> ExportCompaniesToExcel(IEnumerable<Company> companies)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Aziende");
            
            worksheet.Cells[1, 1].Value = "Ragione Sociale";
            worksheet.Cells[1, 2].Value = "Codice";
            worksheet.Cells[1, 3].Value = "Partita IVA";
            worksheet.Cells[1, 4].Value = "Descrizione";
            worksheet.Cells[1, 5].Value = "Data Inserimento";
            
            int row = 2;
            foreach (var company in companies)
            {
                worksheet.Cells[row, 1].Value = company.RagioneSociale;
                worksheet.Cells[row, 2].Value = company.Codice;
                worksheet.Cells[row, 3].Value = company.PartitaIva;
                worksheet.Cells[row, 4].Value = company.Descrizione;
                worksheet.Cells[row, 5].Value = company.DataInserimento.ToString("dd/MM/yyyy");
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }



public async Task<byte[]> ExportCompaniesToPdf(IEnumerable<Company> companies)
    {
        using var stream = new MemoryStream();

        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4);

            // Font
            PdfFont titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, PdfEncodings.WINANSI);
            PdfFont textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);

            // Titolo
            document.Add(new Paragraph("Lista Aziende").SetFont(titleFont).SetFontSize(16));
        document.Add(new Paragraph(" "));

        // Tabella con 4 colonne
        var table = new Table(4).UseAllAvailableWidth();

        // Intestazioni
        table.AddHeaderCell(new Cell().Add(new Paragraph("Ragione Sociale").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("P.IVA").SetFont(titleFont)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Codice").SetFont(titleFont)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Descrizione").SetFont(titleFont)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Data Inserimento").SetFont(titleFont)));

        // Dati
        foreach (var company in companies)
        {
            table.AddCell(new Cell().Add(new iText.Layout.Element.Paragraph(company.RagioneSociale ?? "").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(company.PartitaIva ?? "").SetFont(textFont)));
                table.AddCell(new Cell().Add(new Paragraph(company.Codice ?? "").SetFont(textFont)));
            table.AddCell(new Cell().Add(new Paragraph(company.Descrizione ?? "").SetFont(textFont)));
            table.AddCell(new Cell().Add(new Paragraph(company.DataInserimento.ToString("dd/MM/yyyy")).SetFont(textFont)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }

    public async Task<byte[]> ExportQRCodesToExcel(IEnumerable<QRCode> qrCodes)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("QR Codes");
            
            worksheet.Cells[1, 1].Value = "Codice";
            worksheet.Cells[1, 2].Value = "Extracted Code";
            worksheet.Cells[1, 3].Value = "Link Destinazione";
            worksheet.Cells[1, 4].Value = "Utente";
            worksheet.Cells[1, 5].Value = "N° Chiamate";
            worksheet.Cells[1, 6].Value = "Ultima Chiamata";
            worksheet.Cells[1, 7].Value = "Stato";
            
            int row = 2;
            foreach (var qr in qrCodes)
            {
                worksheet.Cells[row, 1].Value = qr.Content;
                worksheet.Cells[row, 2].Value = qr.ExtractedCode;
                worksheet.Cells[row, 3].Value = qr.DestinationLink ?? "-";
                worksheet.Cells[row, 4].Value = qr.CreatedBy?.Email ?? "-";
                worksheet.Cells[row, 5].Value = qr.NumeroChiamate;
                worksheet.Cells[row, 6].Value = qr.UltimaChiamata?.ToString("dd/MM/yyyy HH:mm") ?? "Mai";
                worksheet.Cells[row, 7].Value = qr.Bloccato ? "Bloccato" : "Attivo";
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }



public async Task<byte[]> ExportQRCodesToPdf(IEnumerable<QRCode> qrCodes)
    {
        using var stream = new MemoryStream();

        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4.Rotate());

        // Font
        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, PdfEncodings.WINANSI);
        var headerFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD, PdfEncodings.WINANSI);
        var dataFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);

        // Titolo
        document.Add(new Paragraph("Lista QR Codes").SetFont(titleFont).SetFontSize(16));
        document.Add(new Paragraph(" "));

        // Tabella a 7 colonne, larghezza piena
        var table = new Table(7).UseAllAvailableWidth();

        // Intestazioni
        table.AddHeaderCell(new Cell().Add(new Paragraph("Codice QR").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Codice Completo").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Link").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Utente").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Chiamate").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Ultima Chiamata").SetFont(headerFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Stato").SetFont(headerFont).SetFontSize(9)));

        // Dati
        foreach (var qr in qrCodes)
        {
            table.AddCell(new Cell().Add(new Paragraph(qr.Content ?? "").SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.ExtractedCode ?? "").SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.DestinationLink ?? "-").SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.CreatedBy?.Email ?? "-").SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.NumeroChiamate.ToString()).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.UltimaChiamata?.ToString("dd/MM/yyyy") ?? "Mai").SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(qr.Bloccato ? "Bloccato" : "Attivo").SetFont(dataFont).SetFontSize(8)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }


    public async Task<byte[]> ExportActivityLogsToExcel(IEnumerable<ActivityLog> logs)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Activity Logs");
            
            worksheet.Cells[1, 1].Value = "Data/Ora";
            worksheet.Cells[1, 2].Value = "Utente";
            worksheet.Cells[1, 3].Value = "Azione";
            worksheet.Cells[1, 4].Value = "Entità";
            worksheet.Cells[1, 5].Value = "Dettagli";
            worksheet.Cells[1, 6].Value = "IP Address";
            
            int row = 2;
            foreach (var log in logs)
            {
                worksheet.Cells[row, 1].Value = log.DataOperazione.ToString("dd/MM/yyyy HH:mm:ss");
                worksheet.Cells[row, 2].Value = log.User?.Email ?? "Sistema";
                worksheet.Cells[row, 3].Value = log.Azione;
                worksheet.Cells[row, 4].Value = log.Entita;
                worksheet.Cells[row, 5].Value = log.Dettagli;
                worksheet.Cells[row, 6].Value = log.IPAddress ?? "-";
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }

        
public async Task<byte[]> ExportActivityLogsToPdf(IEnumerable<ActivityLog> logs)
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4.Rotate(), false);

        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Titolo
        document.Add(new Paragraph("Log Attività")
            .SetFont(titleFont)
            .SetFontSize(16)
            .SetMarginBottom(10));

        // Tabella: 6 colonne
        var table = new Table(UnitValue.CreatePercentArray(6)).UseAllAvailableWidth();

        // Intestazioni
        table.AddHeaderCell(new Cell().Add(new Paragraph("Data/Ora").SetFont(titleFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Utente").SetFont(titleFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Azione").SetFont(titleFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Entità").SetFont(titleFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Dettagli").SetFont(titleFont).SetFontSize(9)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("IP").SetFont(titleFont).SetFontSize(9)));

        // Dati
        foreach (var log in logs)
        {
            table.AddCell(new Cell().Add(new Paragraph(log.DataOperazione.ToString("dd/MM/yyyy HH:mm")).SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(log.User?.Email ?? "Sistema").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(log.Azione ?? "").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(log.Entita ?? "").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(log.Dettagli ?? "").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(log.IPAddress ?? "-").SetFont(textFont).SetFontSize(8)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }




        public async Task<byte[]> ExportCodeRangesToExcel(IEnumerable<CodeRange> ranges)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Range Codici");
            
            worksheet.Cells[1, 1].Value = "Codice Iniziale";
            worksheet.Cells[1, 2].Value = "Codice Finale";
            worksheet.Cells[1, 3].Value = "Azienda";
            worksheet.Cells[1, 4].Value = "Data Inserimento";
            worksheet.Cells[1, 5].Value = "Data Modifica";
            
            int row = 2;
            foreach (var range in ranges)
            {
                worksheet.Cells[row, 1].Value = range.CodiceIniziale;
                worksheet.Cells[row, 2].Value = range.CodiceFinale;
                worksheet.Cells[row, 3].Value = range.Company?.RagioneSociale ?? "Generale";
                worksheet.Cells[row, 4].Value = range.DataInserimento.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 5].Value = range.DataModifica?.ToString("dd/MM/yyyy") ?? "-";
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }


public async Task<byte[]> ExportCodeRangesToPdf(IEnumerable<CodeRange> ranges)
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4);

        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Titolo
        document.Add(new Paragraph("Range Codici")
            .SetFont(titleFont)
            .SetFontSize(16)
            .SetMarginBottom(10));

        // Tabella con 5 colonne, larghezza 100%
        var table = new Table(UnitValue.CreatePercentArray(5)).UseAllAvailableWidth();

        // Intestazioni
        table.AddHeaderCell(new Cell().Add(new Paragraph("Codice Iniziale").SetFont(titleFont).SetFontSize(10)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Codice Finale").SetFont(titleFont).SetFontSize(10)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Azienda").SetFont(titleFont).SetFontSize(10)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Data Inserimento").SetFont(titleFont).SetFontSize(10)));
        table.AddHeaderCell(new Cell().Add(new Paragraph("Data Modifica").SetFont(titleFont).SetFontSize(10)));

        // Dati
        foreach (var range in ranges)
        {
            table.AddCell(new Cell().Add(new Paragraph(range.CodiceIniziale ?? "").SetFont(textFont).SetFontSize(10)));
            table.AddCell(new Cell().Add(new Paragraph(range.CodiceFinale ?? "").SetFont(textFont).SetFontSize(10)));
            table.AddCell(new Cell().Add(new Paragraph(range.Company?.RagioneSociale ?? "Generale").SetFont(textFont).SetFontSize(10)));
            table.AddCell(new Cell().Add(new Paragraph(range.DataInserimento.ToString("dd/MM/yyyy")).SetFont(textFont).SetFontSize(10)));
            table.AddCell(new Cell().Add(new Paragraph(range.DataModifica?.ToString("dd/MM/yyyy") ?? "-").SetFont(textFont).SetFontSize(10)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }


    public async Task<byte[]> ExportAnomaliesToExcel(IEnumerable<Anomaly> anomalies)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Anomalie");
            
            worksheet.Cells[1, 1].Value = "Tipo";
            worksheet.Cells[1, 2].Value = "Descrizione";
            worksheet.Cells[1, 3].Value = "Codice Interessato";
            worksheet.Cells[1, 4].Value = "Utente";
            worksheet.Cells[1, 5].Value = "Data Segnalazione";
            worksheet.Cells[1, 6].Value = "Stato";
            worksheet.Cells[1, 7].Value = "Data Risoluzione";
            worksheet.Cells[1, 8].Value = "Risolta Da";
            
            int row = 2;
            foreach (var anomaly in anomalies)
            {
                worksheet.Cells[row, 1].Value = anomaly.Tipo;
                worksheet.Cells[row, 2].Value = anomaly.Descrizione;
                worksheet.Cells[row, 3].Value = anomaly.CodiceInteressato ?? "-";
                worksheet.Cells[row, 4].Value = anomaly.User?.Email ?? "-";
                worksheet.Cells[row, 5].Value = anomaly.DataSegnalazione.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 6].Value = anomaly.Risolta ? "Risolta" : "Aperta";
                worksheet.Cells[row, 7].Value = anomaly.DataRisoluzione?.ToString("dd/MM/yyyy HH:mm") ?? "-";
                worksheet.Cells[row, 8].Value = anomaly.RisoltaDa?.Email ?? "-";
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }


public async Task<byte[]> ExportAnomaliesToPdf(IEnumerable<Anomaly> anomalies)
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4.Rotate());

        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var textFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Titolo
        document.Add(new Paragraph("Lista Anomalie")
            .SetFont(titleFont)
            .SetFontSize(16)
            .SetMarginBottom(10));

        // Tabella con 8 colonne
        var table = new Table(UnitValue.CreatePercentArray(8)).UseAllAvailableWidth();

        // Intestazioni
        var headers = new[]
        {
        "Tipo", "Descrizione", "Codice", "Utente", "Data Segnalazione",
        "Stato", "Data Risoluzione", "Risolta Da"
    };

        foreach (var header in headers)
        {
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph(header).SetFont(titleFont).SetFontSize(9)));
        }

        // Dati
        foreach (var anomaly in anomalies)
        {
            table.AddCell(new Cell().Add(new Paragraph(anomaly.Tipo ?? "").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.Descrizione ?? "").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.CodiceInteressato ?? "-").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.User?.Email ?? "-").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.DataSegnalazione.ToString("dd/MM/yyyy")).SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.Risolta ? "Risolta" : "Aperta").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.DataRisoluzione?.ToString("dd/MM/yyyy") ?? "-").SetFont(textFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(anomaly.RisoltaDa?.Email ?? "-").SetFont(textFont).SetFontSize(8)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }




    public async Task<byte[]> ExportCompanyStatisticsToExcel(IEnumerable<CompanyStatViewModel> statistics)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Statistiche Aziende");
            
            worksheet.Cells[1, 1].Value = "Azienda";
            worksheet.Cells[1, 2].Value = "Codice";
            worksheet.Cells[1, 3].Value = "N° Utenti";
            worksheet.Cells[1, 4].Value = "QR Totali";
            worksheet.Cells[1, 5].Value = "QR Utilizzati";
            worksheet.Cells[1, 6].Value = "QR Rimanenti";
            worksheet.Cells[1, 7].Value = "% Utilizzo";
            
            int row = 2;
            foreach (var stat in statistics)
            {
                var remaining = stat.TotalQRCodes - stat.UsedQRCodes;
                var percentage = stat.TotalQRCodes > 0 ? (double)stat.UsedQRCodes / stat.TotalQRCodes * 100 : 0;
                
                worksheet.Cells[row, 1].Value = stat.Company.RagioneSociale;
                worksheet.Cells[row, 2].Value = stat.Company.Codice;
                worksheet.Cells[row, 3].Value = stat.Company.Users?.Count ?? 0;
                worksheet.Cells[row, 4].Value = stat.TotalQRCodes;
                worksheet.Cells[row, 5].Value = stat.UsedQRCodes;
                worksheet.Cells[row, 6].Value = remaining;
                worksheet.Cells[row, 7].Value = $"{percentage:F1}%";
                row++;
            }
            
            worksheet.Cells.AutoFitColumns();
            return await Task.FromResult(package.GetAsByteArray());
        }


public async Task<byte[]> ExportCompanyStatisticsToPdf(IEnumerable<CompanyStatViewModel> statistics)
    {
        using var stream = new MemoryStream();
        var writer = new PdfWriter(stream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4.Rotate());

        var titleFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var dataFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        // Titolo
        document.Add(new Paragraph("Statistiche Consumi Aziende")
            .SetFont(titleFont)
            .SetFontSize(16)
            .SetMarginBottom(10));

        // Tabella con 7 colonne
        var table = new Table(UnitValue.CreatePercentArray(7)).UseAllAvailableWidth();

        // Intestazioni
        var headers = new[]
        {
        "Azienda", "Codice", "N° Utenti", "QR Totali", "QR Utilizzati", "QR Rimanenti", "% Utilizzo"
    };

        foreach (var header in headers)
        {
            table.AddHeaderCell(new Cell().Add(new Paragraph(header)
                .SetFont(titleFont)
                .SetFontSize(9)));
        }

        // Dati
        foreach (var stat in statistics)
        {
            var companyName = stat.Company?.RagioneSociale ?? "";
            var companyCode = stat.Company?.Codice ?? "";
            var userCount = stat.Company?.Users?.Count ?? 0;
            var total = stat.TotalQRCodes;
            var used = stat.UsedQRCodes;
            var remaining = total - used;
            var percentage = total > 0 ? (double)used / total * 100 : 0;

            table.AddCell(new Cell().Add(new Paragraph(companyName).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(companyCode).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(userCount.ToString()).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(total.ToString()).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(used.ToString()).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph(remaining.ToString()).SetFont(dataFont).SetFontSize(8)));
            table.AddCell(new Cell().Add(new Paragraph($"{percentage:F1}%").SetFont(dataFont).SetFontSize(8)));
        }

        document.Add(table);
        document.Close();

        return await Task.FromResult(stream.ToArray());
    }

}
}
