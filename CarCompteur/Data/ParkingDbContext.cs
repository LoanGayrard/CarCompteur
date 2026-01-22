using CarCompteur.Domain;
using Microsoft.EntityFrameworkCore;

namespace CarCompteur.Data;

public class ParkingDbContext(DbContextOptions<ParkingDbContext> options) : DbContext(options)
{
    public DbSet<ParkingEvent> ParkingEvents => Set<ParkingEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ParkingEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TimestampUtc)
                  .IsRequired();

            // Stocker l'enum en int (simple, stable)
            entity.Property(e => e.Type)
                  .HasConversion<int>()
                  .IsRequired();

            entity.HasIndex(e => e.TimestampUtc);
            entity.HasIndex(e => new { e.TimestampUtc, e.Type });
        });
    }
}
