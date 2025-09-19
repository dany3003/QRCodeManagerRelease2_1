
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.ViewModels
{
    public class QRCodeViewModel
    {
        [Required(ErrorMessage = "Il contenuto QR è obbligatorio")]
        [Display(Name = "Contenuto QR")]
        public string Content { get; set; } = string.Empty;
        
        [Display(Name = "Codice Completo")]
        public string? ExtractedCode { get; set; }
        
        [Display(Name = "Dettagli")]
        public string? Details { get; set; }
        
        [Display(Name = "Link di destinazione")]
        [Url(ErrorMessage = "Inserire un URL valido")]
        public string? DestinationLink { get; set; }
        
        [Display(Name = "Usa Password")]
        public bool UsePassword { get; set; }
        
        [Display(Name = "Password")]
        public string? Password { get; set; }
        
        [Display(Name = "Permetti Download")]
        public bool AllowDownload { get; set; }
        
        [Display(Name = "Avvisami quando aperto la prima volta")]
        public bool AvvisamiPrimaVolta { get; set; }
        
        [Display(Name = "Codice Utilizzo")]
        public string? CodiceUtilizzo { get; set; }
        
        [Display(Name = "Avvisa Destinatario")]
        public bool AvvisaDestinatario { get; set; }
        
        [Display(Name = "Email Destinatario")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        public string? EmailDestinatario { get; set; }
        
        [Display(Name = "Messaggio")]
        public string? MessaggioDestinatario { get; set; } = "Ho appena creato il sigillo QRCode...";
    }
}
