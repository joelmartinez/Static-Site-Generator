using CodeCube.Api.Core.Models;
using CodeCube.Api.Core.Services;
using Xunit;

namespace CodeCube.Api.Tests;

public class ContentExtractorTests
{
    private readonly MockHttpClient _mockHttpClient;
    private readonly ContentExtractor _contentExtractor;

    public ContentExtractorTests()
    {
        _mockHttpClient = new MockHttpClient();
        _contentExtractor = new ContentExtractor(_mockHttpClient);
    }

    [Fact]
    public async Task ExtractContentAsync_ValidHtml_ReturnsPageContent()
    {
        // Arrange
        var url = "https://example.com/test";
        var html = """
            <!DOCTYPE html>
            <html>
            <head>
                <title>Test Page</title>
                <meta name="description" content="This is a test page">
                <meta property="og:title" content="OG Test Page">
                <link rel="canonical" href="https://example.com/canonical">
            </head>
            <body>
                <main>
                    <h1>Welcome to Test Page</h1>
                    <p>This is the main content of the test page.</p>
                </main>
            </body>
            </html>
            """;
        
        _mockHttpClient.SetResponse(url, html);

        // Act
        var result = await _contentExtractor.ExtractContentAsync(url);

        // Assert
        Assert.Equal(url, result.Url);
        Assert.Equal("Test Page", result.Title);
        Assert.Equal("This is a test page", result.MetaDescription);
        Assert.Contains("Welcome to Test Page", result.Content);
        Assert.Contains("main content", result.Content);
        Assert.True(result.Metadata.ContainsKey("og:title"));
        Assert.Equal("OG Test Page", result.Metadata["og:title"]);
        Assert.True(result.Metadata.ContainsKey("canonical"));
        Assert.Equal("https://example.com/canonical", result.Metadata["canonical"]);
    }

    [Fact]
    public async Task ExtractContentFromHtmlAsync_MinimalHtml_ReturnsBasicContent()
    {
        // Arrange
        var url = "https://example.com/minimal";
        var html = "<html><body><p>Simple content</p></body></html>";

        // Act
        var result = await _contentExtractor.ExtractContentFromHtmlAsync(url, html);

        // Assert
        Assert.Equal(url, result.Url);
        Assert.Equal("", result.Title);
        Assert.Null(result.MetaDescription);
        Assert.Contains("Simple content", result.Content);
    }

    [Fact]
    public async Task ExtractContentFromHtmlAsync_EmptyUrl_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _contentExtractor.ExtractContentFromHtmlAsync("", "<html></html>"));
    }

    [Fact]
    public async Task ExtractContentFromHtmlAsync_EmptyHtml_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _contentExtractor.ExtractContentFromHtmlAsync("https://example.com", ""));
    }
}