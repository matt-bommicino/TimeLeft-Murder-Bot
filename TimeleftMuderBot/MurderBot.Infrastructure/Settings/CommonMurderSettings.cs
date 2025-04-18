﻿namespace MurderBot.Infrastructure.Settings;

public class CommonMurderSettings
{
    public string? WassengerApiToken { get; set; }
    
    public string? WassengerDeviceId { get; set; }
    
    public string? WassengerWebhookSecret { get; set; }
    
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
    
    /// <summary>
    /// The amount of time before the bot will send the same auto-reply again
    /// </summary>
    public TimeSpan AutoReplyTimeout { get; set; } = TimeSpan.FromMinutes(30);
    
    
    /// <summary>
    /// This is to calculate the next run time for display purposes.
    /// It has no effect on the web job.
    /// </summary>
    public TimeSpan WebJonRunInterval { get; set; } = TimeSpan.FromMinutes(60);

    /// <summary>
    /// This is to calculate the next run time for display purposes.
    /// It has no effect on the web job.
    /// </summary>
    public int WebJobStartHour { get; set; } = 8;

    /// <summary>
    /// This is to calculate the next run time for display purposes.
    /// It has no effect on the web job.
    /// </summary>
    public int WebJobEndHour { get; set; } = 21;
    
    public int MaxReAddAttempts { get; set; } = 3;
    
    public string ReAddTriggerUrl { get; set; }
    
    public string ReAddJobUserName { get; set; }
    
    public string ReAddJobPassword { get; set; }

}