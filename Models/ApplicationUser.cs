
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100)]
        public string? FirstName { get; set; }
        
        [StringLength(100)]
        public string? LastName { get; set; }
        
        [ForeignKey("CustomerGroup")]
        public string? CustomerGroupId { get; set; }
        
        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        
        // Nuovi campi per la registrazione
        public bool IsAzienda { get; set; }
        
        [StringLength(200)]
        public string? NomeAzienda { get; set; }
        
        [StringLength(300)]
        public string? Via { get; set; }
        
        [StringLength(16)]
        public string? PartitaIva { get; set; }
        
        [StringLength(20)]
        public string? Telefono { get; set; }
        
        [StringLength(1000)]
        public string? Note { get; set; }
        
        public bool Abilitato { get; set; } = false;
        
        public DateTime DataRegistrazione { get; set; } = DateTime.Now;
        
        public virtual CustomerGroup? CustomerGroup { get; set; }
        public virtual Company? Company { get; set; }
        public virtual ICollection<QRCode> QRCodes { get; set; } = new List<QRCode>();
        public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
        public virtual ICollection<Anomaly> Anomalies { get; set; } = new List<Anomaly>();
        public virtual ICollection<Anomaly> AnomaliesRisolte { get; set; } = new List<Anomaly>();
    }
}
