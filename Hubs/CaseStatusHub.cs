using Microsoft.AspNetCore.SignalR;

namespace FusimAiAssiant.Hubs;

public class CaseStatusHub : Hub
{
    public const string OverviewGroup = "overview";
    public const string CasesGroup = "cases";

    public Task SubscribeOverview()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, OverviewGroup);
    }

    public Task SubscribeCases()
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, CasesGroup);
    }
}
