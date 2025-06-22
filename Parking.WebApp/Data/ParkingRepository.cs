using System.Data;
using Dapper;
using Parking.WebApp.Data.Entities;

namespace Parking.WebApp.Data;

public class ParkingRepository(IDbConnection connection)
{
    private const string ClientsTable = "Клиент";
    private const string ParkingSpotsTable = "Место";
    private const string VehiclesTable = "Автомобиль";

    public async Task<List<ParkingSpotEntity>> GetAllSpotsAsync()
    {
        const string sql = $"""

                                        SELECT 
                                            номер,
                                            расположение,
                                            номер_клиента
                                        FROM {ParkingSpotsTable}
                            """;

        var spots = await connection.QueryAsync<ParkingSpotEntity>(sql);
        return spots.ToList();
    }

    public async Task<List<ClientEntity>> GetAllClientsAsync()
    {
        const string sql = $"""

                                        SELECT 
                                            c.телефон,
                                            c.фамилия,
                                            c.имя,
                                            c.отчество,
                                            ps.номер as ParkingSpotNumber,
                                            ps.расположение as ParkingSpotLocation,
                                            v.госномер as VehicleNumber,
                                            v.описание as VehicleDescription
                                        FROM {ClientsTable} c
                                        LEFT JOIN {ParkingSpotsTable} ps ON c.телефон = ps.номер_клиента
                                        LEFT JOIN {VehiclesTable} v ON c.телефон = v.телефон
                            """;

        var clientDict = new Dictionary<string, ClientEntity>();

        await connection.QueryAsync<ClientEntity, ParkingSpotEntity, VehicleEntity, ClientEntity>(
            sql,
            (client, parkingSpot, vehicle) =>
            {
                if (!clientDict.TryGetValue(client.телефон, out var existingClient))
                {
                    existingClient = client;
                    clientDict.Add(client.телефон, existingClient);
                }

                if (parkingSpot != null && parkingSpot.номер != 0)
                {
                    existingClient.ParkingSpot = parkingSpot;
                }

                if (vehicle != null && !string.IsNullOrEmpty(vehicle.госномер))
                {
                    existingClient.VehicleEntity = vehicle;
                }

                return existingClient;
            },
            splitOn: "ParkingSpotNumber,VehicleNumber"
        );

        return clientDict.Values.ToList();
    }

    public async Task<ParkingSpotEntity?> GetSpotByNumberAsync(int spotNumber)
    {
        const string sql = $"""

                                        SELECT 
                                            номер,
                                            расположение,
                                            номер_клиента
                                        FROM {ParkingSpotsTable} 
                                        WHERE номер = @SpotNumber
                            """;

        return await connection.QueryFirstOrDefaultAsync<ParkingSpotEntity>(sql, new { SpotNumber = spotNumber });
    }

    public async Task UpdateSpotClientAsync(int spotNumber, string? clientPhone)
    {
        const string sql = $"""

                                        UPDATE {ParkingSpotsTable} 
                                        SET номер_клиента = @ClientPhone 
                                        WHERE номер = @SpotNumber
                            """;

        await connection.ExecuteAsync(sql, new { ClientPhone = clientPhone, SpotNumber = spotNumber });
    }

    public async Task<ClientEntity?> GetClientByPhoneAsync(string phone)
    {
        const string sql = $"""

                                        SELECT 
                                            телефон,
                                            фамилия,
                                            имя,
                                            отчество
                                        FROM {ClientsTable} 
                                        WHERE телефон = @Phone
                            """;

        return await connection.QueryFirstOrDefaultAsync<ClientEntity>(sql, new { Phone = phone });
    }

    public async Task<VehicleEntity?> GetVehicleByPhoneAsync(string phone)
    {
        const string sql = $"""

                                        SELECT 
                                            госномер,
                                            телефон,
                                            описание
                                        FROM {VehiclesTable} 
                                        WHERE телефон = @Phone
                            """;

        return await connection.QueryFirstOrDefaultAsync<VehicleEntity>(sql, new { Phone = phone });
    }
}