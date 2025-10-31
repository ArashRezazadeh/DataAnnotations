

using Microsoft.EntityFrameworkCore;

namespace DataAnnotations.Data;



public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<EventRegistration> EventRegistrations { get; set; } 
}