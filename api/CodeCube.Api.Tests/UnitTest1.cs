using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Models;
using CodeCube.Api.Core.Services;
using Xunit;

namespace CodeCube.Api.Tests;

/// <summary>
/// Mock HTTP client for testing
/// </summary>
public class MockHttpClient : IHttpClient
{
    private readonly Dictionary<string, string> _responses = new();

    public void SetResponse(string url, string content)
    {
        _responses[url] = content;
    }

    public Task<string> GetStringAsync(string url)
    {
        if (_responses.TryGetValue(url, out var content))
        {
            return Task.FromResult(content);
        }
        throw new HttpRequestException($"No mock response configured for URL: {url}");
    }

    public Task<Stream> GetStreamAsync(string url)
    {
        if (_responses.TryGetValue(url, out var content))
        {
            var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            return Task.FromResult<Stream>(stream);
        }
        throw new HttpRequestException($"No mock response configured for URL: {url}");
    }
}

public class SitemapParserTests
{
    private readonly MockHttpClient _mockHttpClient;
    private readonly SitemapParser _sitemapParser;

    public SitemapParserTests()
    {
        _mockHttpClient = new MockHttpClient();
        _sitemapParser = new SitemapParser(_mockHttpClient);
    }

    [Fact]
    public async Task ParseSitemapAsync_ValidSitemap_ReturnsExpectedEntries()
    {
        // Arrange
        var sitemapUrl = "https://example.com/sitemap.xml";
        var sitemapXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url>
                <loc>https://example.com/</loc>
                <lastmod>2025-01-15</lastmod>
                <changefreq>weekly</changefreq>
                <priority>1.0</priority>
              </url>
              <url>
                <loc>https://example.com/about</loc>
                <lastmod>2025-01-10</lastmod>
                <changefreq>monthly</changefreq>
                <priority>0.8</priority>
              </url>
            </urlset>
            """;
        
        _mockHttpClient.SetResponse(sitemapUrl, sitemapXml);

        // Act
        var entries = await _sitemapParser.ParseSitemapAsync(sitemapUrl);
        var entriesList = entries.ToList();

        // Assert
        Assert.Equal(2, entriesList.Count);
        
        var firstEntry = entriesList[0];
        Assert.Equal("https://example.com/", firstEntry.Url);
        Assert.Equal(new DateTime(2025, 1, 15), firstEntry.LastModified);
        Assert.Equal("weekly", firstEntry.ChangeFrequency);
        Assert.Equal(1.0, firstEntry.Priority);

        var secondEntry = entriesList[1];
        Assert.Equal("https://example.com/about", secondEntry.Url);
        Assert.Equal(new DateTime(2025, 1, 10), secondEntry.LastModified);
        Assert.Equal("monthly", secondEntry.ChangeFrequency);
        Assert.Equal(0.8, secondEntry.Priority);
    }

    [Fact]
    public async Task ParseSitemapFromContentAsync_MinimalSitemap_ReturnsBasicEntries()
    {
        // Arrange
        var sitemapXml = """
            <?xml version="1.0" encoding="UTF-8"?>
            <urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
              <url>
                <loc>https://example.com/</loc>
              </url>
            </urlset>
            """;

        // Act
        var entries = await _sitemapParser.ParseSitemapFromContentAsync(sitemapXml);
        var entriesList = entries.ToList();

        // Assert
        Assert.Single(entriesList);
        Assert.Equal("https://example.com/", entriesList[0].Url);
        Assert.Null(entriesList[0].LastModified);
        Assert.Null(entriesList[0].ChangeFrequency);
        Assert.Null(entriesList[0].Priority);
    }

    [Fact]
    public async Task ParseSitemapFromContentAsync_EmptyContent_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _sitemapParser.ParseSitemapFromContentAsync(""));
    }

    [Fact]
    public async Task ParseSitemapFromContentAsync_InvalidXml_ThrowsException()
    {
        // Arrange
        var invalidXml = "<invalid>xml content";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _sitemapParser.ParseSitemapFromContentAsync(invalidXml));
    }
}