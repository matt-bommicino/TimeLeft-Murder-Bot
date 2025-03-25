using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public enum CheckInMethod
{
    NotSet = 0,
    Exempt = 1,
    ReadCheckInMessage = 2,
    RecentGroupMessage = 3,
    RepliedToCheckInMessage = 4,
}


public class GroupCheckInParticipantCheckIn : IDateCreated, IDateModified
{
    [Key]
    public int GroupCheckinId { get; set; }
    
    [Key]
    public required string ParticipantId { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public DateTimeOffset? MessageSentTime { get; set; }
    
    public string? CheckInMessageId { get; set; }
    
    public string? RemovalMessageId { get; set; }
    
    public Guid? AutoReaddTokenId {get; set;}
    
    public DateTimeOffset? MessageReceivedTime { get; set; }
    
    public string? IncomingMessageId { get; set; }
    
    public DateTimeOffset? CheckInSuccess { get; set; }
    
    public CheckInMethod CheckInMethod { get; set; }
    
    public DateTimeOffset? CheckInFailedTime { get; set; }
    
}