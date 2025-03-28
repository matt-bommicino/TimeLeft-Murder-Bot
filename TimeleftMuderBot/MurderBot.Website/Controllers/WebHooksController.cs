using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Routines.Web;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.WassengerClient.Models.WebHooks;
using Newtonsoft.Json;


namespace MurderBot.Website.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebHooksController : ControllerBase
    {
        private readonly ILogger<WebHooksController> _logger;
        private readonly MurderContext _dbContext;

        private readonly CommonMurderSettings _murderOptions;

        public WebHooksController(ILogger<WebHooksController> logger,
            IOptions<CommonMurderSettings> murderOptions,
            MurderContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
            _murderOptions = murderOptions.Value;
        }
        
        [HttpPost]
        [Route("incomingmessage")]
        public async Task<IActionResult> IncomingMessage(string? secret,
            [FromServices] ProcessIncomingMessageRoutine processIncomingMessageRoutine,
            [FromServices] AutoReplyRoutine autoReplyRoutine)
        {
            if (secret != _murderOptions.WassengerWebhookSecret)
                return Unauthorized();
            
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.LogInformation($"Received incoming message: {body}");
            
            var webhook = JsonConvert.DeserializeObject<MessageWebhook>(body);

            if (webhook?.Data != null)
            {
                if (webhook.Data.Flow != "inbound")
                {
                    //wrong webhook??
                    return UnprocessableEntity();
                }
                var msgBody = webhook.Data?.Body;
                if (string.IsNullOrWhiteSpace(msgBody))
                {
                    msgBody = "[object]";
                }
                
                //check for minimum needed field
                if (webhook.Data.From == null || webhook.Data.Id == null
                    || webhook.Data.FromNumber == null)
                    return UnprocessableEntity();
                

                var dbParticipant = await _dbContext.Participant
                    .FirstOrDefaultAsync(p => p.Phone == webhook.Data.FromNumber);

                if (dbParticipant == null)
                {
                    //if it's not someone we know about we don't care
                    return Ok();
                }
                
                var participantId = dbParticipant.WId;
                
    
                
                //record the message
                var chatMessage = new ChatMessage
                {
                    Body = msgBody,
                    Id = webhook.Data.Id,
                    ChatId = webhook.Data.From,
                    WaId = webhook.Data.Id,
                    ParticipantId = participantId,
                    SendAt = webhook.Data.Date ?? DateTimeOffset.Now,
                    DeliverAt = webhook.Data.Date ?? DateTimeOffset.Now,
                    OutgoingMessage = false
                };
                _dbContext.ChatMessage.Add(chatMessage);
                await _dbContext.SaveChangesAsync();

                //group message
                if (chatMessage.ChatId != chatMessage.ParticipantId)
                    await autoReplyRoutine.Execute(chatMessage);
                
                await processIncomingMessageRoutine.Execute(chatMessage);
                
                return Ok();
            }


            return UnprocessableEntity();
        }
        
        [HttpPost]
        [Route("outgoingmessage")]
        public async Task<IActionResult> OutgoingMessage(string? secret)
        {
            if (secret != _murderOptions.WassengerWebhookSecret)
                return Unauthorized();
            
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }
            _logger.LogInformation($"Received outgoing message: {body}");
            var webhook = JsonConvert.DeserializeObject<MessageWebhook>(body);

            if (webhook != null)
            {
                if (webhook.Data != null)
                {
                    if (webhook.Data.Flow != "outbound")
                    {
                        //wrong webhook??
                        return UnprocessableEntity();
                    }
                    var messageId = webhook.Data.Id;
                    var dateSent = webhook.Data.Date ?? DateTimeOffset.Now;

                    var dbMessage = await _dbContext.ChatMessage.FirstOrDefaultAsync(m => m.WaId == messageId);

                    if (dbMessage == null)
                    {
                        _logger.LogInformation($"Message {messageId} not found in database");
                    }
                    else
                    {
                        dbMessage.SendAt = dateSent;
                        await _dbContext.SaveChangesAsync();
                    }

                    return Ok();
                }
            }
            
 
            return UnprocessableEntity();
        }
        
        [HttpPost]
        [Route("messageack")]
        public async Task<IActionResult> MessageAck(string? secret)
        {
            if (secret != _murderOptions.WassengerWebhookSecret)
                return Unauthorized();
            
            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
            }
            
            _logger.LogInformation($"Received message Ack: {body}");


            return Ok();
        }


    }
}
