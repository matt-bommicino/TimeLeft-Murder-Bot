using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class Participant : IDateCreated
{
    [Key]
    [StringLength(50)]
    public required string WId { get; set; }
    
    [StringLength(50)]
    public required string Phone { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    [StringLength(255)]
    public string? FriendlyName { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}