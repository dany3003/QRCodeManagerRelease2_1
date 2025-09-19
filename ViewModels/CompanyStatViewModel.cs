
using QRCodeManagerRelease2.Models;

namespace QRCodeManagerRelease2.ViewModels
{
    public class CompanyStatViewModel
    {
        public Company Company { get; set; } = null!;
        public int TotalQRCodes { get; set; }
        public int UsedQRCodes { get; set; }
        public int RemainingCodes => TotalQRCodes > 0 ? TotalQRCodes - UsedQRCodes : 0;
    }
}
