using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

/// <summary>
/// This table is to log when auto-reply messages are sent
/// so we can have some timeout period before they are sent again
/// </summary>
public class GroupAutoReplyMessage : IDateCreated
{
    private readonly ILazyLoader _lazyLoader;
    private GroupAutoReply _groupAutoReply = null!;
    private ChatMessage _outgoingMessage = null!;

    public GroupAutoReplyMessage()
    {
        
    }

    private GroupAutoReplyMessage(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    [Key]
    public int GroupAutoReplyMessageId { get; set; }
    
    public int GroupAutoReplyId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    [StringLength(30)]
    public string? OutgoingMessageId { get; set; }

    public GroupAutoReply GroupAutoReply
    {
        get => _lazyLoader.Load(this, ref _groupAutoReply!)!;
        set => _groupAutoReply = value;
    }

    public ChatMessage OutgoingMessage
    {
        get => _lazyLoader.Load(this, ref _outgoingMessage!)!;
        set => _outgoingMessage = value;
    }

    [Timestamp]
    public byte[] RowVersion { get; set; }
}