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
    private readonly CommonMurderSettings _murderSettings;

    public GroupMurderRoutine(ILogger<GroupMurderRoutine> logger,
        MurderContext dbContext, IOptions<CommonMurderSettings> murderSettings,
        MurderUtil murderUtil)
    {
        _logger = logger;
        _dbContext = dbContext;
        _murderUtil = murderUtil;
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
            return;
        }

        if (groupCheckIn.ParticipantsReadFinished != null && groupCheckIn.FirstMessageSent == null)
        {
            await  ProcessSendMessagesStage(groupCheckIn);
            return;
        }

        if (groupCheckIn.FirstMessageSent != null && groupCheckIn.ChatResponsesFinished == null)
        {
            await ProcessWaitForMessagesStage(groupCheckIn);
            return;
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
            
            //the bot itself is always exempt
            if (partCheckIn.ParticipantId == _murderSettings.BotWid)
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.Exempt;
            }

            if (_exemptParticipants.Any(ep => ep.ParticipantId == partCheckIn.ParticipantId
                                              && (ep.GroupId == null || ep.GroupId == group.WId)))
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.Exempt;
            }

            var postCutoff = DateTimeOffset.Now - group.LastMessageExemptTime;

            if (gp.LastGroupMessage > postCutoff)
            {
                partCheckIn.CheckInSuccess = DateTimeOffset.Now;
                partCheckIn.CheckInMethod = CheckInMethod.RecentGroupMessage;
            }

        }

        await _dbContext.SaveChangesAsync();

        await SendAndRecordGroupCheckInMessage(newCheckin);


    }

    private async Task SendAndRecordGroupCheckInMessage(GroupCheckIn groupCheckIn)
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

    private async Task CheckInMessageSendRecovery(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Message send recovery for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
        
        await SendAndRecordGroupCheckInMessage(groupCheckIn);
    }

    private async Task ProcessReadingStage(GroupCheckIn groupCheckIn)
    {
        _logger.LogInformation($"Wait for read recipients for {groupCheckIn.GroupCheckinId} : {groupCheckIn.Group.Name} : {groupCheckIn.GroupId}");
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
                        && c.CheckInSuccess == null).ToListAsync();
        
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

        var messageSent = false;
        foreach (var bp in badParticipants)
        {
            try
            {
                messageSent = messageSent || await DoParticipantSendMessage(groupCheckIn, bp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to send check in message for {bp.GroupCheckinId} : {bp.ParticipantId}");
            }


        }

        if (messageSent)
        {
            groupCheckIn.FirstMessageSent = DateTimeOffset.Now;
            await _dbContext.SaveChangesAsync();
        }
        
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
                        && c.CheckInSuccess == null).ToListAsync();


        foreach (var bp in badParticipants)
        {
            try
            {
                await DoParticipantRemoval(groupCheckIn, bp);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Unable to do participant removal for {bp.GroupCheckinId} : {bp.ParticipantId}");
            }


        }
        

    }

    private async Task<bool> DoParticipantSendMessage(GroupCheckIn groupCheckIn, GroupCheckInParticipantCheckIn bp)
    {
        if (bp.CheckInMessage?.SendAt != null)
        {
            _logger.LogWarning($"Message already sent for group check in {groupCheckIn.GroupId} : {bp.ParticipantId}");
            return false;
        }


        var cm = await _murderUtil.SendParticipantMessage(bp.ParticipantId, _participantCheckInMessageText);
        
        bp.CheckInMessageId = cm.WaId;
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