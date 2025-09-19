
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.ViewModels
{
    public class QRCodeAccessViewModel
    {
        public string QRCodeContent { get; set; } = string.Empty;

        public string ExtractedCode { get; set; } = string.Empty;

        [Display(Name = "Password")]
        public string? Password { get; set; }
        
        public bool RequiresPassword { get; set; }
        
        public bool AllowsDownload { get; set; }
        
        public string? DestinationLink { get; set; }
        
        public string? Details { get; set; }
    }
}
