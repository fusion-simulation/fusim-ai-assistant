using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public interface ISubmitParameterChatAgentService
{
    Task<SubmitAgentChatResponse> ChatAsync(
        SubmitAgentChatRequest request,
        CancellationToken cancellationToken = default);
}
