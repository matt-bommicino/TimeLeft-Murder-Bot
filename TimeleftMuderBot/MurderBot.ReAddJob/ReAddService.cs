using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MurderBot.Infrastructure.Routines;

namespace MurderBot.ReAddJob;

public class ReAddService
{
    private readonly ILogger<ReAddService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ReAddRoutine _reAddRoutine;

    public ReAddService(ILogger<ReAddService> logger,
        IServiceScopeFactory serviceScopeFactory,
        ReAddRoutine reAddRoutine)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _reAddRoutine = reAddRoutine;
    }
    public async Task RunAsync()
    {
        try
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                _logger.LogInformation($"Executing ReAdd Routine");
                await _reAddRoutine.Execute();
                _logger.LogInformation($"ReAdd Routine Completed");
            }
        }
        catch (Exception e)
        {
           _logger.LogError(e, "ReAdd Routine Failed");
        }
    }
}