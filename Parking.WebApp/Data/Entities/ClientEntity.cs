namespace Parking.WebApp.Data.Entities;

public class ClientEntity
{
    public string телефон { get; set; }
    public string фамилия { get; set; }
    public string имя { get; set; }
    public string отчество { get; set; }
    public ParkingSpotEntity? ParkingSpot { get; set; }
    public VehicleEntity? VehicleEntity { get; set; }
}