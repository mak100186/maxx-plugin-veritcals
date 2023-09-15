namespace Maxx.PluginVerticals.Shared.Entities;

public class Article
{
    public Guid Id { get; set; }

    public int Stars { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? PublishedOnUtc { get; set; }
}
