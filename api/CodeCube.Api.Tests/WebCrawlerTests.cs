using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Models;
using CodeCube.Api.Core.Services;
using Moq;
using Xunit;

namespace CodeCube.Api.Tests;

public class WebCrawlerTests
{
    private readonly Mock<ISitemapParser> _mockSitemapParser;
    private readonly Mock<IContentExtractor> _mockContentExtractor;
    private readonly WebCrawler _webCrawler;

    public WebCrawlerTests()
    {
        _mockSitemapParser = new Mock<ISitemapParser>();
        _mockContentExtractor = new Mock<IContentExtractor>();
        _webCrawler = new WebCrawler(_mockSitemapParser.Object, _mockContentExtractor.Object);
    }

    [Fact]
    public async Task CrawlSitemapAsync_ValidSitemap_ReturnsPageContent()
    {
        // Arrange
        var sitemapUrl = "https://example.com/sitemap.xml";
        var sitemapEntries = new List<SitemapEntry>
        {
            new() { Url = "https://example.com/" },
            new() { Url = "https://example.com/about" }
        };

        var pageContents = new List<PageContent>
        {
            new() { Url = "https://example.com/", Title = "Home Page", Content = "Home content" },
            new() { Url = "https://example.com/about", Title = "About Page", Content = "About content" }
        };

        _mockSitemapParser
            .Setup(x => x.ParseSitemapAsync(sitemapUrl))
            .ReturnsAsync(sitemapEntries);

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/"))
            .ReturnsAsync(pageContents[0]);

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/about"))
            .ReturnsAsync(pageContents[1]);

        // Act
        var result = await _webCrawler.CrawlSitemapAsync(sitemapUrl);
        var resultList = result.ToList();

        // Assert
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, p => p.Url == "https://example.com/" && p.Title == "Home Page");
        Assert.Contains(resultList, p => p.Url == "https://example.com/about" && p.Title == "About Page");
        
        _mockSitemapParser.Verify(x => x.ParseSitemapAsync(sitemapUrl), Times.Once);
        _mockContentExtractor.Verify(x => x.ExtractContentAsync(It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CrawlUrlsAsync_ValidUrls_ReturnsPageContent()
    {
        // Arrange
        var urls = new[] { "https://example.com/page1", "https://example.com/page2" };
        var pageContent1 = new PageContent { Url = "https://example.com/page1", Title = "Page 1" };
        var pageContent2 = new PageContent { Url = "https://example.com/page2", Title = "Page 2" };

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/page1"))
            .ReturnsAsync(pageContent1);

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/page2"))
            .ReturnsAsync(pageContent2);

        // Act
        var result = await _webCrawler.CrawlUrlsAsync(urls);
        var resultList = result.ToList();

        // Assert
        Assert.Equal(2, resultList.Count);
        Assert.Contains(resultList, p => p.Url == "https://example.com/page1");
        Assert.Contains(resultList, p => p.Url == "https://example.com/page2");
    }

    [Fact]
    public async Task CrawlUrlsAsync_EmptyUrls_ReturnsEmpty()
    {
        // Act
        var result = await _webCrawler.CrawlUrlsAsync(new string[0]);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task CrawlSitemapAsync_EmptyUrl_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _webCrawler.CrawlSitemapAsync(""));
    }

    [Fact]
    public async Task CrawlUrlsAsync_NullUrls_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _webCrawler.CrawlUrlsAsync(null!));
    }

    [Fact]
    public async Task CrawlUrlsAsync_ErrorInContentExtraction_ContinuesWithOtherUrls()
    {
        // Arrange
        var urls = new[] { "https://example.com/good", "https://example.com/bad" };
        var goodPageContent = new PageContent { Url = "https://example.com/good", Title = "Good Page" };

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/good"))
            .ReturnsAsync(goodPageContent);

        _mockContentExtractor
            .Setup(x => x.ExtractContentAsync("https://example.com/bad"))
            .ThrowsAsync(new HttpRequestException("Page not found"));

        // Act
        var result = await _webCrawler.CrawlUrlsAsync(urls);
        var resultList = result.ToList();

        // Assert
        Assert.Single(resultList);
        Assert.Equal("https://example.com/good", resultList[0].Url);
    }
}