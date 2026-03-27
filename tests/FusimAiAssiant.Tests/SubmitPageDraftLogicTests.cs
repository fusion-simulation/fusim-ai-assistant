using FusimAiAssiant.Models;
using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class SubmitPageDraftLogicTests
{
    [Fact]
    public void CanSwitchToFormMode_ReturnsFalse_WhenRawInputHasContentButNoParsedFields()
    {
        var draft = new VmomInputDraft(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
            "rmajor = 7.9,",
            []);

        var result = SubmitPageDraftLogic.CanSwitchToFormMode("rmajor = 7.9,", draft);

        Assert.False(result);
    }

    [Fact]
    public void CanSwitchToFormMode_ReturnsTrue_WhenParsedFieldsExist()
    {
        var draft = new VmomInputDraft(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rmajor"] = "7.9"
            },
            "&eqinpt\nrmajor = 7.9,\n/",
            []);

        var result = SubmitPageDraftLogic.CanSwitchToFormMode(draft.InputContent, draft);

        Assert.True(result);
    }

    [Fact]
    public void BuildFormSubmissionFields_PreservesExtraDraftKeys()
    {
        var result = SubmitPageDraftLogic.BuildFormSubmissionFields(new Dictionary<string, string>
        {
            ["rmajor"] = "7.9",
            ["custom_key"] = "42"
        });

        Assert.Equal("7.9", result["rmajor"]);
        Assert.Equal("42", result["custom_key"]);
    }

    [Fact]
    public void CountAppliedChanges_CountsOnlyAcceptedChanges()
    {
        var currentDraft = new VmomInputDraft(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rmajor"] = "7.9"
            },
            "&eqinpt\nrmajor = 7.9,\n/",
            []);
        var updatedDraft = new VmomInputDraft(
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["rmajor"] = "8.1"
            },
            "&eqinpt\nrmajor = 8.1,\n/",
            ["missing_key"]);

        var count = SubmitPageDraftLogic.CountAppliedChanges(
            currentDraft,
            updatedDraft,
            [
                new SubmitAgentProposedChange("rmajor", "7.9", "8.1", "valid"),
                new SubmitAgentProposedChange("missing_key", "", "1", "rejected")
            ]);

        Assert.Equal(1, count);
    }
}
