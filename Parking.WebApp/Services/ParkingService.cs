using Parking.WebApp.Data;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Services;

public class ParkingService
{
    public List<ParkingSpotEntity> Spots { get; }
    public List<ClientEntity> Clients { get; }
    public event Action? OnChange;

    public ParkingService(IServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Spots = context.Место.ToList();
        Clients = context.Клиент.ToList();
    }
    
    public void ToggleSpot(int idx)
    {
        if (idx < 0 || idx >= Spots.Count) return;
        // Spots[idx].IsOccupied = !Spots[idx].IsOccupied;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}