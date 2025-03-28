using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public enum CheckInMethod
{
    NotSet = 0,
    Exempt = 1,
    ReadCheckInMessage = 2,
    RecentGroupMessage = 3,
    RepliedToCheckInMessage = 4,
    ParticipantRemoved = 5,
}


[PrimaryKey(nameof(GroupCheckinId), nameof(ParticipantId))]
public class GroupCheckInParticipantCheckIn : IDateCreated, IDateModified
{
    private readonly ILazyLoader _lazyLoader;
    private GroupCheckIn _groupCheckIn = null!;
    private ChatMessage _checkInMessage = null!;
    private ChatMessage _removalMessage = null!;
    private ChatMessage _incomingMessage = null!;

    public GroupCheckInParticipantCheckIn()
    {
        
    }

    private GroupCheckInParticipantCheckIn(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    [StringLength(50)]
    public int GroupCheckinId { get; set; }
    
    [StringLength(50)]
    public required string ParticipantId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public DateTimeOffset? MessageSentTime { get; set; }
    
    [StringLength(50)]
    public string? CheckInMessageId { get; set; }
    
    [StringLength(50)]
    public string? RemovalMessageId { get; set; }
    
    public Guid? AutoReAddTokenId {get; set;}
    
    public DateTimeOffset? MessageReceivedTime { get; set; }
    
    [StringLength(50)]
    public string? IncomingMessageId { get; set; }
    
    public DateTimeOffset? CheckInSuccess { get; set; }
    
    public CheckInMethod CheckInMethod { get; set; }
    
    public DateTimeOffset? RemovalTime { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; }


    public ChatMessage CheckInMessage
    {
        get => _lazyLoader.Load(this, ref _checkInMessage!)!;
        set => _checkInMessage = value;
    }

    public ChatMessage RemovalMessage
    {
        get => _lazyLoader.Load(this, ref _removalMessage!)!;
        set => _removalMessage = value;
    }

    public ChatMessage IncomingMessage
    {
        get => _lazyLoader.Load(this, ref _incomingMessage!)!;
        set => _incomingMessage = value;
    }


    public GroupCheckIn GroupCheckIn
    {
        get => _lazyLoader.Load(this, ref _groupCheckIn!)!;
        set => _groupCheckIn  = value;
    }
}