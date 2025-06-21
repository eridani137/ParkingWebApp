using System.ComponentModel.DataAnnotations;

namespace Parking.WebApp.Data.Entities;

public class ParkingSpot
{
    [Key] public int номер { get; set; }
    [MaxLength(255)] public string? расположение { get; set; }
}