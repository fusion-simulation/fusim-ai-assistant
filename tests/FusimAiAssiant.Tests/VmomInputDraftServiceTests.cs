using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class VmomInputDraftServiceTests
{
    [Fact]
    public void ParseEqinpt_ParsesScalarAndArrayFields()
    {
        var service = new VmomInputDraftService(new VmomNamelistBuilder());

        var result = service.ParseEqinpt(
            """
            &eqinpt
            rmajor = 7.9,
            eqiotb = 0.1, 0.2, 0.3,
            /
            """);

        Assert.Equal("7.9", result.Fields["rmajor"]);
        Assert.Equal("0.1, 0.2, 0.3", result.Fields["eqiotb"]);
    }

    [Fact]
    public void ApplyChanges_UpdatesKnownFields_AndRebuildsNormalizedInput()
    {
        var service = new VmomInputDraftService(new VmomNamelistBuilder());
        var draft = service.CreateDraftFromFields(new Dictionary<string, string>
        {
            ["rmajor"] = "7.9",
            ["elong"] = "1.5"
        });

        var result = service.ApplyChanges(
            draft,
            [
                new SubmitAgentProposedChange("rmajor", "7.9", "8.1", "increase major radius"),
                new SubmitAgentProposedChange("elong", "1.5", "1.6", "increase elongation")
            ]);

        Assert.Equal("8.1", result.Fields["rmajor"]);
        Assert.Equal("1.6", result.Fields["elong"]);
        Assert.Contains("rmajor = 8.1,", result.InputContent, StringComparison.Ordinal);
        Assert.Contains("elong = 1.6,", result.InputContent, StringComparison.Ordinal);
    }

    [Fact]
    public void ApplyChanges_IgnoresUnknownFields_AndReportsRejectedKeys()
    {
        var service = new VmomInputDraftService(new VmomNamelistBuilder());
        var draft = service.CreateDraftFromFields(new Dictionary<string, string>
        {
            ["rmajor"] = "7.9"
        });

        var result = service.ApplyChanges(
            draft,
            [new SubmitAgentProposedChange("unknown_key", "", "1", "invalid key")]);

        Assert.Equal("7.9", result.Fields["rmajor"]);
        Assert.Contains("unknown_key", result.RejectedKeys);
    }
}
