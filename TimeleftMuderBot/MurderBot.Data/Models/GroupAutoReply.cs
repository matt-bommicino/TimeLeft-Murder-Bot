using System.ComponentModel.DataAnnotations;
using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;

public class GroupAutoReply : IDateCreated
{
    [Key]
    public int GroupAutoReplyId { get; set; }
    
    public required string TriggerRegEx { get; set; }
    
    public required string ReplyMessage { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
    public string? GroupId { get; set; }
    
    public Group Group { get; set; } = null!;
}