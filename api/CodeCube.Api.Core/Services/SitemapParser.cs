using System.Globalization;
using System.Xml;
using CodeCube.Api.Core.Interfaces;
using CodeCube.Api.Core.Models;

namespace CodeCube.Api.Core.Services;

/// <summary>
/// Parses XML sitemaps and extracts URL entries
/// </summary>
public class SitemapParser : ISitemapParser
{
    private readonly IHttpClient _httpClient;

    public SitemapParser(IHttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<SitemapEntry>> ParseSitemapAsync(string sitemapUrl)
    {
        if (string.IsNullOrWhiteSpace(sitemapUrl))
            throw new ArgumentException("Sitemap URL cannot be null or empty", nameof(sitemapUrl));

        var sitemapXml = await _httpClient.GetStringAsync(sitemapUrl);
        return await ParseSitemapFromContentAsync(sitemapXml);
    }

    public Task<IEnumerable<SitemapEntry>> ParseSitemapFromContentAsync(string sitemapXml)
    {
        if (string.IsNullOrWhiteSpace(sitemapXml))
            throw new ArgumentException("Sitemap XML content cannot be null or empty", nameof(sitemapXml));

        var entries = new List<SitemapEntry>();

        try
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(sitemapXml);

            // Handle XML namespaces
            var namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("sitemap", "http://www.sitemaps.org/schemas/sitemap/0.9");

            var urlNodes = xmlDoc.SelectNodes("//sitemap:url", namespaceManager);
            
            if (urlNodes != null)
            {
                foreach (XmlNode urlNode in urlNodes)
                {
                    var entry = ParseUrlNode(urlNode, namespaceManager);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse sitemap XML: {ex.Message}", ex);
        }

        return Task.FromResult<IEnumerable<SitemapEntry>>(entries);
    }

    private static SitemapEntry? ParseUrlNode(XmlNode urlNode, XmlNamespaceManager namespaceManager)
    {
        var locNode = urlNode.SelectSingleNode("sitemap:loc", namespaceManager);
        if (locNode?.InnerText == null)
            return null;

        var entry = new SitemapEntry
        {
            Url = locNode.InnerText
        };

        // Parse optional lastmod
        var lastModNode = urlNode.SelectSingleNode("sitemap:lastmod", namespaceManager);
        if (lastModNode?.InnerText != null && 
            DateTime.TryParse(lastModNode.InnerText, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var lastMod))
        {
            entry.LastModified = lastMod;
        }

        // Parse optional changefreq
        var changeFreqNode = urlNode.SelectSingleNode("sitemap:changefreq", namespaceManager);
        if (changeFreqNode?.InnerText != null)
        {
            entry.ChangeFrequency = changeFreqNode.InnerText;
        }

        // Parse optional priority
        var priorityNode = urlNode.SelectSingleNode("sitemap:priority", namespaceManager);
        if (priorityNode?.InnerText != null && 
            double.TryParse(priorityNode.InnerText, CultureInfo.InvariantCulture, out var priority))
        {
            entry.Priority = priority;
        }

        return entry;
    }
}