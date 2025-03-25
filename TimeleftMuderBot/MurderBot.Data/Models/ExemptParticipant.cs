using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

/// <summary>
/// Participants in this table will never be removed or messaged for removal
/// </summary>
public class ExemptParticipant
{
    [Key]
    public int ExemptParticipantId { get; set; }
    
    public required string ParticipantId { get; set; }
    
    public string? GroupId { get; set; }
}