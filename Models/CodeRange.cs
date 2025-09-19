
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QRCodeManagerRelease2.Models
{
    public class CodeRange
    {
        [Key]
        public int Id { get; set; }
        
        [Required, StringLength(50)]
        public string CodiceIniziale { get; set; } = string.Empty;
        
        [Required, StringLength(50)]
        public string CodiceFinale { get; set; } = string.Empty;
        
        [Required]
        public DateTime DataInserimento { get; set; } = DateTime.Now;
        
        public DateTime? DataModifica { get; set; }
        
        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        
        public virtual Company? Company { get; set; }
    }
}
