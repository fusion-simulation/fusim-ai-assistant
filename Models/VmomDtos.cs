namespace FusimAiAssiant.Models;

public record LoginRequest(string Username, string Password);

public record LoginResponse(bool Success, string Message, int UserId, string Username);

public record CurrentUserResponse(bool IsAuthenticated, int UserId, string Username);

public record VmomInputCatalogResponse(Dictionary<string, string> Fields, string TemplateInput);

public record CreateCaseRequest(string Title, string InputContent);

public record CreateCaseFromFormRequest(string Title, Dictionary<string, string> Fields);

public record CaseListItem(
    int Id,
    string Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ErrorMessage);

public record CaseOverviewResponse(
    int TotalCount,
    int RunningCount,
    int SuccessCount,
    int FailedCount,
    IReadOnlyList<CaseListItem> RecentCases);

public record VmomCaseDetail(
    int Id,
    int UserId,
    string Title,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? ErrorMessage,
    string InputContent,
    string RzText,
    string EqprIotaText,
    string VmomOutText);

public record VmomCaseWorkspace(
    int Id,
    string Status,
    string WorkDirectory,
    IReadOnlyList<string> Files);

public record CaseAgentChatMessage(
    string Role,
    string Content,
    string? ImageUrl);

public record CaseAgentChatRequest(
    string Message,
    IReadOnlyList<CaseAgentChatMessage>? History);

public record CaseAgentChatResponse(
    string Answer,
    string? ImageUrl,
    string? ImageFileName,
    IReadOnlyList<string>? AvailableVariables,
    IReadOnlyList<string>? AvailableFiles);
