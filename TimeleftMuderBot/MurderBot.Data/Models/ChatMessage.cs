using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class ChatMessage
{
    [Key]
    public required string Id { get; set; }
    
    public required string WaId { get; set; }
    
    public required string Body { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset? SendAt { get; set; }
    
    public DateTimeOffset? DeliverAt { get; set; }
    
    public required string ChatId { get; set; }
    
    public string? ParticipantId { get; set; }
    
    /// <summary>
    /// If this is false, it is an incoming message
    /// </summary>
    public bool OutgoingMessage { get; set; }
}