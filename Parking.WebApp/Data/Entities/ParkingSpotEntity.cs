using System.ComponentModel.DataAnnotations;

namespace Parking.WebApp.Data.Entities;

public class ParkingSpotEntity
{
    public int номер { get; set; }
    [MaxLength(255)] public string? расположение { get; set; }
    [MaxLength(32)] public string? номер_клиента { get; set; }
    public ClientEntity? ClientEntity { get; set; }
}