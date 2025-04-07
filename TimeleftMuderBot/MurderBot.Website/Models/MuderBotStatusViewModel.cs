namespace MurderBot.Website.Models;

public class RemovalStageViewModel
{
    public string StageName { get; set; }
    public DateTimeOffset StartTime { get; set; }        // The time the stage started or is scheduled to start
    public int ParticipantsNotRemoved { get; set; } // Number of participants saved by this stage
    public bool IsCurrentStage { get; set; }        // Indicates if this is the current active stage
    public bool IsComplete { get; set; }            // Indicates if this stage has been completed
    
    public string StageDescription { get; set; }
}

public class MurderBotStatusViewModel
{
    public Guid Id { get; set; }
    public List<RemovalStageViewModel> Stages { get; set; }
    
    public string GroupName { get; set; }
    
    public int TotalParticipants { get; set; }
    
    public int ParticipantsEligibleForRemoval { get; set; }
    
    public DateTimeOffset RemovalStartTime { get; set; }
    
    public bool RemovalCompleted { get; set; }
    
    public bool IsInReadingStage { get; set; }
    
    public DateTimeOffset? LastReadCountCompleted { get; set; }
}