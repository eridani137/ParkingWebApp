using System.ComponentModel.DataAnnotations;

namespace Parking.WebApp.Data.Entities;

public class ClientEntity
{
    [MaxLength(32)] public string телефон { get; set; }
    [MaxLength(64)] public string фамилия { get; set; }
    [MaxLength(64)] public string имя { get; set; }
    [MaxLength(64)] public string отчество { get; set; }
}