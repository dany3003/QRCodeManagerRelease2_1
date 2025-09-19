
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QRCodeManagerRelease2.Models;

namespace QRCodeManagerRelease2.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<CustomerGroup> CustomerGroups { get; set; }
        public DbSet<QRCode> QRCodes { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CodeRange> CodeRanges { get; set; }
        public DbSet<QRCodeCallHistory> QRCodeCallHistories { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<Anomaly> Anomalies { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.CustomerGroup)
                .WithMany(g => g.Users)
                .HasForeignKey(u => u.CustomerGroupId);
                
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId);
                
            builder.Entity<QRCode>()
                .HasOne(q => q.CreatedBy)
                .WithMany(u => u.QRCodes)
                .HasForeignKey(q => q.UserId);
                
            builder.Entity<QRCodeCallHistory>()
                .HasOne(h => h.QRCode)
                .WithMany(q => q.CallHistory)
                .HasForeignKey(h => h.QRCodeId);
                
            builder.Entity<CodeRange>()
                .HasOne(cr => cr.Company)
                .WithMany(c => c.CodeRanges)
                .HasForeignKey(cr => cr.CompanyId);
                
            builder.Entity<ActivityLog>()
                .HasOne(al => al.User)
                .WithMany(u => u.ActivityLogs)
                .HasForeignKey(al => al.UserId);
                
            builder.Entity<Anomaly>()
                .HasOne(a => a.User)
                .WithMany(u => u.Anomalies)
                .HasForeignKey(a => a.UserId);
                
            builder.Entity<Anomaly>()
                .HasOne(a => a.RisoltaDa)
                .WithMany(u => u.AnomaliesRisolte)
                .HasForeignKey(a => a.RisoltaDaUserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
