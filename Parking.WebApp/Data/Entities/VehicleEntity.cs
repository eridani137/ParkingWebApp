namespace Parking.WebApp.Data.Entities;

public class VehicleEntity
{
    public string госномер { get; set; }
    public string телефон { get; set; }
    public string описание { get; set; }
    public ClientEntity? ClientEntity { get; set; }
}