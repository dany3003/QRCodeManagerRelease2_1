using Microsoft.AspNetCore.Mvc.Rendering;
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

        // Nuovi campi per filtro utenti
        public List<SelectListItem> GroupUsers { get; set; } = new();
        public string? SelectedUserId { get; set; }
        public List<QRCode> QRCodes { get; set; } = new();
        public int TotalQRCodes { get; set; }
        public int UsedQRCodes { get; set; }
        public int TotalCalls { get; set; }
        public decimal AverageCalls { get; set; }

    }

    public class QRCodeDetailViewModel
    {
        public QRCode QRCode { get; set; } = null!;
        public ApplicationUser CreatedBy { get; set; } = null!;
        public int TotalCalls { get; set; }
        public DateTime? LastCall { get; set; }
    }
}
