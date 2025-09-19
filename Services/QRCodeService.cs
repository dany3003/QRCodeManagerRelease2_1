

using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Data;
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout;
using System.Threading.Tasks;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.IO.Font;

namespace QRCodeManagerRelease2.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly ApplicationDbContext _context;

        public QRCodeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<QRCode> CreateQRCodeAsync(QRCodeViewModel model, string userId)
        {
            var qrCode = new Models.QRCode
            {
                Content = model.Content,
                ExtractedCode = model.ExtractedCode,
                Details = model.Details,
                DestinationLink = model.DestinationLink,
                UsePassword = model.UsePassword,
                Password = model.UsePassword ? model.Password : null,
                AllowDownload = model.AllowDownload,
                UserId = userId,
                AvvisamiPrimaVolta = model.AvvisamiPrimaVolta,
                CodiceUtilizzo = model.CodiceUtilizzo
            };

            _context.QRCodes.Add(qrCode);
            await _context.SaveChangesAsync();
            return qrCode;
                       
        }

        public async Task<Models.QRCode?> GetQRCodeByFullCodeAsync(string content)
        {
            return await _context.QRCodes
                .Include(q => q.CreatedBy)
                .FirstOrDefaultAsync(q => q.ExtractedCode == content);
        }

        public async Task<Models.QRCode?> GetQRCodeByContentAsync(string content)
        {
            return await _context.QRCodes
                .Include(q => q.CreatedBy)
                .FirstOrDefaultAsync(q => q.ExtractedCode == content);
        }

        //public async Task<byte[]> GenerateQRCodeImageAsync(string content)
        //{
        //    using var qrGenerator = new QRCodeGenerator();
        //    using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
        //    using var qrCode = new PngByteQRCode(qrCodeData);
            
        //    return qrCode.GetGraphic(20);
        //}


        public async Task<byte[]> GenerateQRCodeImageAsync(string content)
        {
            // 1. Initialize the QR code generator
            var qrGenerator = new QRCodeGenerator();

            // 2. Create QR code data from the provided content
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            // 3. Create a PngByteQRCode object from the data.
            // This class is specifically designed to produce QR codes directly as PNG byte arrays.
            using (PngByteQRCode qrCode = new PngByteQRCode(qrCodeData))
            {
                // 4. Get the QR code as a PNG byte array.
                // The parameter '20' specifies the pixels per module (the size of each square in the QR code).
                // For PngByteQRCode, the correct method to get the PNG byte array is GetGraphic().
                byte[] qrCodeBytes = qrCode.GetGraphic(20);

                // 5. Return the byte array.
                return await Task.FromResult(qrCodeBytes);
            }
        }


public async Task<byte[]> GenerateQRCodePdfAsync(string content, string details, string codiceUtilizzo)
    {
        using var ms = new MemoryStream();

        var writer = new PdfWriter(ms);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf, PageSize.A4);

            // Aggiungi testo


            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA, PdfEncodings.WINANSI);

        document.Add(new Paragraph($"QR Code: {content}").SetFont(font));
        document.Add(new Paragraph($"Dettagli: {details}").SetFont(font));
        document.Add(new Paragraph($"Codice Utilizzo: {codiceUtilizzo}").SetFont(font));
          
        // Genera immagine QR code
        var qrCodeBytes = await GenerateQRCodeImageAsync(content);

        var imageData = ImageDataFactory.Create(qrCodeBytes);
        var qrCodeImage = new iText.Layout.Element.Image(imageData)
            .ScaleToFit(200f, 200f)
            .SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

        document.Add(qrCodeImage);

        document.Close();
        return ms.ToArray();
    }


}
}
