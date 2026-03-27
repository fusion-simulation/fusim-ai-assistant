using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class SubmitAgentSuggestionViewBuilderTests
{
    [Fact]
    public void Build_ProducesSelectableSuggestions_FromStructuredChanges()
    {
        var items = SubmitAgentSuggestionViewBuilder.Build(
            [
                new SubmitAgentProposedChange("rmajor", "7.9", "8.1", "增大主半径")
            ]);

        Assert.Single(items);
        Assert.Equal("rmajor", items[0].FieldKey);
        Assert.True(items[0].IsSelected);
    }
}
