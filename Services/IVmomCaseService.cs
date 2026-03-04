using FusimAiAssiant.Models;

namespace FusimAiAssiant.Services;

public interface IVmomCaseService
{
    Task<int> CreateCaseAsync(string title, string inputContent, CancellationToken cancellationToken = default);

    Task<int> CreateCaseFromFormAsync(string title, IReadOnlyDictionary<string, string> fields, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CaseListItem>> ListCasesAsync(CancellationToken cancellationToken = default);

    Task<CaseOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

    Task<(CaseOverviewResponse Overview, IReadOnlyList<CaseListItem> Cases)> GetBroadcastPayloadAsync(CancellationToken cancellationToken = default);

    Task<VmomCaseDetail?> GetCaseDetailAsync(int caseId, CancellationToken cancellationToken = default);

    Task<VmomCaseWorkspace?> GetCaseWorkspaceAsync(int caseId, CancellationToken cancellationToken = default);

    Task<(byte[] Content, string FileName)?> GetCaseZipAsync(int caseId, CancellationToken cancellationToken = default);
}
