using System.ComponentModel.DataAnnotations;

namespace MurderBot.Data.Models;

public class GroupAutoReply
{
    [Key]
    public int GroupAutoReplyId { get; set; }
    
    public required string TriggerRegEx { get; set; }
    
    public required string ReplyMessage { get; set; }
    
    public DateTimeOffset DateCreated { get; set; }
    
}