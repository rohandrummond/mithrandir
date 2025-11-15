using Microsoft.EntityFrameworkCore;
using mithrandir.Models;

namespace mithrandir.Data;

public class MithrandirDbContext(DbContextOptions<MithrandirDbContext> options) : DbContext(options)
{
  public DbSet<ApiKey> ApiKeys { get; set; } = null!;
  public DbSet<ApiUsage> ApiUsages { get; set; } = null!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Convert custom enum properties to strings
    modelBuilder.Entity<ApiKey>()
        .Property(e => e.Tier)
        .HasConversion<string>();

    modelBuilder.Entity<ApiKey>()
        .Property(e => e.Status)
        .HasConversion<string>();
    
    // Index for KeyHash (ApiKeys table)
    modelBuilder.Entity<ApiKey>()
        .HasIndex(k => k.KeyHash);
        
    // Composite index for ApKeyId and Timestamp (rate limiting) (ApiUsages table)
    modelBuilder.Entity<ApiUsage>()
        .HasIndex(u => new { u.ApiKeyId, u.Timestamp });
  }

}