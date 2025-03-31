using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.WassengerClient;
using MurderBot.Website.Models;

namespace MurderBot.Website.Controllers;

public class GCController : Controller
{
    private readonly ILogger<GCController> _logger;
    private readonly MurderContext _dbContext;
    private readonly CommonMurderSettings _murderSettings;

    public GCController(ILogger<GCController> logger, MurderContext dbContext, IOptions<CommonMurderSettings> murderSettings)
    {
        _logger = logger;
        _dbContext = dbContext;
        _murderSettings = murderSettings.Value;
    }


    [HttpPost]
    public async Task<IActionResult> ReAdd(Guid id, ReAddViewModel viewModel,
        [FromServices] WassengerClient apiClient)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        
        var token = await _dbContext.AutoReAddToken.FirstOrDefaultAsync(
            g => g.TokenGuid == id);

        if (token == null)
            return NotFound();
        
        var participant = await _dbContext.Participant.SingleOrDefaultAsync(p => p.WId == token.ParticipantId);
        
        if (token.DateClaimed != null || DateTimeOffset.Now > token.ExpirationDate)
        {
            RedirectToAction("ReAdd", new { id });
        }

        var success = await apiClient.AddGroupParticipant(participant.Phone, token.GroupCheckIn.GroupId);
        
        if (!success)
        {
            ModelState.AddModelError("",
                "Unable to re-add you to the group. Ensure that you have added the Murder Bot as a contact.");
            return View(viewModel);
        }
        
        token.DateClaimed = DateTimeOffset.Now;
        await _dbContext.SaveChangesAsync();
        
        viewModel.RejoinTime = DateTimeOffset.Now;
        
