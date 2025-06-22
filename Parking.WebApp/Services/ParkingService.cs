using Parking.WebApp.Data;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Services;

public class ParkingService : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SemaphoreSlim _initSemaphore = new(1, 1);
    private readonly Timer _refreshTimer;
    private bool _isInitialized;
    private bool _disposed;

    public List<ParkingSpotEntity> Spots { get; private set; } = [];
    public List<ClientEntity> Clients { get; private set; } = [];
    public event Action? OnChange;

    public ParkingService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        _refreshTimer = new Timer(async void (_) =>
            {
                try
                {
                    await RefreshDataIfInitialized();
                }
                catch
                {
                    // ignored
                }
            },
            null,
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(5)
        );
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

    private async Task RefreshDataAsync()
    {
        if (_disposed) return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<ParkingRepository>();

            Spots = await repository.GetAllSpotsAsync();
            Clients = await repository.GetAllClientsAsync();
            NotifyStateChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении данных: {ex.Message}");
        }
    }

    private async Task RefreshDataIfInitialized()
    {
        if (_isInitialized && !_disposed)
        {
            await RefreshDataAsync();
        }
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

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _refreshTimer.Dispose();
        _initSemaphore.Dispose();
        GC.SuppressFinalize(this);
    }
}