
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QRCodeManagerRelease2.ViewModels
{
    public class CompanyViewModel
    {
        [Required(ErrorMessage = "La ragione sociale è obbligatoria")]
        [Display(Name = "Ragione Sociale")]
        public string RagioneSociale { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Il codice è obbligatorio")]
        [Display(Name = "Codice")]
        public string Codice { get; set; } = string.Empty;
        
        [Display(Name = "Descrizione")]
        public string? Descrizione { get; set; }

        [Display(Name = "Partita Iva")]
        public string? PartitaIva { get; set; }

        [Display(Name = "Via")]
        public string? Via { get; set; }
        
        [Display(Name = "Logo")]
        public IFormFile? Logo { get; set; }
        
        public string? LogoPath { get; set; }
    }
}
