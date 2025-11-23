

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataAnnotations.Data;



public class AppDbContext: IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<EventRegistration> EventRegistrations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<EventRegistration>(entity =>
        {
            entity.OwnsOne(e => e.AdditionalContact, ac =>
            {
                ac.Property(a => a.PhoneNumber).HasColumnName("PhoneNumber");
                ac.Property(a => a.Address).HasColumnName("Address");
            });
        });
    }
}