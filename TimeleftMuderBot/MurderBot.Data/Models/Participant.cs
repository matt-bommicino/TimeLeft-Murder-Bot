using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class Participant
{
    [Key]
    public required string WId { get; set; }
    
    public required string Phone { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public string? FriendlyName { get; set; }
}