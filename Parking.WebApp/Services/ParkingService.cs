using Microsoft.EntityFrameworkCore;
using Parking.WebApp.Data;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Services;

public class ParkingService
{
    private readonly IServiceProvider _provider;
    public List<ParkingSpotEntity> Spots { get; }
    public List<ClientEntity> Clients { get; }
    public event Action? OnChange;

    public ParkingService(IServiceProvider provider)
    {
        _provider = provider;
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Spots = context.Место.ToList();
        Clients = context.Клиент.ToList();
    }
    
    public async Task TakeSpot(int idx, ClientEntity client)
    {
        var spot = Spots.ElementAtOrDefault(idx);
        if (spot is null || spot.номер_клиента is not null) return;

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbSpot = await context.Место.FirstOrDefaultAsync(s => s.номер == spot.номер);
        if (dbSpot is null) return;
        
        

        dbSpot.номер_клиента = client.телефон;
        await context.SaveChangesAsync();

        spot.номер_клиента = client.телефон;
        NotifyStateChanged();
    }

    
    public async Task FreeSpot(int spotNumber)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var localSpot = Spots.FirstOrDefault(s => s.номер == spotNumber);

        var dbSpot = await context.Место.FirstOrDefaultAsync(s => s.номер == spotNumber);
        if (dbSpot?.номер_клиента == null) return;

        dbSpot.номер_клиента = null;
        await context.SaveChangesAsync();

        if (localSpot != null)
        {
            localSpot.номер_клиента = null;
        }
    
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}