using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Services;
using CodeCube.Api.Core.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeCube.Api.Crawler;

class Program
{
    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("CodeCube Content Crawler");
        Console.WriteLine("========================");

        if (args.Length == 0 || args[0] == "--help" || args[0] == "-h")
        {
            ShowUsage();
            return args.Length == 0 ? 1 : 0;
        }

        var sitemapUrl = args[0];
        DateTime? cutoffDate = null;
        string? outputFile = null;

        // Parse additional arguments
        for (int i = 1; i < args.Length; i++)
        {
            if (args[i] == "--cutoff-date" && i + 1 < args.Length)
            {
                if (DateTime.TryParseExact(args[i + 1], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out var date))
                {
                    cutoffDate = date;
                    i++; // Skip the next argument since we consumed it
                }
                else
                {
                    Console.WriteLine($"Error: Invalid date format '{args[i + 1]}'. Expected format: YYYY-MM-DD");
                    return 1;
                }
            }
            else if (args[i] == "--output" && i + 1 < args.Length)
            {
                outputFile = args[i + 1];
                i++; // Skip the next argument since we consumed it
            }
        }

        // Default output file if not specified
        if (string.IsNullOrEmpty(outputFile))
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            outputFile = $"crawled-content-{timestamp}.json";
        }

        Console.WriteLine($"Sitemap URL: {sitemapUrl}");
        if (cutoffDate.HasValue)
        {
            Console.WriteLine($"Cutoff date: {cutoffDate:yyyy-MM-dd} (only crawling content updated on or after this date)");
        }
        else
        {
            Console.WriteLine("Cutoff date: None (crawling all content)");
        }
        Console.WriteLine($"Output file: {outputFile}");
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

            // Filter by cutoff date if specified
            if (cutoffDate.HasValue)
            {
                var filteredEntries = entriesList.Where(entry => 
                    entry.LastModified.HasValue && entry.LastModified.Value.Date >= cutoffDate.Value.Date).ToList();
                
                Console.WriteLine($"   Filtered to {filteredEntries.Count} URLs updated on or after {cutoffDate:yyyy-MM-dd}");
                entriesList = filteredEntries;
            }

            if (!entriesList.Any())
            {
                Console.WriteLine("   No URLs to crawl after filtering.");
                
                // Create empty output file
                var emptyResult = new CrawlResult
                {
                    CrawledAt = DateTime.UtcNow,
                    SitemapUrl = sitemapUrl,
                    CutoffDate = cutoffDate,
                    TotalUrls = 0,
                    CrawledPages = new List<PageContent>(),
                    Errors = new List<CrawlError>()
                };
                
                await SaveResultsAsync(emptyResult, outputFile);
                Console.WriteLine($"Empty results saved to: {outputFile}");
                return 0;
            }

            // Show sample URLs
            Console.WriteLine("   Sample URLs to crawl:");
            foreach (var entry in entriesList.Take(5))
            {
                Console.WriteLine($"   - {entry.Url}");
                if (entry.LastModified.HasValue)
                    Console.WriteLine($"     Last modified: {entry.LastModified:yyyy-MM-dd}");
            }
            
            if (entriesList.Count > 5)
                Console.WriteLine($"   ... and {entriesList.Count - 5} more");

            Console.WriteLine();

            // Crawl all filtered URLs
            Console.WriteLine($"2. Crawling {entriesList.Count} URLs...");
            var urls = entriesList.Select(e => e.Url);
            var crawlResult = await webCrawler.CrawlUrlsWithErrorTrackingAsync(urls);
            var pagesList = crawlResult.SuccessfulPages;

            Console.WriteLine($"   Successfully crawled {pagesList.Count} pages");
            
            if (crawlResult.Errors.Any())
            {
                Console.WriteLine($"   Encountered {crawlResult.Errors.Count} errors during crawling");
            }

            // Create result object
            var result = new CrawlResult
            {
                CrawledAt = DateTime.UtcNow,
                SitemapUrl = sitemapUrl,
                CutoffDate = cutoffDate,
                TotalUrls = entriesList.Count,
                CrawledPages = pagesList,
                Errors = crawlResult.Errors
            };

            // Save results to JSON file
            await SaveResultsAsync(result, outputFile);

            Console.WriteLine();
            Console.WriteLine($"✅ Crawling completed successfully!");
            Console.WriteLine($"📄 Results saved to: {outputFile}");
            Console.WriteLine($"📊 Crawled {pagesList.Count} pages from {entriesList.Count} URLs");
            
            // Display error summary if there were any errors
            if (crawlResult.Errors.Any())
            {
                Console.WriteLine();
                Console.WriteLine($"⚠️  Error Summary ({crawlResult.Errors.Count} errors encountered):");
                
                // Group errors by message to deduplicate similar errors
                var errorGroups = crawlResult.Errors
                    .GroupBy(e => e.Message)
                    .OrderByDescending(g => g.Count())
                    .Take(10); // Show top 10 most common errors
                
                foreach (var errorGroup in errorGroups)
                {
                    var count = errorGroup.Count();
                    var message = errorGroup.Key;
                    Console.WriteLine($"   • {message} ({count} occurrence{(count > 1 ? "s" : "")})");
                    
                    // Show first few URLs that had this error
                    var exampleUrls = errorGroup.Take(3).Select(e => e.Url);
                    foreach (var url in exampleUrls)
                    {
                        Console.WriteLine($"     - {url}");
                    }
                    
                    if (errorGroup.Count() > 3)
                    {
                        Console.WriteLine($"     ... and {errorGroup.Count() - 3} more");
                    }
                }
                
                if (crawlResult.Errors.Count > 30) // Show message if many errors were grouped
                {
                    var hiddenErrors = crawlResult.Errors.Count - errorGroups.Sum(g => g.Count());
                    if (hiddenErrors > 0)
                    {
                        Console.WriteLine($"   ... and {hiddenErrors} other errors (see JSON output for full details)");
                    }
                }
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            return 1;
        }
    }

    private static async Task SaveResultsAsync(CrawlResult result, string outputFile)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(result, options);
        await File.WriteAllTextAsync(outputFile, json);
    }

    private static void ShowUsage()
    {
        Console.WriteLine("""
            Usage: CodeCube.Api.Crawler <sitemap-url> [options]

            Arguments:
              sitemap-url          The URL of the XML sitemap to crawl

            Options:
              --cutoff-date DATE   Only crawl content updated on or after DATE (format: YYYY-MM-DD)
              --output FILE        Output file path (default: crawled-content-TIMESTAMP.json)

            Examples:
              CodeCube.Api.Crawler https://codecube.net/sitemap.xml
              CodeCube.Api.Crawler https://codecube.net/sitemap.xml --cutoff-date 2025-01-01
              CodeCube.Api.Crawler https://codecube.net/sitemap.xml --cutoff-date 2025-08-01 --output my-crawl.json

            This tool will:
              1. Parse the provided sitemap XML
              2. Filter URLs by cutoff date if provided
              3. Extract content from all matching URLs
              4. Save results as structured JSON for indexing
            """);
    }
}

/// <summary>
/// Result structure for crawler output
/// </summary>
public class CrawlResult
{
    public DateTime CrawledAt { get; set; }
    public string SitemapUrl { get; set; } = string.Empty;
    public DateTime? CutoffDate { get; set; }
    public int TotalUrls { get; set; }
    public List<PageContent> CrawledPages { get; set; } = new();
    public List<CrawlError> Errors { get; set; } = new();
}
