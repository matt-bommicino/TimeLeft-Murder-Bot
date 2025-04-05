using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.Utility;

namespace MurderBot.Infrastructure.Routines;

public class GroupMurderRoutine : IServiceRoutine
{
    private readonly ILogger<GroupMurderRoutine> _logger;
    private readonly MurderContext _dbContext;
    private readonly MurderUtil _murderUtil;
    private readonly WassengerClient.WassengerClient _apiClient;
    private readonly CommonMurderSettings _murderSettings;

    public GroupMurderRoutine(ILogger<GroupMurderRoutine> logger,
        MurderContext dbContext, IOptions<CommonMurderSettings> murderSettings,
        MurderUtil murderUtil, WassengerClient.WassengerClient apiClient)
    {
        _logger = logger;
        _dbContext = dbContext;
        _murderUtil = murderUtil;
        _apiClient = apiClient;
        _murderSettings = murderSettings.Value;
    }
    
    private List<ExemptParticipant> _exemptParticipants;
    
    public bool CascadeFailure => false;
    public async Task Execute()
    {
        var groups = await _dbContext.Group.Where(g => g.Ignore == false
                                                 && g.DoMurders == true && g.IsBotAdmin == true).ToListAsync();

        _exemptParticipants = await _dbContext.ExemptParticipant.ToListAsync();
        
        foreach (var group in groups)
        {
            await ProcessGroup(group);
        }
        
    }

    private async Task ProcessGroup(Group group)
    {
        //get the latest group check in
        var groupCheckIn = await _dbContext.GroupCheckIn.OrderByDescending(c => c.DateCreated)
            .FirstOrDefaultAsync(c => c.GroupId == group.WId);

        if (groupCheckIn == null)
        {
            await StartNewCheckIn(group);
            return;
        }

        if (groupCheckIn.RemovalsCompleted != null)
        {
            var cutoff = groupCheckIn.DateCreated + group.MinimumTimeBetweenRuns;

            if (DateTimeOffset.Now > cutoff)
            {
                await StartNewCheckIn(group);
            }
            return;
        }

        if (groupCheckIn.ParticipantsReadFinished == null && groupCheckIn.Messages.Count == 0)
        {
            await CheckInMessageSendRecovery(groupCheckIn);
            return;
        }

        if (groupCheckIn.ParticipantsReadFinished == null)
        {
            await ProcessReadingStage(groupCheckIn);
        }

        if (groupCheckIn.ParticipantsReadFinished != null && groupCheckIn.FirstMessageSent == null)
        {
            await  ProcessSendMessagesStage(groupCheckIn);
            return;
        }

        if (groupCheckIn.FirstMessageSent != null && groupCheckIn.ChatResponsesFinished == null)
        {
            await ProcessWaitForMessagesStage(groupCheckIn);
        }

        if (groupCheckIn.ChatResponsesFinished != null && groupCheckIn.RemovalsCompleted == null)
        {
            await ProcessRemovalsStage(groupCheckIn);
            return;
        }
    }


