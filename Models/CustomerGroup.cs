
using System.ComponentModel.DataAnnotations;

namespace QRCodeManagerRelease2.Models
{
    public class CustomerGroup
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500)]
        public string? Description { get; set; }
        
        public virtual ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
