using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Utility;

namespace MurderBot.Infrastructure.Routines.Web;

public class ProcessIncomingMessageRoutine
{
    private readonly ILogger<ProcessIncomingMessageRoutine> _logger;
    private readonly MurderUtil _murderUtil;
    private readonly MurderContext _dbContext;

    public ProcessIncomingMessageRoutine(ILogger<ProcessIncomingMessageRoutine> logger,
        MurderUtil murderUtil, MurderContext dbContext)
    {
        _logger = logger;
        _murderUtil = murderUtil;
        _dbContext = dbContext;
    }

    private Participant? _dbParticipant;
    
    private ChatMessage _chatMessage;
    
    public async Task Execute(ChatMessage chatMessage)
    {
        _chatMessage = chatMessage;
        _dbParticipant = await _dbContext.Participant.FirstOrDefaultAsync(p => p.WId ==
                                                                              chatMessage.ParticipantId);
        
        if (_dbParticipant == null)
            return;

        if (_chatMessage.ParticipantId == _chatMessage.ChatId)
        {
            await ProcessIncomingParticipantMessage();
        }
        else
        {
            await ProcessIncomingGroupMessage();
        }
    }

    private async Task ProcessIncomingGroupMessage()
    {
        var groupId = _chatMessage.ChatId;
        var gp = await _dbContext.GroupParticipant.FirstOrDefaultAsync(p => p.GroupId == groupId
          && p.ParticipantId == _chatMessage.ParticipantId);
        
        if (gp == null)
            return;

        gp.LastGroupMessage = _chatMessage.DateCreated;
        await _dbContext.SaveChangesAsync();
        
        //find active checkins
        var checkIns = await _dbContext.GroupCheckInParticipantCheckIn
            .Where(c => c.ParticipantId == gp.ParticipantId
                        && c.GroupCheckIn.GroupId == gp.GroupId && c.RemovalTime == null
                        && c.CheckInSuccess == null && c.GroupCheckIn.RemovalsCompleted == null).ToListAsync();

        foreach (var checkIn in checkIns)
        {
            checkIn.CheckInSuccess = DateTimeOffset.Now;
            checkIn.CheckInMethod = CheckInMethod.RecentGroupMessage;
            await _dbContext.SaveChangesAsync();
        }

    }
    
    private async Task ProcessIncomingParticipantMessage()
    {
        //find any check-in where we send a message but didn't get one back
        //find active checkins
        var checkIns = await _dbContext.GroupCheckInParticipantCheckIn
            .Where(c => c.ParticipantId == _chatMessage.ParticipantId
                        && c.RemovalTime == null && c.CheckInMessageId != null
                        && c.CheckInSuccess == null && c.GroupCheckIn.RemovalsCompleted == null).ToListAsync();
        
        foreach (var checkIn in checkIns)
        {
            checkIn.CheckInSuccess = DateTimeOffset.Now;
            checkIn.CheckInMethod = CheckInMethod.RepliedToCheckInMessage;
            checkIn.MessageReceivedTime = DateTimeOffset.Now;
            checkIn.IncomingMessageId = _chatMessage.WaId;
            
            await _dbContext.SaveChangesAsync();


            var messageTemplate = await _dbContext.MessageTemplate
                .OrderByDescending(t => t.DateCreated)
                .FirstOrDefaultAsync(t =>
                    t.IsActive && t.MessageTemplateType == MessageTemplateType.ParticipantReplyMessage);

            if (messageTemplate != null)
            {
                var messageText = messageTemplate.MessageBody.Replace("%groupname%", checkIn.GroupCheckIn.Group.Name);
                await _murderUtil.SendParticipantMessage(checkIn.ParticipantId, messageText);
            }
        }
        
        
    }
}