using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupCheckIn : IDateCreated, IDateModified
{
    [Key]
    public int GroupCheckinId { get; set; }
    
    /// <summary>
    /// A guid used to access the summary via the website
    /// </summary>
    public Guid UrlGuid { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public required string GroupId { get; set; }
    
    public string? OutgoingMessageId { get; set; }
    
    public DateTimeOffset? MessageSent { get; set; }
    
    public DateTimeOffset? ParticipantsReadFinished { get; set; }
    
    public DateTimeOffset? ChatResponsesFinished { get; set; }
    
    public DateTimeOffset? RemovalsCompleted { get; set; }
    
    public Group Group { get; set; } = null!;
    
    
}