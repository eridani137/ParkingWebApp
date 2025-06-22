using Microsoft.EntityFrameworkCore;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClientEntity> Клиент { get; set; }
    public DbSet<ParkingSpotEntity> Место { get; set; }
    public DbSet<VehicleEntity> Автомобиль { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientEntity>(entity =>
        {
            entity.HasKey(e => e.телефон);
            
            entity.HasOne(c => c.ParkingSpot)
                .WithOne(p => p.ClientEntity)
                .HasForeignKey<ParkingSpotEntity>(p => p.номер_клиента)
                .HasPrincipalKey<ClientEntity>(c => c.телефон);

            entity.HasOne(c => c.VehicleEntity)
                .WithOne(p => p.ClientEntity)
                .HasForeignKey<VehicleEntity>(v => v.телефон)
                .HasPrincipalKey<ClientEntity>(c => c.телефон);
        });

        modelBuilder.Entity<ParkingSpotEntity>(entity =>
        {
            entity.HasKey(e => e.номер);
            
            entity.HasIndex(p => p.номер_клиента).IsUnique();
        });

        modelBuilder.Entity<VehicleEntity>(entity =>
        {
            entity.HasKey(e => e.госномер);
            
            entity.HasIndex(p => p.телефон).IsUnique();
        });
    }
}