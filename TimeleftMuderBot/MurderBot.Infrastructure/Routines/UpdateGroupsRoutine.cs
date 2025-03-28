using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.WassengerClient.Models;

namespace MurderBot.Infrastructure.Routines;

public class UpdateGroupsRoutine : IServiceRoutine
{
    private readonly CommonMurderSettings _murderOptions;
    private readonly WassengerClient.WassengerClient _apiClient;
    private readonly MurderContext _dbContext;
    private readonly ILogger<UpdateGroupsRoutine> _logger;

    public UpdateGroupsRoutine(IOptions<CommonMurderSettings> murderOptions, 
        WassengerClient.WassengerClient wassengerClient,
        MurderContext dbContext, ILogger<UpdateGroupsRoutine> logger)
    {
        _murderOptions = murderOptions.Value;
        _apiClient = wassengerClient;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute()
    {
        var allGroups = await _apiClient.GetGroups();

        //If the murder bot has been removed from the group mark it as ignore
        var existingGroups = _dbContext.Group.Where(i => i.Ignore == false).ToList();

        foreach (var eDbGroup in existingGroups)
        {
            var id = eDbGroup.WId;
            if (allGroups.All(g => g.Wid != id))
            {
                eDbGroup.Ignore = true;
                await _dbContext.SaveChangesAsync();
            }
        }
        

        foreach (var groupListResult in allGroups)
        {
            
            
            try
            {
                if (groupListResult.IsCommunityAnnounce)
                    continue;
                
                _logger.LogInformation($"Starting group {groupListResult.Name} : {groupListResult.Wid}");
                
                var id = groupListResult.Wid;
                var existingGroup = _dbContext.Group.FirstOrDefault(g => g.WId == id);
                if (existingGroup != null)
                {
                    if (existingGroup.Ignore)
                    {
                        _logger.LogInformation("Group set to ignore");
                        continue;
                    }

                    var update = false;

                    if (existingGroup.Name != groupListResult.Name)
                    {
                        existingGroup.Name = groupListResult.Name;
                        update = true;
                    }

                    if (existingGroup.Description != groupListResult.Description)
                    {
                        existingGroup.Description = groupListResult.Description;
                        update = true;
                    }

                    if (update)
                        await _dbContext.SaveChangesAsync();
                
                }
                else
                {
                    _logger.LogInformation($"Group {groupListResult.Name} is a new group, adding to DB");
                    var newGroup = new Group
                    {
                        Name = groupListResult.Name,
                        Description = groupListResult.Description,
                        WId = groupListResult.Wid,
                        CreatedAt = groupListResult.CreatedAt ?? DateTimeOffset.Now,
                        Ignore = false,
                        DoMurders = false,
                        IsBotAdmin = false,
                        LastMessageExemptTime = _murderOptions.DefaultLastMessageExemptTime,
                        MinimumTimeBetweenRuns = _murderOptions.DefaultMinimumTimeBetweenRuns,
                        CheckInMessageResponseTimeout = _murderOptions.DefaultCheckInMessageResponseTimeout,
                        CheckInReadTimeout = _murderOptions.DefaultCheckInReadTimeout
                    };
                    _dbContext.Group.Add(newGroup);
                    await  _dbContext.SaveChangesAsync();
                    existingGroup = newGroup;
                }
            
                var isBotAdmin = await SyncParticipants(groupListResult.Wid);
                existingGroup.IsBotAdmin = isBotAdmin;
                existingGroup.LastParticipantSync = DateTimeOffset.Now;
                await _dbContext.SaveChangesAsync();
                
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"Unable to process group ID {groupListResult.Wid}");
            }
        }
    }

    public bool CascadeFailure => false;


    /// <summary>
    /// </summary>
    /// <param name="groupId"></param>
    /// <returns>Returns true if the bot is an admin of the group</returns>
    private async Task<bool> SyncParticipants(string groupId)
    {
        var newCount = 0;
        var removeCount = 0;
        
        var result = false;
        var groupResult = await _apiClient.GetGroupById(groupId);
        
        var dbGroup = _dbContext.Group.Include(g => g.GroupParticipants)
            .Single(g => g.WId == groupId);

        var dbParticipants = await _dbContext.GroupParticipant.Where(gp => gp.GroupId == groupId)
            .Select(gp => gp.Participant).ToListAsync();
        
        foreach (var gpr in (groupResult.Participants ?? new List<GroupParticipantResult>()))
        {
            if (gpr.Wid == _murderOptions.BotWid)
            {
                result = gpr.IsAdmin;
            }
            
            var dbParticipant = await GetOrCreateDbParticipant(gpr, dbParticipants);
            var dbGroupParticipant = dbGroup.GroupParticipants.SingleOrDefault(p => p.ParticipantId == gpr.Wid);
            if (dbGroupParticipant == null)
            {
                dbGroupParticipant = new GroupParticipant
                {
                    ParticipantId = dbParticipant.WId,
                    GroupId = gpr.Wid,
                    IsAdmin = gpr.IsAdmin,
                    IsOwner = gpr.IsOwner,
                };
                dbGroup.GroupParticipants.Add(dbGroupParticipant);
                await  _dbContext.SaveChangesAsync();
                newCount++;
            }
            else
            {
                if (dbGroupParticipant.IsAdmin != gpr.IsAdmin || 
                    dbGroupParticipant.IsOwner != gpr.IsOwner)
                {
                    dbGroupParticipant.IsAdmin = gpr.IsAdmin;
                    dbGroupParticipant.IsOwner = gpr.IsOwner;
                    await  _dbContext.SaveChangesAsync();
                }
            }
            
        }

        if (groupResult.Participants != null && groupResult.Participants.Count > 0)
        {
            var doUpdate = false;
            var dbParticpants = dbGroup.GroupParticipants.ToList();
            foreach (var dbGp in dbParticpants)
            {
                var p = groupResult.Participants.FirstOrDefault(gr => gr.Wid == dbGp.ParticipantId);
                
                if (p == null)
                {
                    _dbContext.GroupParticipant.Remove(dbGp);
                    doUpdate = true;
                    removeCount++;
                }

            }

            if (doUpdate)
                await _dbContext.SaveChangesAsync();
        }
        _logger.LogInformation($"Found {newCount} new participants. Removed {removeCount} participants");

        return result;

    }

    private async Task<Participant> GetOrCreateDbParticipant(GroupParticipantResult gpr, List<Participant> dbParticipants)
    {
        var dbParticipant = dbParticipants.SingleOrDefault(p => p.WId == gpr.Wid);

        if (dbParticipant == null)
        {
            dbParticipant = _dbContext.Participant.SingleOrDefault(p => p.WId == gpr.Wid);
        }

        if (dbParticipant == null)
        {
            dbParticipant = new Participant
            {
                WId = gpr.Wid,
                Phone = gpr.Phone,
                DateCreated = DateTimeOffset.Now
            };
            _dbContext.Participant.Add(dbParticipant);
            await _dbContext.SaveChangesAsync();
        }
        else
        {
            if (dbParticipant.Phone != gpr.Phone)
            {
                dbParticipant.Phone = gpr.Phone;
                await _dbContext.SaveChangesAsync();
            }
        }
        
        return dbParticipant;
        
        
    }
    
    
    
    
}