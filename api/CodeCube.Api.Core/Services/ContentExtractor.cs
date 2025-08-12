using HtmlAgilityPack;
using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Models;

namespace CodeCube.Api.Core.Services;

/// <summary>
/// Extracts content from web pages using HTML parsing
/// </summary>
public class ContentExtractor : IContentExtractor
{
    private readonly IHttpClient _httpClient;

    public ContentExtractor(IHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<PageContent> ExtractContentAsync(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));

        var html = await _httpClient.GetStringAsync(url);
        return await ExtractContentFromHtmlAsync(url, html);
    }

    public async Task<PageContent> ExtractContentFromHtmlAsync(string url, string html)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty", nameof(url));
        
        if (string.IsNullOrWhiteSpace(html))
            throw new ArgumentException("HTML content cannot be null or empty", nameof(html));

        return await Task.Run(() =>
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var pageContent = new PageContent
            {
                Url = url,
                CrawledAt = DateTime.UtcNow
            };

            // Extract title
            var titleNode = doc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
            {
                pageContent.Title = HtmlEntity.DeEntitize(titleNode.InnerText).Trim();
            }

            // Extract meta description
            var metaDescNode = doc.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (metaDescNode != null)
            {
                pageContent.MetaDescription = metaDescNode.GetAttributeValue("content", "");
            }

            // Extract main content
            pageContent.Content = ExtractMainContent(doc);

            // Extract additional metadata
            ExtractMetadata(doc, pageContent.Metadata);

            return pageContent;
        });
    }

    private static string ExtractMainContent(HtmlDocument doc)
    {
        // Try to find main content areas in order of preference
        var contentSelectors = new[]
        {
            "//main",
            "//article", 
            "//*[@class='content']",
            "//*[@id='content']",
            "//*[@class='post-content']",
            "//*[@class='entry-content']",
            "//body"
        };

        foreach (var selector in contentSelectors)
        {
            var contentNode = doc.DocumentNode.SelectSingleNode(selector);
            if (contentNode != null)
            {
                return CleanText(contentNode.InnerText);
            }
        }

        // Fallback to body if nothing else found
        var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
        return bodyNode != null ? CleanText(bodyNode.InnerText) : "";
    }

    private static void ExtractMetadata(HtmlDocument doc, Dictionary<string, string> metadata)
    {
        // Extract Open Graph metadata
        var ogNodes = doc.DocumentNode.SelectNodes("//meta[@property and starts-with(@property, 'og:')]");
        if (ogNodes != null)
        {
            foreach (var node in ogNodes)
            {
                var property = node.GetAttributeValue("property", "");
                var content = node.GetAttributeValue("content", "");
                if (!string.IsNullOrEmpty(property) && !string.IsNullOrEmpty(content))
                {
                    metadata[property] = content;
                }
            }
        }

        // Extract Twitter Card metadata  
        var twitterNodes = doc.DocumentNode.SelectNodes("//meta[@name and starts-with(@name, 'twitter:')]");
        if (twitterNodes != null)
        {
            foreach (var node in twitterNodes)
            {
                var name = node.GetAttributeValue("name", "");
                var content = node.GetAttributeValue("content", "");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
                {
                    metadata[name] = content;
                }
            }
        }

        // Extract canonical URL
        var canonicalNode = doc.DocumentNode.SelectSingleNode("//link[@rel='canonical']");
        if (canonicalNode != null)
        {
            var href = canonicalNode.GetAttributeValue("href", "");
            if (!string.IsNullOrEmpty(href))
            {
                metadata["canonical"] = href;
            }
        }
    }

    private static string CleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "";

        // Decode HTML entities and normalize whitespace
        var cleaned = HtmlEntity.DeEntitize(text);
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ");
        return cleaned.Trim();
    }
}