        return View("ReAddConfirmation", viewModel);
    }
    
    [HttpGet]
    public async Task<IActionResult> ReAdd(Guid id)
    {
        var token = await _dbContext.AutoReAddToken.FirstOrDefaultAsync(
            g => g.TokenGuid == id);

        if (token == null)
            return NotFound();

        
        
        var participant = await _dbContext.Participant.SingleOrDefaultAsync(p => p.WId == token.ParticipantId);


        var murderBotParticipant = await _dbContext.Participant.SingleOrDefaultAsync(p => p.WId == _murderSettings.BotWid);

        var model = new ReAddViewModel
        {
            GroupName = token.GroupCheckIn.Group.Name,
            PhoneNumber = participant.Phone,
            RemovedTime = token.DateCreated,
            MurderBotPhoneNumber = murderBotParticipant.Phone
        };
        
        if (token.DateClaimed != null)
        {
            model.FatalError = "This re-join token has already been claimed.";
        }
        
        else if (token.ExpirationDate < DateTimeOffset.Now)
        {
            model.FatalError = "This re-join token has expired.";
        }
        
        return View(model);
    }

    private DateTimeOffset GetEasternDate(DateTimeOffset date)
    {
        return TimeZoneInfo.ConvertTime(date, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
    }

    
    private static string NormalizePhoneNumber(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
         {
             return string.Empty;
         }

        // Trim leading/trailing spaces
        rawInput = rawInput.Trim();

        // Check if the input already starts with '+'
        bool startsWithPlus = rawInput.StartsWith("+");
        
        bool startsWithOne = rawInput.StartsWith("1");

        // Extract only digits
        string digits = new string(rawInput.Where(char.IsDigit).ToArray());


        return (startsWithPlus || startsWithOne) ? $"+{digits}" : $"+1{digits}";
    }
    
    [HttpPost]
    public async Task<IActionResult> CheckPhoneStatus(Guid id, string phoneNumber)
    {
        var checkin = await _dbContext.GroupCheckIn.FirstOrDefaultAsync(
            g => g.UrlGuid == id);
        
        if (checkin == null)
            return NotFound();

        var phone = NormalizePhoneNumber(phoneNumber);

        var model = new CheckPhoneStatusResultViewModel
        {

            Id = id,
            PhoneNumber = phone,
            GroupName = checkin.Group.Name,
        };

        var participant = await _dbContext.Participant.FirstOrDefaultAsync(p => p.Phone == phone);

        if (participant == null)
        {
            model.Status = "ParticipantNotFound";
            model.AdditionalInfo = "This phone number was not found in the database. Check the number and try again.";
            return View(model);
        }

        var cinp = await _dbContext.GroupCheckInParticipantCheckIn.FirstOrDefaultAsync(p =>
            p.GroupCheckinId == checkin.GroupCheckinId && p.ParticipantId == participant.WId);
        
        if (cinp == null)
        {
            model.Status = "ParticipantNotFound";
            model.AdditionalInfo =
                "This number was not found as part of this group check-in. This may happen if you have joined the group recently. If the number is not part of the check-in, it will not be removed.";
            return View(model);
        }

        if (cinp.RemovalTime != null)
        {
            model.Status = "WasRemoved";
            model.AdditionalInfo = $"This participant did not reply and was removed from the group on {cinp.RemovalTime.Value:MM/dd/yyyy h:mm tt}.";
        }
        else if (cinp.CheckInSuccess != null)
        {
            model.Status = "WillNotBeRemoved";
            switch (cinp.CheckInMethod)
            {
                case CheckInMethod.Exempt:
                    model.AdditionalInfo = "You are exempt from removal";
                    break;
                case CheckInMethod.ReadCheckInMessage:
                    model.AdditionalInfo = "You will not be removed because you read the group check-in message.";
                    break;
                case CheckInMethod.RecentGroupMessage:
                    model.AdditionalInfo = "You will not be removed because you've posted in the group recently.";
                    break;
                case CheckInMethod.RepliedToCheckInMessage:
                    model.AdditionalInfo = "You will not be removed because you replied to the check-in DM";
                    break;
            }
        }
        else
        {
            model.Status = "EligibleForRemoval";
            if (checkin.ParticipantsReadFinished != null)
            {
                model.AdditionalInfo = "Make sure you respond to the check-in DM or you will be removed.";
            }
            else
            {
                model.AdditionalInfo = "The model is still waiting to see who reads the check-in message. It may take several hours for this to update. If you have recently read the message, check back later.";
            }
        }

            
        return View(model);
    }

    private DateTimeOffset CheckHourRange(DateTimeOffset dateToCheck)
    {
        if (dateToCheck.Hour > _murderSettings.WebJobEndHour)
            return dateToCheck.Date.AddDays(1).AddHours(_murderSettings.WebJobStartHour);

        if (dateToCheck.Hour < _murderSettings.WebJobStartHour)
            return dateToCheck.Date.AddHours(_murderSettings.WebJobStartHour);

        return dateToCheck;
    }

    public async Task<IActionResult> Status(Guid id)
    {

        
        var checkin = await _dbContext.GroupCheckIn.FirstOrDefaultAsync(
            g => g.UrlGuid == id);
        
        if (checkin == null)
            return NotFound();

        var firstMessage = checkin.Messages.FirstOrDefault(m => m.OutgoingMessage.SendAt != null);

        var readStartTime = DateTimeOffset.Now.AddMinutes(2);
        var readStarted = false;
        if (firstMessage != null)
        {
            readStartTime = firstMessage.OutgoingMessage.SendAt.Value;
            readStarted = true;
        }

        var messageStartTime = readStartTime + checkin.Group.CheckInReadTimeout + _murderSettings.WebJonRunInterval;
        messageStartTime = CheckHourRange(messageStartTime);
        var messagesStarted = false;

        if (checkin.FirstMessageSent != null)
        {
            messagesStarted = true;
            messageStartTime = checkin.FirstMessageSent.Value;
        }

        var removalsComplete = checkin.RemovalsCompleted != null;


        var exemptCount = checkin.ParticipantsCheckIns.Count(p => p.CheckInMethod == CheckInMethod.Exempt);
        var recentMessageCount = checkin.ParticipantsCheckIns.Count(p => p.CheckInMethod == CheckInMethod.RecentGroupMessage);
        var readCount = checkin.ParticipantsCheckIns.Count(p => p.CheckInMethod == CheckInMethod.ReadCheckInMessage);
        var replyCount = checkin.ParticipantsCheckIns.Count(p => p.CheckInMethod == CheckInMethod.RepliedToCheckInMessage);
       
        
        var model = new MurderBotStatusViewModel
        {
            Id = id,
            Stages = new List<RemovalStageViewModel>
            {
                new RemovalStageViewModel
                {
                    StageName = "Check for Exempt Participants",
                    StartTime = GetEasternDate(checkin.DateCreated),
                    ParticipantsNotRemoved = exemptCount,
                    IsCurrentStage = false,
                    IsComplete = true,
                    StageDescription = "Save exempted participants such as the bot itself and the owner of the group"
                },
                new RemovalStageViewModel
                {
                    StageName = "Checking for Recent Posts in The Group",
                    StartTime = GetEasternDate(checkin.DateCreated),
                    ParticipantsNotRemoved = recentMessageCount,
                    IsCurrentStage = !readStarted,
                    IsComplete = true,
                    StageDescription = "If you have posted in the group recently, you won’t be removed"
                },
                new RemovalStageViewModel
                {
                    StageName = "Notify Group & Check Who Reads the Message",
                    StartTime = GetEasternDate(readStartTime),
                    ParticipantsNotRemoved = readCount,
                    IsCurrentStage = readStarted && !messagesStarted,
                    IsComplete = messagesStarted,
                    StageDescription = "The Murder Bot will send out a check in message to the group. Any participant who reads this message will be saved from removal"
                },
                new RemovalStageViewModel
                {
                    StageName = "Send Individual Messages and Wait for Replies",
                    StartTime = GetEasternDate(messageStartTime),
                    ParticipantsNotRemoved = replyCount,
                    IsCurrentStage = messagesStarted && checkin.ChatResponsesFinished == null,
                    IsComplete = checkin.ChatResponsesFinished != null,
                    StageDescription = "Each participant that is still eligible for removal will be messaged individually. If that participant replies before the start of the removal stage, they will be saved from removal"
                }
            }
        };

        model.GroupName = checkin.Group.Name;
        model.TotalParticipants = checkin.ParticipantsCheckIns.Count;

        if (checkin.RemovalsCompleted == null)
        {
            model.ParticipantsEligibleForRemoval =
                checkin.ParticipantsCheckIns.Count(c => c.CheckInSuccess == null);

            if (checkin.ParticipantsReadFinished != null)
            {
                model.RemovalStartTime = checkin.ParticipantsReadFinished.Value + checkin.Group.CheckInMessageResponseTimeout + _murderSettings.WebJonRunInterval;
            }
            else
            {
                model.RemovalStartTime = checkin.DateCreated +
                                         checkin.Group.CheckInMessageResponseTimeout
                                         + checkin.Group.CheckInReadTimeout + (_murderSettings.WebJonRunInterval * 2);
            }
            model.RemovalStartTime = CheckHourRange(model.RemovalStartTime);
            
        }
        else
        {
            model.ParticipantsEligibleForRemoval =
                checkin.ParticipantsCheckIns.Count(c => c.RemovalTime != null);

            model.RemovalStartTime = checkin.RemovalsCompleted.Value;
        }
     
        model.RemovalCompleted = removalsComplete;

        model.RemovalStartTime = GetEasternDate(model.RemovalStartTime);
        
        return View(model);
    }
}