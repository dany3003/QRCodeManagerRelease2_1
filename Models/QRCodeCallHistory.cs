
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class QRCodeCallHistory
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("QRCode")]
        public int QRCodeId { get; set; }
        
        [Required]
        public DateTime DataChiamata { get; set; } = DateTime.Now;
        
        [StringLength(45)]
        public string? IPAddress { get; set; }
        
        [StringLength(200)]
        public string? Location { get; set; }
        
        [StringLength(500)]
        public string? UserAgent { get; set; }
        
        public virtual QRCode QRCode { get; set; } = null!;
    }
}
