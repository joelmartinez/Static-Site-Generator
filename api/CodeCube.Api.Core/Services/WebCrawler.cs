using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Models;

namespace CodeCube.Api.Core.Services;

/// <summary>
/// Main web crawler that orchestrates sitemap parsing and content extraction
/// </summary>
public class WebCrawler : IWebCrawler
{
    private readonly ISitemapParser _sitemapParser;
    private readonly IContentExtractor _contentExtractor;

    public WebCrawler(ISitemapParser sitemapParser, IContentExtractor contentExtractor)
    {
        _sitemapParser = sitemapParser ?? throw new ArgumentNullException(nameof(sitemapParser));
        _contentExtractor = contentExtractor ?? throw new ArgumentNullException(nameof(contentExtractor));
    }

    public async Task<IEnumerable<PageContent>> CrawlSitemapAsync(string sitemapUrl)
    {
        if (string.IsNullOrWhiteSpace(sitemapUrl))
            throw new ArgumentException("Sitemap URL cannot be null or empty", nameof(sitemapUrl));

        // Parse the sitemap to get all URLs
        var sitemapEntries = await _sitemapParser.ParseSitemapAsync(sitemapUrl);
        var urls = sitemapEntries.Select(entry => entry.Url);

        // Crawl all URLs found in the sitemap
        return await CrawlUrlsAsync(urls);
    }

    public async Task<IEnumerable<PageContent>> CrawlUrlsAsync(IEnumerable<string> urls)
    {
        if (urls == null)
            throw new ArgumentNullException(nameof(urls));

        var urlList = urls.Where(url => !string.IsNullOrWhiteSpace(url)).ToList();
        if (!urlList.Any())
            return Enumerable.Empty<PageContent>();

        var results = new List<PageContent>();
        var semaphore = new SemaphoreSlim(5, 5); // Limit concurrent requests

        var tasks = urlList.Select(async url =>
        {
            await semaphore.WaitAsync();
            try
            {
                return await _contentExtractor.ExtractContentAsync(url);
            }
            catch (Exception ex)
            {
                // Log error but continue with other URLs
                Console.WriteLine($"Error crawling {url}: {ex.Message}");
                return null;
            }
            finally
            {
                semaphore.Release();
            }
        });

        var crawledPages = await Task.WhenAll(tasks);
        results.AddRange(crawledPages.Where(page => page != null)!);

        return results;
    }
}