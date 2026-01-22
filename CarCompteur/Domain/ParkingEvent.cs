using System.ComponentModel.DataAnnotations;

namespace CarCompteur.Domain;

public class ParkingEvent
{
    public int Id { get; set; }

    [Required]
    public DateTime TimestampUtc { get; set; }

    [Required]
    public ParkingEventType Type { get; set; }
}
