
using QRCodeManagerRelease2.Models;
using QRCodeManagerRelease2.ViewModels;

namespace QRCodeManagerRelease2.Services
{
    public interface IExportService
    {
        Task<byte[]> ExportUsersToExcel(IEnumerable<ApplicationUser> users);
        Task<byte[]> ExportUsersToPdf(IEnumerable<ApplicationUser> users);
        Task<byte[]> ExportCompaniesToExcel(IEnumerable<Company> companies);
        Task<byte[]> ExportCompaniesToPdf(IEnumerable<Company> companies);
        Task<byte[]> ExportQRCodesToExcel(IEnumerable<QRCode> qrCodes);
        Task<byte[]> ExportQRCodesToPdf(IEnumerable<QRCode> qrCodes);
        Task<byte[]> ExportActivityLogsToExcel(IEnumerable<ActivityLog> logs);
        Task<byte[]> ExportActivityLogsToPdf(IEnumerable<ActivityLog> logs);
        Task<byte[]> ExportCodeRangesToExcel(IEnumerable<CodeRange> ranges);
        Task<byte[]> ExportCodeRangesToPdf(IEnumerable<CodeRange> ranges);
        Task<byte[]> ExportAnomaliesToExcel(IEnumerable<Anomaly> anomalies);
        Task<byte[]> ExportAnomaliesToPdf(IEnumerable<Anomaly> anomalies);
        Task<byte[]> ExportCompanyStatisticsToExcel(IEnumerable<CompanyStatViewModel> statistics);
        Task<byte[]> ExportCompanyStatisticsToPdf(IEnumerable<CompanyStatViewModel> statistics);
    }
}
