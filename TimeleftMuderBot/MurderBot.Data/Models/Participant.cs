using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class Participant : IDateCreated
{
    [Key]
    public required string WId { get; set; }
    
    public required string Phone { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public string? FriendlyName { get; set; }
}