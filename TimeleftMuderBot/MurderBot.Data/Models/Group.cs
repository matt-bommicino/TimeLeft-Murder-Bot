using System.Collections;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class Group : IDateCreated, IDateModified
{
    private readonly ILazyLoader _lazyLoader;
    private ICollection<GroupParticipant> _groupParticipants = null!;
    private ICollection<GroupCheckIn> _groupCheckIns = null!;
    private ICollection<GroupAutoReply> _autoReplies = null!;

    public Group()
    {
        GroupCheckIns = new List<GroupCheckIn>();
        GroupParticipants = new List<GroupParticipant>();
        AutoReplies = new List<GroupAutoReply>();
    }

    private Group(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    [Key]
    [StringLength(50)]
    public required string WId { get; set; }
    
    public DateTimeOffset CreatedAt { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public DateTimeOffset DateModified { get; set; }
    
    public DateTimeOffset? LastParticipantSync {get; set;}
    
    [StringLength(255)]
    public required string Name { get; set; }
    
    public string? Description { get; set; }
    
    public bool IsBotAdmin { get; set; }
    
    public bool DoMurders { get; set; }
    
    public bool Ignore { get; set; }
    
    /// <summary>
    /// The amount of time to wait for participants to read
    /// the group check in message before messaging unread
    /// participants individually
    /// </summary>
    public TimeSpan CheckInReadTimeout { get; set; }
    
    /// <summary>
    /// The amount of time to wait for a response to the check in
    /// message before removing the participant
    /// </summary>
    public TimeSpan CheckInMessageResponseTimeout { get; set; }
    
    /// <summary>
    /// If the group received a message from this person within this amount of time
    /// they will not be removed
    /// </summary>
    public TimeSpan LastMessageExemptTime { get; set; }
    
    /// <summary>
    /// The minimum time before the bot starts the whole process
    /// over again since the last time it started
    /// </summary>
    public TimeSpan MinimumTimeBetweenRuns { get; set; }

    public ICollection<GroupParticipant> GroupParticipants
    {
        get => _lazyLoader.Load(this, ref _groupParticipants!)!;
        set => _groupParticipants = value;
    }

    public ICollection<GroupCheckIn> GroupCheckIns
    {
        get => _lazyLoader.Load(this, ref _groupCheckIns!)!;
        set => _groupCheckIns = value;
    }

    public ICollection<GroupAutoReply> AutoReplies
    {
        get => _lazyLoader.Load(this, ref _autoReplies!)!;
        set => _autoReplies = value;
    }

    [Timestamp]
    public byte[] RowVersion { get; set; }
    
}