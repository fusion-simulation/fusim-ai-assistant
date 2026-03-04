using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public interface ICaseDetailChatAgentService
{
    Task<CaseAgentChatResponse> ChatAsync(int caseId, string message, CancellationToken cancellationToken = default);
}
