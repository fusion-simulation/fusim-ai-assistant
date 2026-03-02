using FusimAiAssiant.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class SemanticKernelRegistrationTests
{
    [Fact]
    public void AddSemanticKernelFoundation_Throws_WhenBaseUrlMissing()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SemanticKernel:OpenAI:BaseUrl"] = "",
            ["SemanticKernel:OpenAI:ModelId"] = "gpt-4o-mini",
            ["SemanticKernel:OpenAI:ApiKey"] = "test-key"
        });

        services.AddSemanticKernelFoundation(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SemanticKernelOptions>>();

        var exception = Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        Assert.Contains("BaseUrl", exception.Message);
    }

    [Fact]
    public void AddSemanticKernelFoundation_Throws_WhenBaseUrlIsNotAbsoluteUri()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SemanticKernel:OpenAI:BaseUrl"] = "not-a-valid-uri",
            ["SemanticKernel:OpenAI:ModelId"] = "gpt-4o-mini",
            ["SemanticKernel:OpenAI:ApiKey"] = "test-key"
        });

        services.AddSemanticKernelFoundation(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<SemanticKernelOptions>>();

        var exception = Assert.Throws<OptionsValidationException>(() => _ = options.Value);
        Assert.Contains("absolute URI", exception.Message);
    }

    [Fact]
    public async Task AddSemanticKernelFoundation_ThrowsOnHostStart_WhenBaseUrlMissing()
    {
        var hostBuilder = Host.CreateApplicationBuilder();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SemanticKernel:OpenAI:BaseUrl"] = "",
            ["SemanticKernel:OpenAI:ModelId"] = "gpt-4o-mini",
            ["SemanticKernel:OpenAI:ApiKey"] = "test-key"
        });

        hostBuilder.Services.AddSemanticKernelFoundation(configuration);

        using var host = hostBuilder.Build();
        var exception = await Assert.ThrowsAsync<OptionsValidationException>(() => host.StartAsync());
        Assert.Contains("BaseUrl", exception.Message);
    }

    [Fact]
    public void AddSemanticKernelFoundation_ResolvesKernel_WhenConfigurationIsValid()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["SemanticKernel:OpenAI:BaseUrl"] = "https://api.openai.com/v1",
            ["SemanticKernel:OpenAI:ModelId"] = "gpt-4o-mini",
            ["SemanticKernel:OpenAI:ApiKey"] = "test-key"
        });

        services.AddSemanticKernelFoundation(configuration);

        using var serviceProvider = services.BuildServiceProvider();
        var kernel = serviceProvider.GetRequiredService<Kernel>();

        Assert.NotNull(kernel);
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();
    }
}
