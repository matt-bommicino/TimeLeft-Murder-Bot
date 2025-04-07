using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupCheckIn : IDateCreated, IDateModified
{
    private readonly ILazyLoader _lazyLoader;

    public GroupCheckIn()
    {
        Messages = new List<GroupCheckInMessage>();
        ParticipantsCheckIns = new List<GroupCheckInParticipantCheckIn>();
    }

    private GroupCheckIn(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }

    private ICollection<GroupCheckInMessage> _messages;
    private Group _group = null!;
    private ICollection<GroupCheckInParticipantCheckIn> _participantsCheckIns;

    [Key]
    public int GroupCheckinId { get; set; }
    
    /// <summary>
    /// A guid used to access the summary via the website
    /// </summary>
    public Guid UrlGuid { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    [StringLength(50)]
    public required string GroupId { get; set; }
    
    public DateTimeOffset? FirstMessageSent { get; set; }
    
    public DateTimeOffset? ParticipantsReadFinished { get; set; }
    
    public DateTimeOffset? ChatResponsesFinished { get; set; }
    
    public DateTimeOffset? RemovalsCompleted { get; set; }
    
    public DateTimeOffset? LastReadCountCompleted { get; set; }
    
    public int? LastReadCount { get; set; }
    
    public int ChatMessageSendStageAttempts { get; set; }
    
    public int RemovalStageAttempts { get; set; }

    public Group Group
    {
        get => _lazyLoader.Load(this, ref _group!)!;
        set => _group = value;
    }

    public ICollection<GroupCheckInMessage> Messages
    {
        get => _lazyLoader.Load(this, ref _messages!)!;
        set => _messages = value;
    }

    public ICollection<GroupCheckInParticipantCheckIn> ParticipantsCheckIns
    {
        get => _lazyLoader.Load(this, ref _participantsCheckIns!)!;
        set => _participantsCheckIns = value;
    }

    [Timestamp]
    public byte[] RowVersion { get; set; }
    
}