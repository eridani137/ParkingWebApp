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
    
    public void TakeSpot(int idx, ClientEntity client)
    {
        var spot = Spots.ElementAtOrDefault(idx);
        if (spot is null) return;
        if (spot.номер_клиента is not null) return;

        spot.номер_клиента = client.телефон;
        
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Место.Attach(Spots[idx]);
        context.Entry(Spots[idx]).State = EntityState.Modified;
        context.SaveChanges();

        NotifyStateChanged();
    }

    public void FreeSpot(int idx)
    {
        var spot = Spots.FirstOrDefault(s => s.номер == idx);
        if (spot?.номер_клиента == null) return;
        
        spot.номер_клиента = null;
        
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Место.Attach(spot);
        context.Entry(spot).State = EntityState.Modified;
        context.SaveChanges();
        
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}