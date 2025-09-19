
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class Anomaly
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("User")]
        public string? UserId { get; set; }
        
        [Required, StringLength(100)]
        public string Tipo { get; set; } = string.Empty;
        
        [Required, StringLength(1000)]
        public string Descrizione { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? CodiceInteressato { get; set; }
        
        [Required]
        public DateTime DataSegnalazione { get; set; } = DateTime.Now;
        
        public bool Risolta { get; set; } = false;
        
        public DateTime? DataRisoluzione { get; set; }
        
        [ForeignKey("RisoltaDa")]
        public string? RisoltaDaUserId { get; set; }
        
        public virtual ApplicationUser? User { get; set; }
        public virtual ApplicationUser? RisoltaDa { get; set; }
    }
}
