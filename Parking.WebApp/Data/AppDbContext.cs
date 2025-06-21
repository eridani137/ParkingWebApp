using Microsoft.EntityFrameworkCore;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClientEntity> Клиент { get; set; }
    public DbSet<ParkingSpotEntity> Место { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientEntity>(entity =>
        {
            entity.HasKey(e => e.телефон);
            entity.HasIndex(e => e.телефон).IsUnique();
        });
        
        modelBuilder.Entity<ParkingSpotEntity>(entity =>
        {
            entity.HasKey(e => e.номер);
            entity.HasIndex(e => e.номер).IsUnique();
            entity.HasOne(e => e.ClientEntity)
                .WithOne()
                .HasForeignKey<ParkingSpotEntity>(e => e.номер_клиента)
                .HasPrincipalKey<ClientEntity>(c => c.телефон)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}