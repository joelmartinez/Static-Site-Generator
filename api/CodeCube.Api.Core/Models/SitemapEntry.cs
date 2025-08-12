namespace CodeCube.Api.Core.Models;

/// <summary>
/// Represents a URL entry from a sitemap
/// </summary>
public class SitemapEntry
{
    public string Url { get; set; } = string.Empty;
    public DateTime? LastModified { get; set; }
    public string? ChangeFrequency { get; set; }
    public double? Priority { get; set; }
}

/// <summary>
/// Represents the complete content of a web page
/// </summary>
public class PageContent
{
    public string Url { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
    public DateTime CrawledAt { get; set; } = DateTime.UtcNow;
}