using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.Utility;

namespace MurderBot.Infrastructure.Routines;

public class ReAddRoutine : IServiceRoutine
{
    
    private readonly CommonMurderSettings _murderOptions;
    private readonly WassengerClient.WassengerClient _apiClient;
    private readonly MurderContext _dbContext;
    private readonly ILogger<ReAddRoutine> _logger;
    private readonly MurderUtil _murderUtil;

    public ReAddRoutine(IOptions<CommonMurderSettings> murderOptions, 
        WassengerClient.WassengerClient apiClient,
        MurderContext dbContext,
        ILogger<ReAddRoutine> logger, MurderUtil murderUtil)
    {
        _murderOptions = murderOptions.Value;
        _apiClient = apiClient;
        _dbContext = dbContext;
        _logger = logger;
        _murderUtil = murderUtil;
    }
    
    public async Task Execute()
    {
        //This job is written this way so that requests that come in while the job is already running will be processed
        var ranOnce = false;
        while (true)
        {
            //get pending re-adds
            var readds = 
                await _dbContext.ReAddJobTrigger.Where(j => j.JobCompleteDate == null)
                .ToListAsync();

            if (!readds.Any())
            {
                if (!ranOnce)
                    _logger.LogInformation("ReAddRoutine started but no pending ReAdds were found.");
                return;
            }
            
            //wait a minute before attempting re-adds again
            if (ranOnce)
            {
                _logger.LogInformation("Attempting delay");
                await Task.Delay(TimeSpan.FromMinutes(1));
            }

            foreach (var job in readds)
            {
                if (!job.JobStartDate.HasValue)
                {
                    await SendStartMessage(job);
                    job.JobStartDate = DateTimeOffset.Now;
                    await  _dbContext.SaveChangesAsync();
                }

                string result ;
                var paricipantId = job.AutoReAddToken.ParticipantId;

                try
                {
                    

                    var participant = _dbContext.Participant.Single(p => p.WId == paricipantId);
                    var groupId = job.AutoReAddToken.GroupCheckIn.GroupId;

                    result = await _apiClient.AddGroupParticipant(participant.Phone, groupId);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Error readding participant {paricipantId}");
                    result = "exception";
                }
                
                //success
                if (string.IsNullOrWhiteSpace(result))
                {
                    job.AutoReAddToken.DateClaimed = DateTimeOffset.Now;
                    job.JobCompleteDate = DateTimeOffset.Now;
                    await _dbContext.SaveChangesAsync();
                    await SendReAddSuccessMessage(job);
                    _logger.LogInformation($"{paricipantId} readded successfully.");
                }
                //participant already added
  
                else if (result.StartsWith("Conflict"))
                {
                    _logger.LogError(result);
                    job.AutoReAddToken.DateClaimed = DateTimeOffset.Now;
                    job.JobCompleteDate = DateTimeOffset.Now;
                    await _dbContext.SaveChangesAsync();
                    await SendAlreadyAddedMessage(job);
                    _logger.LogInformation($"{paricipantId} already in the group.");
                }
                else
                {
                    _logger.LogError(result);
                    job.RetryCount++;

                    if (job.RetryCount >= _murderOptions.MaxReAddAttempts)
                    {
                        job.JobCompleteDate = DateTimeOffset.Now;
                        await _dbContext.SaveChangesAsync();
                        await SendReAddFailedMessage(job);
                    }
                    else
                    {
                        await _dbContext.SaveChangesAsync();
                    }
                }
            }
            
            ranOnce = true;
        }
        
    }

    private async Task SendParticipantMessage(MessageTemplateType templateId, ReAddJobTrigger job,
        Action<string, ReAddJobTrigger> messageIdSetter)
    {
        var messageText = await GetMessageText(templateId, job);
        
        var paricipantId = job.AutoReAddToken.ParticipantId;
        
        var cm = await _murderUtil.SendParticipantMessage(paricipantId, messageText);
        messageIdSetter(cm.WaId, job);
        await  _dbContext.SaveChangesAsync();
    }
    
    private Task SendAlreadyAddedMessage(ReAddJobTrigger job)
    {
        return SendParticipantMessage(MessageTemplateType.ReAddFailParticipantAlreadyInGroup, job, 
            (waId, j) => j.FailureMessageId = waId);
    }
    

    private Task SendStartMessage(ReAddJobTrigger job)
    {
        return SendParticipantMessage(MessageTemplateType.ReAddProcessStartMessage, job, 
            (waId, j) => j.StartMessageId = waId);
    }
    
    private Task SendReAddSuccessMessage(ReAddJobTrigger job)
    {
        return SendParticipantMessage(MessageTemplateType.ReAddSuccessMessage, job, 
            (waId, j) => j.SuccessMessageId = waId);
    }
    
    private Task SendReAddFailedMessage(ReAddJobTrigger job)
    {
        return SendParticipantMessage(MessageTemplateType.ReAddFailedMessage, job, 
            (waId, j) => j.FailureMessageId = waId);
    }


    private async Task<String> GetMessageText(MessageTemplateType templateId, ReAddJobTrigger job)
    {
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == templateId);

        if (messageTemplate == null)
        {
            throw new Exception($"Message Template {templateId} not found");
        }
        
        var groupName = job.AutoReAddToken.GroupCheckIn.Group.Name;

        var text = messageTemplate.MessageBody.Replace("%groupname%", groupName);
        
        return text;
    }
    

    public bool CascadeFailure => true;
}