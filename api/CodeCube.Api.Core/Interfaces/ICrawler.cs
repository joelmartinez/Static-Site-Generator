using CodeCube.Api.Core.Models;

namespace CodeCube.Api.Core.Interfaces;

/// <summary>
/// Interface for sitemap parsing operations
/// </summary>
public interface ISitemapParser
{
    Task<IEnumerable<SitemapEntry>> ParseSitemapAsync(string sitemapUrl);
    Task<IEnumerable<SitemapEntry>> ParseSitemapFromContentAsync(string sitemapXml);
}

/// <summary>
/// Interface for web page content extraction
/// </summary>
public interface IContentExtractor
{
    Task<PageContent> ExtractContentAsync(string url);
    Task<PageContent> ExtractContentFromHtmlAsync(string url, string html);
}

/// <summary>
/// Main crawler interface that orchestrates sitemap parsing and content extraction
/// </summary>
public interface IWebCrawler
{
    Task<IEnumerable<PageContent>> CrawlSitemapAsync(string sitemapUrl);
    Task<IEnumerable<PageContent>> CrawlUrlsAsync(IEnumerable<string> urls);
}