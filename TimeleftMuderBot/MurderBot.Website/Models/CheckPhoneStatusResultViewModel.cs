namespace MurderBot.Website.Models;

public class CheckPhoneStatusResultViewModel
{
    public Guid Id { get; set; }
    public string PhoneNumber { get; set; }
    public string Status { get; set; } 
    // Possible values: "EligibleForRemoval", "WillNotBeRemoved", "ParticipantNotFound"
    
    public string GroupName { get; set; }
    public string AdditionalInfo { get; set; }
    
    public bool RemovalsCompleted { get; set; }
    
    public DateTimeOffset? LastReadCountCompleted { get; set; }
    
}