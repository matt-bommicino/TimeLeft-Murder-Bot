using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MurderBot.Infrastructure.Routines;
using MurderBot.Infrastructure.WassengerClient;

namespace MurderBot.WebJob;

public class MurderBotService
{
    private readonly ILogger<MurderBotService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly UpdateGroupsRoutine _updateGroupsRoutine;
    private readonly AlwaysBanRoutine _alwaysBanRoutine;
    private readonly GroupMurderRoutine _groupMurderRoutine;

    public MurderBotService(ILogger<MurderBotService> logger,
        IServiceScopeFactory serviceScopeFactory,
        UpdateGroupsRoutine updateGroupsRoutine,
        AlwaysBanRoutine alwaysBanRoutine,
        GroupMurderRoutine groupMurderRoutine)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _updateGroupsRoutine = updateGroupsRoutine;
        _alwaysBanRoutine = alwaysBanRoutine;
        _groupMurderRoutine = groupMurderRoutine;
    }
    
    public async Task RunAsync()
    {
        await ExecuteServiceRoutine(_updateGroupsRoutine);
        await ExecuteServiceRoutine(_alwaysBanRoutine);
        await ExecuteServiceRoutine(_groupMurderRoutine);
    }


    private async Task ExecuteServiceRoutine(IServiceRoutine routine)
    {
        var name = routine.GetType().Name;
        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _logger.LogInformation($"Executing Service Routine: {name}");
                await routine.Execute();
                _logger.LogInformation($"Routine Completed: {name}");
            }
        }
        catch (Exception e)
        {
            
            _logger.LogError(e, $"Failed to execute service routine: {name}");

            if (routine.CascadeFailure)
                throw;
        }
    }
}