using System.ComponentModel.DataAnnotations.Schema;

namespace Parking.WebApp.Data.Entities;

public class ParkingSpotEntity
{
    public int номер { get; set; }
    public string? расположение { get; set; }
    public string? номер_клиента { get; set; }
    public ClientEntity? ClientEntity { get; set; }
}