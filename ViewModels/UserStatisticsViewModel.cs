
using QRCodeManagerRelease2.Models;

namespace QRCodeManagerRelease2.ViewModels
{
    public class UserStatisticsViewModel
    {
        public ApplicationUser User { get; set; } = null!;
        public Company? Company { get; set; }
        public int PersonalQRCodes { get; set; }
        public int PersonalUsedQRCodes { get; set; }
        public int CompanyQRCodes { get; set; }
        public int CompanyUsedQRCodes { get; set; }
        public List<QRCodeDetailViewModel> QRCodeDetails { get; set; } = new();
    }

    public class QRCodeDetailViewModel
    {
        public QRCode QRCode { get; set; } = null!;
        public ApplicationUser CreatedBy { get; set; } = null!;
        public int TotalCalls { get; set; }
        public DateTime? LastCall { get; set; }
    }
}