    private async Task StartNewCheckIn(Group group)
    {
        _logger.LogInformation($"StartNewCheckIn for {group.Name} : {group.WId}");

        var newCheckin = new GroupCheckIn
        {
            GroupId = group.WId,
            UrlGuid = Guid.NewGuid(),
        };
        
        _dbContext.GroupCheckIn.Add(newCheckin);
        
        var groupParticipants = await _dbContext.GroupParticipant.Where(cp => cp.GroupId == group.WId).ToListAsync();
        
        //make a check in record for every participant

        foreach (var gp in groupParticipants)
        {
            var partCheckIn = new GroupCheckInParticipantCheckIn
            {
                ParticipantId = gp.ParticipantId
            };
            newCheckin.ParticipantsCheckIns.Add(partCheckIn);
            
            var postCutoff = DateTimeOffset.Now - group.LastMessageExemptTime;
            
            //the bot itself is always exempt, can't remove owners
            if (partCheckIn.ParticipantId == _murderSettings.BotWid || gp.IsOwner)
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.Exempt;
                
            }
            else if (_exemptParticipants.Any(ep => ep.ParticipantId == partCheckIn.ParticipantId
                                              && (ep.GroupId == null || ep.GroupId == group.WId)))
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.Exempt;
            }
            else if (gp.LastGroupMessage > postCutoff)
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.RecentGroupMessage;
            }

        }

        await _dbContext.SaveChangesAsync();

        await SendAndRecordGroupCheckInInitialMessage(newCheckin);


    }

    private async Task SendAndRecordGroupCheckInInitialMessage(GroupCheckIn groupCheckIn)
    {
        var checkinUrl = $"{_murderSettings.WebsiteBaseUrl}/GC/Status/{groupCheckIn.UrlGuid}";
        
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == MessageTemplateType.GroupCheckInMessage);

        if (messageTemplate != null)
        {
            var messageText = messageTemplate.MessageBody.Replace("%groupname%", groupCheckIn.Group.Name)
                .Replace("%progresslink%",checkinUrl);
            var cm = await _murderUtil.SendGroupMessage(groupCheckIn.GroupId, messageText);
            groupCheckIn.Messages.Add(
                new GroupCheckInMessage
                {
                    OutgoingMessageId = cm.WaId
                });
            
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Unable to find group check in message template");
        }
        
    }

    private async Task<string> GetNewRobotMurderJoke()
    {
        //get which Joke is least told
        var leastTold = await _dbContext.MurderJoke.MinAsync(j => j.TimesTold);
        
        //get a random joke that has been told the least number of times
        var joke = await _dbContext.MurderJoke
            .Where(j => j.TimesTold == leastTold)
            .OrderBy(j => Guid.NewGuid())
            .FirstOrDefaultAsync();
        
        if (joke == null)
        {
            throw new Exception("No murder jokes found in the database");
        }
        joke.TimesTold++;
        
        //the times told count will be saved when the message is sent

        return joke.JokeText;
    }
    
    
    private async Task SendAndRecordGroupCheckInReminderMessage(GroupCheckIn groupCheckIn)
    {
        var checkinUrl = $"{_murderSettings.WebsiteBaseUrl}/GC/Status/{groupCheckIn.UrlGuid}";
        
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == MessageTemplateType.GroupCheckInReminderMessage);

        if (messageTemplate != null)
        {
            var readCount = await _dbContext.GroupCheckInParticipantCheckIn
                .CountAsync(c => c.GroupCheckinId == groupCheckIn.GroupCheckinId &&
                                 c.CheckInMethod == CheckInMethod.ReadCheckInMessage);
            
            //get check in message ID to quote
            var checkinMessageID = await _dbContext.GroupCheckInMessage
                .Where(m => m.GroupCheckinId == groupCheckIn.GroupCheckinId
                            && m.OutgoingMessage.SendAt != null)
                .OrderBy(m => m.DateCreated)
                .Select(m => m.OutgoingMessageId)
                .FirstOrDefaultAsync();

            var murderJoke = string.Empty;
            if (messageTemplate.MessageBody.Contains("%murderjoke%"))
            {
                murderJoke = await GetNewRobotMurderJoke();
            }
            
            var messageText = messageTemplate.MessageBody.Replace("%groupname%", groupCheckIn.Group.Name)
                .Replace("%progresslink%",checkinUrl)
                .Replace("%readcount%", readCount.ToString())
                .Replace("%murderjoke%", murderJoke);
            
            var cm = await _murderUtil.SendGroupMessage(groupCheckIn.GroupId, messageText, checkinMessageID);
            groupCheckIn.Messages.Add(
                new GroupCheckInMessage
                {
                    OutgoingMessageId = cm.WaId
                });
            
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            throw new Exception("Unable to find group check in message template");
        }
        
    }

    /// <summary>
    /// Used if something went wrong when the bot attempts to send it's initial check in message
    /// </summary>
    /// <param name="groupCheckIn"></param>
    private async Task CheckInMessageSendRecovery(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Message send recovery for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
        
        await SendAndRecordGroupCheckInInitialMessage(groupCheckIn);
    }


    private async Task DoCheckInReminderMessages(GroupCheckIn groupCheckIn, int totalToSend,int totalSent)
    {
        var totalReadTime = groupCheckIn.Group.CheckInReadTimeout;
        var readStart = groupCheckIn.DateModified;

        var reminderInterval = totalReadTime / totalToSend;
        
        var nextInterval = totalSent * reminderInterval;
        
        var nextMessageTime  = readStart + nextInterval;

        if (DateTimeOffset.Now > nextMessageTime)
        {
            await SendAndRecordGroupCheckInReminderMessage(groupCheckIn);
        }

    }
    
    

    private async Task ProcessReadingStage(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Wait for read recipients for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");

        var badParticipants = await _dbContext.GroupCheckInParticipantCheckIn
            .Where(c => c.GroupCheckinId == groupCheckIn.GroupCheckinId
                        && c.CheckInSuccess == null).ToListAsync();

        //+1 for the initial check in message
        var totalMessagesToSend = groupCheckIn.Group.ReminderCheckinMessages + 1;
        
        var totalMessagesSent = await _dbContext.GroupCheckInMessage
            .CountAsync(m => m.GroupCheckinId == groupCheckIn.GroupCheckinId
            && m.OutgoingMessage.SendAt != null);

        if (totalMessagesToSend > totalMessagesSent)
        {
            await DoCheckInReminderMessages(groupCheckIn, totalMessagesToSend, totalMessagesSent);
        }
        
        
        var newRead = 0;
        foreach (var msg in groupCheckIn.Messages)
        {
            try
            {
                if (msg.OutgoingMessageId == null)
                    continue;

                var deliveryInfo = await _apiClient.GetDeliveryInfo(msg.OutgoingMessageId);

                foreach (var rp in deliveryInfo.Read)
                {
                    var badP = badParticipants.FirstOrDefault(p => p.ParticipantId == rp.Wid);
                    if (badP != null)
                    {
                        badP.CheckInSuccess = DateTimeOffset.Now;
                        badP.CheckInMethod = CheckInMethod.ReadCheckInMessage;
                        await _dbContext.SaveChangesAsync();
                        newRead++;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable get delivery info for {msg.OutgoingMessageId}");
            }
        }
        _logger.LogInformation($"Marked {newRead} participants as read");

        var checkInMessageSent = groupCheckIn.Messages
            .Where(c => c.OutgoingMessage.SendAt != null)
            .OrderBy(c => c.OutgoingMessage.SendAt)
            .Select(c => c.OutgoingMessage.SendAt)
            .FirstOrDefault();

        var cutoff = (checkInMessageSent ?? DateTimeOffset.Now) + groupCheckIn.Group.CheckInReadTimeout;

        if (DateTimeOffset.Now > cutoff)
        {
            groupCheckIn.ParticipantsReadFinished = DateTimeOffset.Now;
            await _dbContext.SaveChangesAsync();
        }

    }

    private string _participantCheckInMessageText;
    private async Task ProcessSendMessagesStage(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"sending messages for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
        
        var badParticipants = await _dbContext.GroupCheckInParticipantCheckIn
            .Where(c => c.GroupCheckinId == groupCheckIn.GroupCheckinId
                        && c.CheckInSuccess == null && c.MessageSentTime == null).ToListAsync();
        
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == MessageTemplateType.ParticipantCheckInMessage);

        var deadline = DateTimeOffset.Now + groupCheckIn.Group.CheckInMessageResponseTimeout;
        var eastern = TimeZoneInfo.ConvertTime(deadline, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
        var deadlineText = eastern.ToString("MM/dd/yyyy h:mm tt");
        
        if (messageTemplate != null)
        {
            _participantCheckInMessageText = messageTemplate.MessageBody.Replace("%groupname%", groupCheckIn.Group.Name)
                .Replace("%removaldeadline%",deadlineText);
        }
        else
        {
            throw new Exception("Unable to find participant check in message template");
        }

        var failures = 0;
        foreach (var bp in badParticipants)
        {
            try
            {
                await DoParticipantSendMessage(groupCheckIn, bp);
            }
            catch (Exception e)
            {
                failures++;
                _logger.LogError(e, $"Unable to send check in message for {bp.GroupCheckinId} : {bp.ParticipantId}");
            }


        }

        //try again later maybe
        if (failures > 0 && groupCheckIn.ChatMessageSendStageAttempts < groupCheckIn.Group.MessageSendStageMaxRetries)
        {
            _logger.LogWarning($"{failures} message failures, trying again later");
            groupCheckIn.ChatMessageSendStageAttempts++;
        }
        else
        {
            groupCheckIn.FirstMessageSent = DateTimeOffset.Now;
            
        }
        
        await _dbContext.SaveChangesAsync();
        
    }
    
    private async Task ProcessWaitForMessagesStage(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Waiting for messages for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
        
        var cutoff = (groupCheckIn.FirstMessageSent ?? DateTimeOffset.Now) + groupCheckIn.Group.CheckInMessageResponseTimeout;
        if (DateTimeOffset.Now > cutoff)
        {
            groupCheckIn.ChatResponsesFinished = DateTimeOffset.Now;
            await _dbContext.SaveChangesAsync();
        }
        
    }

    private async Task SendRemovalStartMessage(int numberToRemove, GroupCheckIn groupCheckIn)
    {
        
        //make sure we don't repeat send the removal message
        var existingRemovalMessages = await _dbContext.GroupCheckInMessage
            .CountAsync(m => m.GroupCheckinId == groupCheckIn.GroupCheckinId
             && m.DateCreated > groupCheckIn.ChatResponsesFinished);

        
        if (existingRemovalMessages > 0)
        {
            return;
        }
        
        // don't send a message for just a few people
        if (numberToRemove > 4)
        {
            var messageTemplate = await _dbContext.MessageTemplate
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync(t =>
                    t.IsActive && t.MessageTemplateType == MessageTemplateType.RemovalStartMessage);
            if (messageTemplate == null) return;
            
            var messageText = messageTemplate.MessageBody.Replace("%groupname%", groupCheckIn.Group.Name)
                .Replace("%numberofremovals%", numberToRemove.ToString());
            
            var cm = await _murderUtil.SendGroupMessage(groupCheckIn.GroupId, messageText);

            var newGcm = new GroupCheckInMessage
            {
                GroupCheckinId = groupCheckIn.GroupCheckinId,
                OutgoingMessageId = cm.WaId
            };
            _dbContext.GroupCheckInMessage.Add(newGcm);
            await _dbContext.SaveChangesAsync();
            
            //wait two minute for the message to hit the group before
            //starting removals
            _logger.LogInformation("Attempting delay");
            await Task.Delay(TimeSpan.FromMinutes(2));
        }
        
        
    }

    private async Task SendRemovalCompleteMessage(GroupCheckIn groupCheckIn)
    {
        
        var numberRemoved = await _dbContext.GroupCheckInParticipantCheckIn
            .CountAsync(c => c.GroupCheckinId == groupCheckIn.GroupCheckinId
             && c.RemovalTime != null);
        
        var templateId = MessageTemplateType.RemovalsCompletedMessage;
        if (numberRemoved < 1)
            templateId = MessageTemplateType.NoRemovalsResultMessage;

        var nextRunDate = groupCheckIn.DateCreated + groupCheckIn.Group.MinimumTimeBetweenRuns;
        
        if (nextRunDate.Hour > _murderSettings.WebJobEndHour)
            nextRunDate = nextRunDate.Date.AddDays(1).AddHours(_murderSettings.WebJobStartHour);
        
        var nextRunDateText = nextRunDate.ToString("d");
        
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == templateId);
        if (messageTemplate == null) return;
            
        var messageText = messageTemplate.MessageBody.Replace("%groupname%", groupCheckIn.Group.Name)
            .Replace("%nextrundate%", nextRunDateText);
            
        var cm = await _murderUtil.SendGroupMessage(groupCheckIn.GroupId, messageText);

        var newGcm = new GroupCheckInMessage
        {
            GroupCheckinId = groupCheckIn.GroupCheckinId,
            OutgoingMessageId = cm.WaId
        };
        _dbContext.GroupCheckInMessage.Add(newGcm);
        await _dbContext.SaveChangesAsync();
        
        
    }
    
    
    

    private string _removalMessageTemplate;
    private async Task ProcessRemovalsStage(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Proccessing removals for check in {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
        
        var messageTemplate = await _dbContext.MessageTemplate
            .OrderByDescending(t => t.DateCreated)
            .FirstOrDefaultAsync(t =>
                t.IsActive && t.MessageTemplateType == MessageTemplateType.ParticipantRemovedMessage);

        if (messageTemplate != null)
        {
            _removalMessageTemplate = messageTemplate.MessageBody;
        }
        else
        {
                throw new Exception("Unable to find removal message template");
        }
        
        
        
        
        //get all the candidates for removal

        var badParticipants = await _dbContext.GroupCheckInParticipantCheckIn
            .Where(c => c.GroupCheckinId == groupCheckIn.GroupCheckinId
                        && c.CheckInSuccess == null && c.RemovalTime == null).ToListAsync();
        
        await SendRemovalStartMessage(badParticipants.Count,groupCheckIn);

        var failures = 0;
        foreach (var bp in badParticipants)
        {
            try
            {
                await DoParticipantRemoval(groupCheckIn, bp);
                //wait 1 second between removals
                await Task.Delay(1000);
            }
            catch (Exception e)
            {
                failures++;
                _logger.LogError(e, $"Unable to do participant removal for {bp.GroupCheckinId} : {bp.ParticipantId}");
            }


        }

        // maybe try the failures again later
        if (failures > 0 && groupCheckIn.RemovalStageAttempts < groupCheckIn.Group.RemovalStageMaxRetries)
        {
            _logger.LogWarning($"{failures} removal failures, trying again later");
            groupCheckIn.RemovalStageAttempts++;
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            groupCheckIn.RemovalsCompleted = DateTimeOffset.Now;
            await _dbContext.SaveChangesAsync();

            await SendRemovalCompleteMessage(groupCheckIn);



        }
        
        
        

    }

    private async Task<bool> DoParticipantSendMessage(GroupCheckIn groupCheckIn, GroupCheckInParticipantCheckIn bp)
    {
        if (bp.CheckInMessage?.SendAt != null)
        {
            _logger.LogInformation($"Message already sent for group check in {groupCheckIn.GroupId} : {bp.ParticipantId}");
            return false;
        }


        var cm = await _murderUtil.SendParticipantMessage(bp.ParticipantId, _participantCheckInMessageText);
        
        bp.CheckInMessageId = cm.WaId;
        bp.MessageSentTime = DateTimeOffset.Now;
        await _dbContext.SaveChangesAsync();

        return true;

    }
    

    private async Task DoParticipantRemoval(GroupCheckIn groupCheckIn, GroupCheckInParticipantCheckIn bp)
    {
        //double check we actually sent the message
        var messageSent = bp.CheckInMessage?.SendAt;
        if (messageSent == null)
            return;
            
        //check if they already left the group
        var gp = await _dbContext.GroupParticipant.FirstOrDefaultAsync(
            p => p.ParticipantId == bp.ParticipantId &&
                 p.GroupId == groupCheckIn.GroupId);
            
        if (gp == null)
            return;

        var cutoff = DateTimeOffset.Now - groupCheckIn.Group.LastMessageExemptTime;

        if (gp.LastGroupMessage > cutoff)
        {
            bp.CheckInSuccess = DateTimeOffset.Now;
            bp.CheckInMethod = CheckInMethod.RecentGroupMessage;
            await _dbContext.SaveChangesAsync();
            return;
        }

        if (bp.IncomingMessageId != null)
        {
            bp.CheckInSuccess = DateTimeOffset.Now;
            bp.CheckInMethod = CheckInMethod.RepliedToCheckInMessage;
            await _dbContext.SaveChangesAsync();
            return;
        }
        
        //do the actual removal
        await _murderUtil.RemoveGroupParticipant(groupCheckIn.GroupId, bp.ParticipantId);
        
        var tokenGuid = Guid.NewGuid();
        //Create the reAdd token
        var readdToken = new AutoReAddToken
        {
            ParticipantId = bp.ParticipantId,
            GroupCheckinId = groupCheckIn.GroupCheckinId,
            TokenGuid = tokenGuid
        };
        _dbContext.AutoReAddToken.Add(readdToken);
        
        bp.RemovalTime = DateTimeOffset.Now;
        bp.CheckInMethod = CheckInMethod.ParticipantRemoved;
        bp.AutoReAddTokenId = tokenGuid;
        await _dbContext.SaveChangesAsync();
        
        //send the removal message
        var readdUrl = $"{_murderSettings.WebsiteBaseUrl}/GC/ReAdd/{readdToken.TokenGuid}";
        var groupName = groupCheckIn.Group.Name;
        
        
        var messageText = _removalMessageTemplate.Replace("%groupname%", groupName)
            .Replace("%rejoinlink%",readdUrl);
        var cm = await _murderUtil.SendParticipantMessage(bp.ParticipantId, messageText);
        bp.RemovalMessageId = cm.WaId;
        await _dbContext.SaveChangesAsync();
       


    }

    
    
    
    
}