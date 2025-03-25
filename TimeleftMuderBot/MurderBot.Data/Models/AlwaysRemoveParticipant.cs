using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

/// <summary>
/// Participants in this table will always be removed when encountered
/// </summary>
public class AlwaysRemoveParticipant : IDateCreated
{
    [Key]
    public int AlwaysRemoveParticipantId { get; set; }
    
    public required string ParticipantId { get; set; }
    
    public string? GroupId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
}