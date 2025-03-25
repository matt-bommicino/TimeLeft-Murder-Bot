using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class Group
{
    [Key]
    public required string WId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
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
    
}