namespace MuderBot.Infrastructure.Settings;

public class CommonMurderSettings
{
    public string? WassengerApiToken { get; set; }
    
    public string? WassengerDeviceId { get; set; }
    
    public string? WebsiteBaseUrl { get; set; }
    
    public string? MurderContextConnectionString { get; set; }
    
    /// <summary>
    /// The WhatsApp ID of this bot
    /// </summary>
    public string? BotWid { get; set; }
    
    
    /// <summary>
    /// The amount of time to wait for participants to read
    /// the group check in message before messaging unread
    /// participants individually
    /// </summary>
    public TimeSpan DefaultCheckInReadTimeout { get; set; } = TimeSpan.FromDays(7);
    
    /// <summary>
    /// The amount of time to wait for a response to the check in
    /// message before removing the participant
    /// </summary>
    public TimeSpan DefaultCheckInMessageResponseTimeout { get; set; } = TimeSpan.FromDays(7);
    
    /// <summary>
    /// If the group received a message from this person within this amount of time
    /// they will not be removed
    /// </summary>
    public TimeSpan DefaultLastMessageExemptTime { get; set; } = TimeSpan.FromDays(30);
    
    /// <summary>
    /// The minimum time before the bot starts the whole process
    /// over again since the last time it started
    /// </summary>
    public TimeSpan DefaultMinimumTimeBetweenRuns { get; set; } = TimeSpan.FromDays(30);
    
    
    
}