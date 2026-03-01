using FusimAiAssiant.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace FusimAiAssiant.Services;

public class CaseStatusBroadcastService : BackgroundService
{
    private static readonly TimeSpan BroadcastInterval = TimeSpan.FromSeconds(1);

    private readonly IVmomCaseService _caseService;
    private readonly IHubContext<CaseStatusHub> _hubContext;
    private readonly ILogger<CaseStatusBroadcastService> _logger;

    public CaseStatusBroadcastService(
        IVmomCaseService caseService,
        IHubContext<CaseStatusHub> hubContext,
        ILogger<CaseStatusBroadcastService> logger)
    {
        _caseService = caseService;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(BroadcastInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast case status updates.");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task BroadcastOnceAsync(CancellationToken cancellationToken)
    {
        var (overview, cases) = await _caseService.GetBroadcastPayloadAsync(cancellationToken);

        await _hubContext.Clients.Group(CaseStatusHub.OverviewGroup)
            .SendAsync("OverviewUpdated", overview, cancellationToken);

        await _hubContext.Clients.Group(CaseStatusHub.CasesGroup)
            .SendAsync("CasesUpdated", cases, cancellationToken);
    }
}
