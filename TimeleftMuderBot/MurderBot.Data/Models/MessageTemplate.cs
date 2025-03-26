using MurderBot.Data.Interface;

namespace MurderBot.Data.Models;


public enum MessageTemplateType
{
    NotSet = 0,
    GroupCheckInMessage = 1,
    ParticipantCheckInMessage = 2,
    ParticipantReplyMessage = 3,
    ParticipantRemovedMessage = 4,
}

public class MessageTemplate: IDateCreated, IDateModified
{
    public int MessageTemplateId { get; set; }
    public DateTimeOffset DateCreated { get; set; }
    public DateTimeOffset DateModified { get; set; }
    
    public required string MessageBody { get; set; }
    
    public bool IsActive { get; set; }
    
    public MessageTemplateType MessageTemplateType { get; set; }
    
}