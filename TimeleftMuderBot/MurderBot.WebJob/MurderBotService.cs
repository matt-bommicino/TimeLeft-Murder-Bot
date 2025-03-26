using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MuderBot.Infrastructure.Routines;
using MuderBot.Infrastructure.WassengerClient;

namespace MurderBot.WebJob;

public class MurderBotService
{
    private readonly WassengerClient _wassengerClient;
    private readonly ILogger<MurderBotService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UpdateGroupsRoutine _updateGroupsRoutine;

    public MurderBotService(WassengerClient wassengerClient, ILogger<MurderBotService> logger,
        IServiceScopeFactory serviceScopeFactory,
        UpdateGroupsRoutine updateGroupsRoutine)
    {
        _wassengerClient = wassengerClient;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _updateGroupsRoutine = updateGroupsRoutine;
    }
    
    public async Task RunAsync()
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            await _updateGroupsRoutine.Execute();
        }
    }
}