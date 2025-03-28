using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.Utility;

namespace MurderBot.Infrastructure.Routines.Web;

public class AutoReplyRoutine
{
    private readonly ILogger<AutoReplyRoutine> _logger;
    private readonly MurderContext _dbContext;
    private readonly MurderUtil _murderUtil;
    private readonly CommonMurderSettings _murderSettings;

    public AutoReplyRoutine(ILogger<AutoReplyRoutine> logger,
        MurderContext dbContext, MurderUtil murderUtil,
        IOptions<CommonMurderSettings> murderSettings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _murderUtil = murderUtil;
        _murderSettings = murderSettings.Value;
    }
    
    public async Task Execute(ChatMessage chatMessage)
    {
        var autoReplies = _dbContext.GroupAutoReply.ToList();
        foreach (var autoReply in autoReplies)
        {
            try
            {
                if (autoReply.GroupId != null)
                {
                    if (chatMessage.ChatId != autoReply.GroupId)
                        continue;
                }
                else
                {
                    var group = await _dbContext.Group.FirstOrDefaultAsync(g => g.WId == chatMessage.ChatId);
                    if (group == null || group.Ignore)
                        continue;
                }
            
                var regex = new Regex(autoReply.TriggerRegEx, RegexOptions.IgnoreCase | RegexOptions.Multiline);

                if (regex.IsMatch(chatMessage.Body))
                {
                    //get the last autoReply for this configuration
                    var lastMessageDate = await _dbContext.GroupAutoReplyMessage
                        .Where(m => m.GroupAutoReplyId == autoReply.GroupAutoReplyId
                        && m.OutgoingMessage.ChatId == chatMessage.ChatId)
                        .OrderByDescending(m => m.DateCreated)
                        .Select(m => m.DateCreated)
                        .FirstOrDefaultAsync();

                    //Send the autoreply
                    if (lastMessageDate + _murderSettings.AutoReplyTimeout < DateTimeOffset.UtcNow)
                    {
                        var replyMessage = await _murderUtil.SendGroupMessage(
                            chatMessage.ChatId, autoReply.ReplyMessage, chatMessage.WaId);

                        var dbReply = new GroupAutoReplyMessage
                        {
                            GroupAutoReplyId = autoReply.GroupAutoReplyId,
                            OutgoingMessageId = chatMessage.WaId
                        };
                        _dbContext.GroupAutoReplyMessage.Add(dbReply);
                        await _dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        _logger.LogInformation($"Skipping auto reply {autoReply.GroupAutoReplyId} because we just replied at {lastMessageDate}");
                    }

                }
            }
            catch (Exception e)
            {
               _logger.LogError(e, $"Could not process auto reply {autoReply.GroupAutoReplyId} for message ID {chatMessage.WaId}");
            }
            
            
        }
        
        
        
        
    }
}