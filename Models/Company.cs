
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(200)]
        public string RagioneSociale { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string Codice { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Via { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string? PartitaIva { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descrizione { get; set; }
        
        [StringLength(500)]
        public string? LogoPath { get; set; }

        public DateTime DataInserimento { get; set; } = DateTime.Now;

        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public virtual ICollection<CodeRange> CodeRanges { get; set; } = new List<CodeRange>();
    }
}
