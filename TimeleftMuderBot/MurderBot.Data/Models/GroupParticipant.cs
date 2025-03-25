using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class GroupParticipant
{
    [Key]
    public required string GroupId { get; set; }
    
    [Key]
    public required string ParticipantId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset LastGroupMessage { get; set; }
}