namespace MurderBot.Website.Models;

public class ReAddViewModel
{
    public string? GroupName { get; set; }
    public string? PhoneNumber { get; set; }
    
    public string? MurderBotPhoneNumber { get; set; }
    public DateTimeOffset RemovedTime { get; set; }
    
    public DateTimeOffset? RejoinTime { get; set; }
    
    public bool IAgreeIAddedTheContact { get; set; }
    
    public bool IAgreeToBeActive { get; set; }
    
    public string? FatalError { get; set; }
}