
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserire un indirizzo email valido")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La password è obbligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
        
        [Display(Name = "Ricordami")]
        public bool RememberMe { get; set; }
        
        public string? ReturnUrl { get; set; }
    }
}
