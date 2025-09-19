
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;

namespace QRCodeManagerRelease2.Services
{
    public interface IQRCodeService
    {
        Task<QRCode> CreateQRCodeAsync(QRCodeViewModel model, string userId);
        Task<QRCode?> GetQRCodeByContentAsync(string content);
        Task<QRCode?> GetQRCodeByFullCodeAsync(string content);
        Task<byte[]> GenerateQRCodeImageAsync(string content);
        Task<byte[]> GenerateQRCodePdfAsync(string content, string details, string CodiceUtilizzo);
    }
}
