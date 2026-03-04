namespace FusimAiAssiant.Services;

public sealed class SemanticKernelOptions
{
    public const string SectionPath = "SemanticKernel:OpenAI";

    public string BaseUrl { get; init; } = string.Empty;

    public string ModelId { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;
}
