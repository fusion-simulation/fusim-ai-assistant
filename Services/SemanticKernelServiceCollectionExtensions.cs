using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace FusimAiAssiant.Services;

public static class SemanticKernelServiceCollectionExtensions
{
    public static IServiceCollection AddSemanticKernelFoundation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SemanticKernelOptions>()
            .Bind(configuration.GetSection(SemanticKernelOptions.SectionPath))
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BaseUrl),
                $"{nameof(SemanticKernelOptions.BaseUrl)} is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ModelId),
                $"{nameof(SemanticKernelOptions.ModelId)} is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ApiKey),
                $"{nameof(SemanticKernelOptions.ApiKey)} is required.")
            .Validate(
                options => Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out _),
                $"{nameof(SemanticKernelOptions.BaseUrl)} must be an absolute URI.")
            .ValidateOnStart();

#pragma warning disable SKEXP0010
        services.AddSingleton<IChatCompletionService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<SemanticKernelOptions>>().Value;
            return new OpenAIChatCompletionService(
                modelId: options.ModelId,
                apiKey: options.ApiKey,
                endpoint: new Uri(options.BaseUrl));
        });
#pragma warning restore SKEXP0010

        services.AddSingleton<Kernel>(sp => new Kernel(sp));

        return services;
    }
}
