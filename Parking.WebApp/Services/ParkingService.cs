using Parking.WebApp.Data;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Services;

public class ParkingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private bool _isInitialized = false;
    
    public List<ParkingSpotEntity> Spots { get; private set; } = new();
    public List<ClientEntity> Clients { get; private set; } = new();
    public event Action? OnChange;

    public ParkingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task EnsureInitializedAsync()
    {
        if (_isInitialized) return;

        await _initSemaphore.WaitAsync();
        try
        {
            if (_isInitialized) return;

            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
            
            Spots = await repository.GetAllSpotsAsync();
            Clients = await repository.GetAllClientsAsync();
            
            _isInitialized = true;
            NotifyStateChanged();
        }
        finally
        {
            _initSemaphore.Release();
        }
    }

    public async Task RefreshDataAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();
        
        Spots = await repository.GetAllSpotsAsync();
        Clients = await repository.GetAllClientsAsync();
        NotifyStateChanged();
    }

    public async Task TakeSpot(int idx, ClientEntity client)
    {
        var spot = Spots.ElementAtOrDefault(idx);
        if (spot is null || spot.номер_клиента is not null) return;

        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();

        var dbSpot = await repository.GetSpotByNumberAsync(spot.номер);
        if (dbSpot is null) return;

        await repository.UpdateSpotClientAsync(spot.номер, client.телефон);

        spot.номер_клиента = client.телефон;
        NotifyStateChanged();
    }

    public async Task FreeSpot(int spotNumber)
    {
        using var scope = _serviceProvider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();

        var localSpot = Spots.FirstOrDefault(s => s.номер == spotNumber);
        var dbSpot = await repository.GetSpotByNumberAsync(spotNumber);

        if (dbSpot?.номер_клиента == null) return;

        await repository.UpdateSpotClientAsync(spotNumber, null);

        if (localSpot != null)
        {
            localSpot.номер_клиента = null;
        }

        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}