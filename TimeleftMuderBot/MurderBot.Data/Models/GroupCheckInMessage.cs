using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupCheckInMessage : IDateCreated
{
    private readonly ILazyLoader _lazyLoader;

    public GroupCheckInMessage()
    {
        
    }

    private GroupCheckInMessage(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    private ChatMessage _outgoingMessage = null!;
    private GroupCheckIn _groupCheckIn = null!;

    [Key]
    public int GroupCheckinMessageId { get; set; }
    
    public int GroupCheckinId { get; set; }
    
    [StringLength(50)]
    public string? OutgoingMessageId { get; set; }

    public GroupCheckIn GroupCheckIn
    {
        get => _lazyLoader.Load(this, ref _groupCheckIn!)!;
        set => _groupCheckIn = value;
    }

    public ChatMessage OutgoingMessage
    {
        get => _lazyLoader.Load(this, ref _outgoingMessage!)!;
        set => _outgoingMessage = value;
    }
    
    public DateTimeOffset DateCreated { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}