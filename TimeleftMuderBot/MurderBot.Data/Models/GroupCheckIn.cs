using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class GroupCheckIn
{
    [Key]
    public int GroupCheckinId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public required string GroupId { get; set; }
    
    public string? OutgoingMessageId { get; set; }
    
    public DateTimeOffset? MessageSent { get; set; }
    
    public DateTimeOffset? ParticipantsReadFinished { get; set; }
    
    public DateTimeOffset? ChatResponsesFinished { get; set; }
    
    public DateTimeOffset? RemovalsCompleted { get; set; }
    
    
}