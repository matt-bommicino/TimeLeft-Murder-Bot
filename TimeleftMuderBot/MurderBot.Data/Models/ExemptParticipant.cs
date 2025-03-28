using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

/// <summary>
/// Participants in this table will never be removed or messaged for removal
/// </summary>
public class ExemptParticipant
{
    [Key]
    public int ExemptParticipantId { get; set; }
    
    [StringLength(50)]
    public required string ParticipantId { get; set; }
    
    [StringLength(50)]
    public string? GroupId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}