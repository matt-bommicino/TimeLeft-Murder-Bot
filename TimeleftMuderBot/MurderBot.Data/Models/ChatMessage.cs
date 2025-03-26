using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class ChatMessage : IDateCreated
{
    [Key]
    [StringLength(30)]
    public required string Id { get; set; }
    
    [StringLength(30)]
    public required string WaId { get; set; }
    
    public required string Body { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset? SendAt { get; set; }
    
    public DateTimeOffset? DeliverAt { get; set; }
    
    [StringLength(30)]
    public required string ChatId { get; set; }
    
    [StringLength(30)]
    public string? ParticipantId { get; set; }
    
    /// <summary>
    /// If this is false, it is an incoming message
    /// </summary>
    public bool OutgoingMessage { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}