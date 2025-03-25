using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupParticipant : IDateCreated
{
    [Key]
    public required string GroupId { get; set; }
    
    [Key]
    public required string ParticipantId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset LastGroupMessage { get; set; }
    
    public Group Group { get; set; } = null!;
    
    public Participant Participant { get; set; } = null!;
}