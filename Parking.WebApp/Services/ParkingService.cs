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
        if (idx < 0 || idx >= Spots.Count) return;
        if (Spots[idx].номер_клиента != null) return;

        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var spotInDb = context.Место.Find(Spots[idx].номер);
        if (spotInDb is not null)
        {
            spotInDb.номер_клиента = client.телефон;
            context.SaveChanges();
        }
        
        Spots[idx].номер_клиента = client.телефон;
        
        NotifyStateChanged();
    }

    public void FreeSpot(int idx)
    {
        var spot = Spots.FirstOrDefault(s => s.номер == idx);
        if (spot?.номер_клиента is null) return;
        
        using var scope = _provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var spotInDb = context.Место.Find(idx);
        if (spotInDb != null)
        {
            spotInDb.номер_клиента = null;
            context.SaveChanges();
        }
        
        spot.номер_клиента = null;
        
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}