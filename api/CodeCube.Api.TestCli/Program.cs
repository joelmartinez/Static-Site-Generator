using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Services;

namespace CodeCube.Api.TestCli;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("CodeCube API Test CLI");
        Console.WriteLine("====================");

        if (args.Length == 0)
        {
            ShowUsage();
            return;
        }

        var sitemapUrl = args[0];
        
        Console.WriteLine($"Testing sitemap crawler with URL: {sitemapUrl}");
        Console.WriteLine();

        try
        {
            // Set up dependencies
            using var httpClient = new HttpClientWrapper();
            var sitemapParser = new SitemapParser(httpClient);
            var contentExtractor = new ContentExtractor(httpClient);
            var webCrawler = new WebCrawler(sitemapParser, contentExtractor);

            // Parse sitemap first
            Console.WriteLine("1. Parsing sitemap...");
            var sitemapEntries = await sitemapParser.ParseSitemapAsync(sitemapUrl);
            var entriesList = sitemapEntries.ToList();
            
            Console.WriteLine($"   Found {entriesList.Count} URLs in sitemap");
            
            if (entriesList.Any())
            {
                Console.WriteLine("   First 5 URLs:");
                foreach (var entry in entriesList.Take(5))
                {
                    Console.WriteLine($"   - {entry.Url}");
                    if (entry.LastModified.HasValue)
                        Console.WriteLine($"     Last modified: {entry.LastModified:yyyy-MM-dd}");
                }
                
                if (entriesList.Count > 5)
                    Console.WriteLine($"   ... and {entriesList.Count - 5} more");
            }

            Console.WriteLine();

            // Test crawling a few pages (limit to first 3 to avoid overwhelming the server)
            Console.WriteLine("2. Testing content extraction on first 3 URLs...");
            var testUrls = entriesList.Take(3).Select(e => e.Url);
            var crawledPages = await webCrawler.CrawlUrlsAsync(testUrls);
            var pagesList = crawledPages.ToList();

            foreach (var page in pagesList)
            {
                Console.WriteLine($"   URL: {page.Url}");
                Console.WriteLine($"   Title: {page.Title}");
                Console.WriteLine($"   Content length: {page.Content.Length} characters");
                if (!string.IsNullOrEmpty(page.MetaDescription))
                    Console.WriteLine($"   Meta description: {page.MetaDescription[..Math.Min(100, page.MetaDescription.Length)]}...");
                Console.WriteLine($"   Metadata entries: {page.Metadata.Count}");
                Console.WriteLine();
            }

            Console.WriteLine($"Successfully crawled {pagesList.Count} pages!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            Environment.Exit(1);
        }
    }

    private static void ShowUsage()
    {
        Console.WriteLine("Usage: CodeCube.Api.TestCli <sitemap-url>");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  CodeCube.Api.TestCli https://codecube.net/sitemap.xml");
        Console.WriteLine("  CodeCube.Api.TestCli http://localhost:8000/sitemap.xml");
        Console.WriteLine();
        Console.WriteLine("This tool will:");
        Console.WriteLine("  1. Parse the provided sitemap XML");
        Console.WriteLine("  2. Extract content from the first 3 URLs found");
        Console.WriteLine("  3. Display summary information about the crawled content");
    }
}
