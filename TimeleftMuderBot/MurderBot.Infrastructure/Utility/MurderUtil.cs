using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.WassengerClient.Models;

namespace MurderBot.Infrastructure.Utility;

public class MurderUtil
{
    private readonly ILogger<MurderUtil> _logger;
    private readonly WassengerClient.WassengerClient _apiClient;
    private readonly MurderContext _dbContext;

    public MurderUtil(ILogger<MurderUtil> logger, WassengerClient.WassengerClient apiClient, MurderContext dbContext)
    {
        _logger = logger;
        _apiClient = apiClient;
        _dbContext = dbContext;
    }
    
    public async Task RemoveGroupParticipant(string groupId, string participantId)
    {
        var gp = await _dbContext.GroupParticipant.Include(groupParticipant => groupParticipant.Participant)
            .FirstOrDefaultAsync(x => x.GroupId == groupId && x.ParticipantId == participantId);
        if (gp == null)
            throw new Exception($"GroupParticipant {groupId}:{participantId} not found");

        await _apiClient.RemoveGroupParticipant(gp.Participant.Phone, gp.GroupId);
        
        _dbContext.GroupParticipant.Remove(gp);
        
        await _dbContext.SaveChangesAsync();
        _logger.LogInformation($"Successfully removed group participant {groupId}:{participantId}");

    }

    public Task<ChatMessage> SendGroupMessage(string groupId, string message, string? quoteMessage = null)
    {
        var inputMessage = new OutgoingMessageInput
        {
            Message = message,
            Group = groupId,
            Quote = quoteMessage
        };
        return SendMessage(inputMessage);
    }

    public async Task<ChatMessage> SendParticipantMessage(string participantId, string message)
    {
        var phone = await _dbContext.Participant
            .Where(p => p.WId == participantId)
            .Select(p => p.Phone).SingleAsync();
        
        var inputMessage = new OutgoingMessageInput
        {
            Message = message,
            Phone = phone
        };
        return await SendMessage(inputMessage, participantId);
    }

    private async Task<ChatMessage> SendMessage(OutgoingMessageInput message, string? participantId = null)
    {
        var apiResult = await _apiClient.SendMessage(message);
        var chatMessage = new ChatMessage
        {
            Body = message.Message,
            Id = apiResult.Id,
            OutgoingMessage = true,
            ChatId = apiResult.Wid,
            WaId = apiResult.WaId,
            DeliverAt = apiResult.DeliverAt,
            ParticipantId = participantId
        };
        _dbContext.ChatMessage.Add(chatMessage);
        await _dbContext.SaveChangesAsync();
        return chatMessage;
    }
    
    
}