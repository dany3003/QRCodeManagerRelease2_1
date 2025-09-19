
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class QRCode
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(200)]
        public string Content { get; set; } = string.Empty;
        
        [StringLength(200)]
        public string? ExtractedCode { get; set; }
        
        [StringLength(500)]
        public string? Details { get; set; }
        
        [StringLength(1000)]
        public string? DestinationLink { get; set; }
        
        public bool UsePassword { get; set; }
        
        [StringLength(100)]
        public string? Password { get; set; }
        
        public bool AllowDownload { get; set; }
        
        [ForeignKey("CreatedBy")]
        public string? UserId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Campi esistenti
        public int NumeroChiamate { get; set; } = 0;
        
        public DateTime? UltimaChiamata { get; set; }
        
        public bool Bloccato { get; set; } = false;
        
        public bool AvvisamiPrimaVolta { get; set; } = false;
        
        public bool NotificatoPrimaVolta { get; set; } = false;
        
        [StringLength(100)]
        public string? CodiceUtilizzo { get; set; }
        
        // Nuovi campi per notifica destinatario
        public bool AvvisaDestinatario { get; set; } = false;
        
        [StringLength(255)]
        public string? EmailDestinatario { get; set; }
        
        [StringLength(1000)]
        public string? MessaggioDestinatario { get; set; }
        
        public virtual ApplicationUser? CreatedBy { get; set; }
        public virtual ICollection<QRCodeCallHistory> CallHistory { get; set; } = new List<QRCodeCallHistory>();
    }
}
