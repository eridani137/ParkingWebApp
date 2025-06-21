using Microsoft.EntityFrameworkCore;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ParkingSpot> Место { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        
    }
}