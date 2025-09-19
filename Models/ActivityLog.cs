
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class ActivityLog
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("User")]
        public string? UserId { get; set; }
        
        [Required, StringLength(100)]
        public string Azione { get; set; } = string.Empty;
        
        [Required, StringLength(100)]
        public string Entita { get; set; } = string.Empty;
        
        public int? EntitaId { get; set; }
        
        [StringLength(1000)]
        public string? Dettagli { get; set; }
        
        [Required]
        public DateTime DataOperazione { get; set; } = DateTime.Now;
        
        [StringLength(45)]
        public string? IPAddress { get; set; }
        
        public virtual ApplicationUser? User { get; set; }
    }
}
