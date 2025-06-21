using Microsoft.EntityFrameworkCore;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ClientEntity> Клиент { get; set; }
    public DbSet<ParkingSpotEntity> Место { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientEntity>()
            .HasKey(e => e.телефон);
        
        modelBuilder.Entity<ParkingSpotEntity>()
            .HasKey(e => e.номер);
    
        modelBuilder.Entity<ClientEntity>()
            .HasOne(c => c.Место)
            .WithOne(p => p.ClientEntity)
            .HasForeignKey<ParkingSpotEntity>(p => p.номер_клиента)
            .HasPrincipalKey<ClientEntity>(c => c.телефон);
        
        modelBuilder.Entity<ParkingSpotEntity>()
            .HasIndex(p => p.номер_клиента)
            .IsUnique();
    }
}