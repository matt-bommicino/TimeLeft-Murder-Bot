using System.Collections;
using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class Group : IDateCreated, IDateModified
{
    [Key]
    public required string WId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsBotAdmin { get; set; }
    
    public bool PerformPruning { get; set; }
    
    /// <summary>
    /// The amount of time to wait for participants to read
    /// the group check in message before messaging unread
    /// participants individually
    /// </summary>
    public TimeSpan CheckInReadTimeout { get; set; }
    
    /// <summary>
    /// The amount of time to wait for a response to the check in
    /// message before removing the participant
    /// </summary>
    public TimeSpan CheckInMessageResponseTimeout { get; set; }
    
    /// <summary>
    /// If the group received a message from this person within this amount of time
    /// they will not be removed
    /// </summary>
    public TimeSpan LastMessageExemptTime { get; set; }
    
    public ICollection<GroupParticipant> GroupParticipants { get; set; } = null!; 
    
    public ICollection<GroupCheckIn> GroupCheckIns { get; set; } = null!;
    public ICollection<GroupAutoReply> AutoReplies { get; set; } = null!; 
    
}