namespace FusimAiAssiant.Services;

public sealed class CaseImagePreviewState
{
    public string? ImageUrl { get; private set; }

    public bool IsOpen => !string.IsNullOrWhiteSpace(ImageUrl);

    public void Open(string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
        {
            return;
        }

        ImageUrl = imageUrl;
    }

    public void Close()
    {
        ImageUrl = null;
    }
}
