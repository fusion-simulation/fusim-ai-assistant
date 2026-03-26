using FusimAiAssiant.Services;
using Xunit;

namespace FusimAiAssiant.Tests;

public sealed class CaseImagePreviewStateTests
{
    [Fact]
    public void Open_WithValidImageUrl_StoresUrlAndMarksPreviewOpen()
    {
        var state = new CaseImagePreviewState();

        state.Open("/api/vmom/cases/1/plots/a.png");

        Assert.True(state.IsOpen);
        Assert.Equal("/api/vmom/cases/1/plots/a.png", state.ImageUrl);
    }

    [Fact]
    public void Close_AfterOpen_ClearsUrlAndMarksPreviewClosed()
    {
        var state = new CaseImagePreviewState();
        state.Open("/api/vmom/cases/1/plots/a.png");

        state.Close();

        Assert.False(state.IsOpen);
        Assert.Null(state.ImageUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Open_WithBlankImageUrl_KeepsPreviewClosed(string? imageUrl)
    {
        var state = new CaseImagePreviewState();

        state.Open(imageUrl);

        Assert.False(state.IsOpen);
        Assert.Null(state.ImageUrl);
    }
}
