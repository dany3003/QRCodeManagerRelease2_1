
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Il nome è obbligatorio")]
        [Display(Name = "Nome")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Il cognome è obbligatorio")]
        [Display(Name = "Cognome")]
        public string LastName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La password è obbligatoria")]
        [StringLength(100, ErrorMessage = "La password deve essere di almeno {2} caratteri", MinimumLength = 4)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        
        [DataType(DataType.Password)]
        [Display(Name = "Conferma password")]
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Display(Name = "È un'azienda")]
        public bool IsAzienda { get; set; }
        
        [Display(Name = "Nome Azienda")]
        public string? NomeAzienda { get; set; }
        
        [Required(ErrorMessage = "L'indirizzo è obbligatorio")]
        [Display(Name = "Via")]
        public string Via { get; set; } = string.Empty;
        
        [Display(Name = "Partita IVA")]
        public string? PartitaIva { get; set; }
        
        [Display(Name = "Telefono")]
        public string? Telefono { get; set; }
        
        [Display(Name = "Note")]
        public string? Note { get; set; }
    }
}
