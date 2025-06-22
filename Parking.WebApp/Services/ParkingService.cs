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
        if (spot is null || spot.номер_клиента is not null) return;

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var dbSpot = context.Место.FirstOrDefault(s => s.номер == spot.номер);
        if (dbSpot is null) return;

        dbSpot.номер_клиента = client.телефон;
        context.SaveChanges();

        spot.номер_клиента = client.телефон;
        NotifyStateChanged();
    }

    public void FreeSpot(int idx)
    {
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
        var spot = context.Место.FirstOrDefault(s => s.номер == idx);
        if (spot?.номер_клиента == null) return;

        spot.номер_клиента = null;
        context.SaveChanges();

        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}