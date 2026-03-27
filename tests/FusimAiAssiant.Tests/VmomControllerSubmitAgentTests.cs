using FusimAiAssiant.Controllers;
using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class VmomControllerSubmitAgentTests
{
    [Fact]
    public async Task ChatSubmitAgent_ReturnsBadRequest_WhenMessageMissing()
    {
        var controller = CreateController(new FakeSubmitParameterChatAgentService());

        var result = await controller.ChatSubmitAgent(
            new SubmitAgentChatRequest("ByForm", "case-a", null, new Dictionary<string, string>(), "", null),
            CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task ChatSubmitAgent_ForwardsRequestToService_AndReturnsResponse()
    {
        var submitAgentService = new FakeSubmitParameterChatAgentService();
        var controller = CreateController(submitAgentService);
        var request = new SubmitAgentChatRequest(
            "ByInputFile",
            "case-b",
            "&eqinpt\nrmajor = 7.9,\n/",
            null,
            "请给出建议",
            []);

        var result = await controller.ChatSubmitAgent(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<SubmitAgentChatResponse>(ok.Value);
        Assert.Equal("建议已生成。", payload.Answer);
        Assert.Same(request, submitAgentService.LastRequest);
    }

    private static VmomController CreateController(ISubmitParameterChatAgentService submitAgentService)
    {
        return new VmomController(
            new FakeCaseService(),
            new VmomInputCatalogService(),
            new FakeCaseDetailChatAgentService(),
            submitAgentService);
    }

    private sealed class FakeSubmitParameterChatAgentService : ISubmitParameterChatAgentService
    {
        public SubmitAgentChatRequest? LastRequest { get; private set; }

        public Task<SubmitAgentChatResponse> ChatAsync(
            SubmitAgentChatRequest request,
            CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new SubmitAgentChatResponse(
                "建议已生成。",
                [],
                request.InputContent,
                request.Fields));
        }
    }

    private sealed class FakeCaseDetailChatAgentService : ICaseDetailChatAgentService
    {
        public Task<CaseAgentChatResponse> ChatAsync(
            int caseId,
            string message,
            IReadOnlyList<CaseAgentChatMessage>? history = null,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class FakeCaseService : IVmomCaseService
    {
        public Task<int> CreateCaseAsync(string title, string inputContent, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<int> CreateCaseFromFormAsync(string title, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<IReadOnlyList<CaseListItem>> ListCasesAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<CaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<(CaseOverviewResponse Overview, IReadOnlyList<CaseListItem> Cases)> GetBroadcastPayloadAsync(CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<VmomCaseDetail?> GetCaseDetailAsync(int caseId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<VmomCaseWorkspace?> GetCaseWorkspaceAsync(int caseId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<(byte[] Content, string FileName)?> GetCaseZipAsync(int caseId, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
