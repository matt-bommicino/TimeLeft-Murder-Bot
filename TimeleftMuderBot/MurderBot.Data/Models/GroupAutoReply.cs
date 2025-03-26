using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupAutoReply : IDateCreated
{
    private readonly ILazyLoader _lazyLoader;
    private Group _group = null!;

    public GroupAutoReply()
    {
        
    }

    private GroupAutoReply(ILazyLoader lazyLoader)
    {
        _lazyLoader = lazyLoader;
    }
    
    [Key]
    public int GroupAutoReplyId { get; set; }
    
    public required string TriggerRegEx { get; set; }
    
    
    public required string ReplyMessage { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    [StringLength(30)]
    public string? GroupId { get; set; }

    public Group Group
    {
        get => _lazyLoader.Load(this, ref _group!)!;
        set => _group = value;
    }

    public ICollection<GroupAutoReplyMessage> Messages { get; set; } = new List<GroupAutoReplyMessage>();
    
    [Timestamp]
    public byte[] RowVersion { get; set; }
}