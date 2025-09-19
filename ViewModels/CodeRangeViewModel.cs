
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.ViewModels
{
    public class CodeRangeViewModel
    {
        [Required(ErrorMessage = "Il codice iniziale è obbligatorio")]
        [Display(Name = "Codice Iniziale")]
        public string CodiceIniziale { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Il codice finale è obbligatorio")]
        [Display(Name = "Codice Finale")]
        public string CodiceFinale { get; set; } = string.Empty;
        
        [Display(Name = "Azienda")]
        public int? CompanyId { get; set; }
    }
}
