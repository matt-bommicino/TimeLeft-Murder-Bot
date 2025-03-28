using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MurderBot.Data.Context;
using MurderBot.Data.Models;
using MurderBot.Infrastructure.Settings;
using MurderBot.Infrastructure.Utility;

namespace MurderBot.Infrastructure.Routines;

public class AlwaysBanRoutine : IServiceRoutine
{
    private readonly CommonMurderSettings _murderOptions;
    private readonly WassengerClient.WassengerClient _apiClient;
    private readonly MurderContext _dbContext;
    private readonly ILogger<AlwaysBanRoutine> _logger;
    private readonly MurderUtil _murderUtil;

    public AlwaysBanRoutine(IOptions<CommonMurderSettings> murderOptions, 
        WassengerClient.WassengerClient apiClient,
        MurderContext dbContext, ILogger<AlwaysBanRoutine> logger, MurderUtil murderUtil)
    {
        _murderOptions = murderOptions.Value;
        _apiClient = apiClient;
        _dbContext = dbContext;
        _logger = logger;
        _murderUtil = murderUtil;
    }

    public async Task Execute()
    {
        //this is needed to avoid attempts to remove participants over and over again
        //in the case a sync fails
        
        var syncCutOff = DateTimeOffset.Now.AddMinutes(-30);
        var alwaysBan = _dbContext.AlwaysRemoveParticipant.ToList();

        if (alwaysBan.Count == 0)
        {
            _logger.LogInformation("No always ban entries found");
            return;
        }

        var groups = _dbContext.Group.Where(g => g.Ignore == false &&
                                                 g.LastParticipantSync > syncCutOff).ToList();

        foreach (var arp in alwaysBan)
        {
            try
            {
                if (arp.GroupId == null)
                {
                    var gps = await _dbContext.GroupParticipant.Where(p => p.ParticipantId == arp.ParticipantId).ToListAsync();
                    foreach (var gp in gps)
                    {
                        if (groups.All(g => g.WId != gp.GroupId))
                            continue;

                        await _murderUtil.RemoveGroupParticipant(gp.GroupId, gp.ParticipantId);

                    }
                
                }
                else
                {
                    var group = groups.SingleOrDefault(g => g.WId == arp.GroupId);
                    if (group == null)
                        continue;
                    
                    var gp = await _dbContext.GroupParticipant.Where(p => p.ParticipantId == arp.ParticipantId
                     && p.GroupId == arp.GroupId).FirstOrDefaultAsync();

                    if (gp != null)
                    {
                        await _murderUtil.RemoveGroupParticipant(gp.GroupId, gp.ParticipantId);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Could not process always ban for id {arp.AlwaysRemoveParticipantId} - {arp.ParticipantId}");
            }
        }
        
        
    }

    public bool CascadeFailure => false;
}