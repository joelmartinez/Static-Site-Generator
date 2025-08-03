using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Text.Json;
using RazorLight;
using Markdig;
using HtmlAgilityPack;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;

namespace CodeCubeConsole
{
    class Program
    {
        static readonly RazorLightEngine Engine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();

        static readonly string BasePath = AppContext.BaseDirectory;

        static string MasterTemplate = string.Empty;

        private static readonly string[] SupportedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".webp" };

        private static bool IsImageFormatSupported(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant();
            return !string.IsNullOrEmpty(extension) && SupportedImageExtensions.Contains(extension);
        }

        private static async Task ProcessHeroImages(IEnumerable<Post> posts)
        {
            foreach (var post in posts.Where(p => !string.IsNullOrEmpty(p.HeroImageUrl)))
            {
                await ProcessHeroImage(post);
            }
        }

        private static async Task ProcessHeroImage(Post post)
        {
            if (string.IsNullOrEmpty(post.HeroImageUrl))
                return;

            // Convert URL path to file path
            var imageUrl = post.HeroImageUrl;
            if (imageUrl.StartsWith("/"))
                imageUrl = imageUrl.Substring(1);

            var imagePath = Path.Combine(BasePath, "out", imageUrl);
            
            if (!File.Exists(imagePath))
            {
                Console.WriteLine($"Warning: Hero image not found: {imagePath}");
                return;
            }

            // Check if the image format is supported by ImageSharp
            if (!IsImageFormatSupported(imagePath))
            {
                Console.WriteLine($"Skipping unsupported image format: {imagePath}");
                return;
            }

            try
            {
                var fileInfo = new FileInfo(imagePath);
                var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileInfo.Name);
                var extension = fileInfo.Extension;
                var directory = fileInfo.DirectoryName;

                var optimizedPath = Path.Combine(directory ?? "", $"{nameWithoutExtension}.optimized{extension}");
                var thumbnailPath = Path.Combine(directory ?? "", $"{nameWithoutExtension}.thumb{extension}");

                // Generate optimized version (max 1200px width, 85% quality)
                if (!File.Exists(optimizedPath))
                {
                    using var image = await Image.LoadAsync(imagePath);
                    
                    // Only resize if image is larger than target
                    if (image.Width > 1200)
                    {
                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(1200, 0), // Keep aspect ratio
                            Mode = ResizeMode.Max
                        }));
                    }

                    var encoder = GetEncoder(extension);
                    await image.SaveAsync(optimizedPath, encoder);
                    Console.WriteLine($"Generated optimized image: {optimizedPath}");
                }

                // Generate thumbnail version (max 300px width)
                if (!File.Exists(thumbnailPath))
                {
                    using var image = await Image.LoadAsync(imagePath);
                    
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(300, 0), // Keep aspect ratio
                        Mode = ResizeMode.Max
                    }));

                    var encoder = GetEncoder(extension);
                    await image.SaveAsync(thumbnailPath, encoder);
                    Console.WriteLine($"Generated thumbnail image: {thumbnailPath}");
                }

                // Update post URLs to point to generated images
                var optimizedUrl = post.HeroImageUrl.Replace(extension, $".optimized{extension}");
                var thumbnailUrl = post.HeroImageUrl.Replace(extension, $".thumb{extension}");
                
                post.HeroImageOptimizedUrl = optimizedUrl;
                post.HeroImageThumbnailUrl = thumbnailUrl;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing hero image {imagePath}: {ex.Message}");
            }
        }

        private static IImageEncoder GetEncoder(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => new JpegEncoder { Quality = 85 },
                _ => new JpegEncoder { Quality = 85 } // Default to JPEG for other formats
            };
        }

        private static string MakeFullyQualifiedUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return url;
                
            // If URL already starts with http or https, return as-is
            if (url.StartsWith("http://") || url.StartsWith("https://"))
                return url;
                
            // If URL starts with /, prefix with the base domain
            if (url.StartsWith("/"))
                return "https://codecube.net" + url;
                
            // For relative URLs, return as-is (shouldn't happen in practice)
            return url;
        }

        public static async Task Main(string[] args)
        {
            // Parse version from command line arguments
            string? version = null;
            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--version" || args[i] == "-v") && i + 1 < args.Length)
                {
                    version = args[i + 1];
                    break;
                }
            }
            
            if (!string.IsNullOrEmpty(version))
            {
                Console.WriteLine($"Building site with version: {version}");
            }
            else
            {
                Console.WriteLine("Building site without version (local development mode)");
            }
            
            await BuildSite(version);
        }

        private static async Task BuildSite(string? version = null)
        {
            var model = GetContent();
            
            // Process hero images to generate optimized and thumbnail versions
            await ProcessHeroImages(model);
            
            MasterTemplate = await GetTemplate("master");

            // start with the index page
            string templateName = "index";
            Console.WriteLine("Generating {0}", templateName);
            string indexTemplate = await GetTemplate(templateName);
            var groupedModel = model
				.Where(m => m.IsPublished)
                .GroupBy(p => p.PublishedOn.Year)
                .Select(p => new Year
                {
                    Name = p.Key.ToString(),
                    Months = p
                        .GroupBy(m => m.PublishedOn.ToString("MMMM"))
                        .Select(m => new Month { Name = m.Key, Posts = m.ToArray() })
                        .ToArray()
                })
                .ToArray();
            string result = await Engine.CompileRenderStringAsync("index", indexTemplate, new IndexModel { Years = groupedModel });
            await SaveFile("CodeCube Ventures", result, string.Format("{0}.html", templateName), version);

            // the 'about' page
            string abouttemplate = await GetTemplate("about");
            result = await Engine.CompileRenderStringAsync("about", abouttemplate, (object?)null);
            await SaveFile("About Joel Martinez", result, "/about/", version);

            // the 'resume' page
            string resumetemplate = await GetTemplate("resume");
            result = await Engine.CompileRenderStringAsync("resume", resumetemplate, (object?)null);
            await SaveFile("Joel Martinez - Resume", result, "/resume/", version);

            // the 'map' page
            string maptemplate = await GetTemplate("map");
            result = await Engine.CompileRenderStringAsync("map", maptemplate, (object?)null);
            await SaveFile("Link Map - CodeCube Ventures", result, "/map/", version);

            // syndication
            string rsstemplate = await GetTemplate("rss");
            result = await Engine.CompileRenderStringAsync("rss", rsstemplate, model.Take(15).ToArray());
            await SaveFile(result, "/feed/");

            // sitemap
            await GenerateSitemap(model);

            // link map data generation
            await GenerateLinkMap(model);

            // now generate each individual content page
            string postTemplate = await GetTemplate("post");
            var latestPost = model.Where(m => m.IsPublished).OrderByDescending(p => p.PublishedOn).FirstOrDefault();
            
            foreach (var post in model)
            {
                var postPageModel = new PostPageModel 
                { 
                    CurrentPost = post, 
                    LatestPost = latestPost 
                };
                
                string postResult = await Engine.CompileRenderStringAsync("post", postTemplate, postPageModel);
                Master master = new Master()
                {
                    Title = post.Title,
                    Content = postResult,
                    Version = version
                };

                // set up meta tags
                string summary = post.BodySummary;

                master.Meta["title"] = post.Title;
                master.Meta["description"] = summary;
                master.Meta["canonical"] = post.URL;

                master.Meta["twitter:card"] = "summary";
                master.Meta["twitter:site"] = "@joelmartinez";
                master.Meta["twitter:creator"] = "@joelmartinez";
                master.Meta["twitter:title"] = post.Title;
                master.Meta["twitter:description"] = summary;

                master.Meta["og:type"] = "article";
                master.Meta["og:title"] = post.Title;
                master.Meta["og:description"] = summary;
                
                // Use hero image if available, otherwise fall back to content image
                string? imageUrl = null;
                if (!string.IsNullOrEmpty(post.HeroImageUrl))
                {
                    // Prefer optimized version for social media, fallback to original
                    imageUrl = post.HeroImageOptimizedUrl ?? post.HeroImageUrl;
                }
                else if (post.HasImage)
                {
                    imageUrl = post.ImageUrl;
                }
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    string fullyQualifiedImageUrl = MakeFullyQualifiedUrl(imageUrl);
                    master.Meta["twitter:image"] = fullyQualifiedImageUrl;
                    master.Meta["og:image"] = fullyQualifiedImageUrl;
                    master.Meta["image"] = fullyQualifiedImageUrl;
                }

                await SaveFile(master, post.UrlPath);
            }
        }

        private static async Task SaveFile(string title, string content, string path, string? version = null)
        {
            await SaveFile(new Master { Title = title, Content = content, Version = version }, path);
        }
        private static async Task SaveFile(Master master, string path)
        {
            string content = await Engine.CompileRenderStringAsync("master", MasterTemplate, master);

            path = await SaveFile(content, path);
        }

        private static async Task<string> SaveFile(string content, string path)
        {
            if (path.First() == '/') path = path.Substring(1, path.Length - 1);
            if (path.Last() == '/') path += "index.html";

            path = Path.Combine(BasePath, "out", path);

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Console.WriteLine("Persisting to {0}", path);
            using (StreamWriter writer = new StreamWriter(path))
            {
                await writer.WriteAsync(content);
            }
            return path;
        }

        private static IEnumerable<Post> GetContent()
        {
            string contentFile = Path.Combine(BasePath, "content.xml");
            XElement root = XElement.Load(contentFile);

            var query = (from xElem in root.Elements("channel").Elements("item")
                         select new
                             Post
                             {
                                 Title = (string)xElem.Element("title").Value,
                                 Body = (string)xElem.Element("contentencoded").Value,
                                 URL = (string)xElem.Element("link").Value,
                                 PublishedOn = DateTime.Parse(xElem.Element("pubDate").Value),
                                 Category = xElem.Elements("category")
                                    .Where(c => c.Attribute("domain")?.Value == "category")
                                    .FirstOrDefault()?.Value
                             }
                        )
                       ;

			IEnumerable<Post> markdownPosts = GetMarkdownContent ();
			var allPosts = query.Union(markdownPosts).OrderByDescending(p => p.PublishedOn).ToArray();

			WireUpPostRelationships(allPosts);
			ExtractEntitiesForAllPosts(allPosts);

			return allPosts;
        }

                static IEnumerable<Post> GetMarkdownContent ()
                {
                        var contentFolder = Path.Combine(BasePath, "content");
                        var allMarkdownFiles = Directory.EnumerateFiles (contentFolder, "*.md", SearchOption.AllDirectories).ToArray();

                        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

                        foreach (var file in allMarkdownFiles) {
				var content = File.ReadAllLines (file);
				var meta = content.TakeWhile (line => line.Contains (":")).Select (line => {
					var split = line.Split(':');
					return new KeyValuePair<string, string>(split[0], split[1]);
				}).ToDictionary(i => i.Key, i => i.Value);

				var contentLines = content.Skip (meta.Count);
				string markdownContent = string.Join (Environment.NewLine, contentLines);
                                string transformedContent = Markdown.ToHtml (markdownContent, pipeline);

				DateTime pubdate = DateTime.Parse (meta ["Date"]);
				string url = Path.GetFileNameWithoutExtension (file);
				url = string.Format ("https://codecube.net/{0}/{1}/{2}/", pubdate.Year, pubdate.Month, url);
				bool isPublished = true;
				if (meta.ContainsKey ("Published")) {
					isPublished = bool.Parse (meta ["Published"]);
				}
				
				string? prevUrl = meta.ContainsKey("Prev") ? meta["Prev"].Trim() : null;
				string? nextUrl = meta.ContainsKey("Next") ? meta["Next"].Trim() : null;
				string? heroImageUrl = meta.ContainsKey("Hero") ? meta["Hero"].Trim() : null;
				string? category = meta.ContainsKey("Category") ? meta["Category"].Trim() : null;
				
				yield return new Post {
					Title = meta["Title"],
					Body = transformedContent,
					PublishedOn = pubdate,
					URL = url,
					IsPublished = isPublished,
					ShouldRenderDoubleNewLine = false,
					PreviousUrl = prevUrl,
					NextUrl = nextUrl,
					HeroImageUrl = heroImageUrl,
					Category = category
				};
			}
		}

		private static void WireUpPostRelationships(Post[] allPosts)
		{
			// Create URL-to-Post map for efficient lookups
			var urlToPostMap = allPosts.ToDictionary(p => p.UrlPath, p => p);
			
			foreach (var post in allPosts)
			{
			    // Wire up previous post reference
			    if (!string.IsNullOrEmpty(post.PreviousUrl) && urlToPostMap.TryGetValue(post.PreviousUrl, out var prevPost))
			    {
			        post.Previous = prevPost;
			        // Bidirectional: if the previous post doesn't already have a Next, set it to this post
			        if (prevPost.Next == null)
			        {
			            prevPost.Next = post;
			        }
			    }
			    
			    // Wire up next post reference
			    if (!string.IsNullOrEmpty(post.NextUrl) && urlToPostMap.TryGetValue(post.NextUrl, out var nextPost))
			    {
			        post.Next = nextPost;
			        // Bidirectional: if the next post doesn't already have a Previous, set it to this post
			        if (nextPost.Previous == null)
			        {
			            nextPost.Previous = post;
			        }
			    }
			}
		}

		private static void ExtractEntitiesForAllPosts(Post[] allPosts)
		{
			Console.WriteLine("Extracting entities from content...");
			
			// First pass: extract entities for each post
			foreach (var post in allPosts)
			{
				post.Entities = EntityExtractor.ExtractEntities(post.Body, post.Title);
				Console.WriteLine($"Extracted {post.Entities.Length} entities from: {post.Title}");
			}
			
			// Second pass: extract entity connections between posts
			foreach (var post in allPosts)
			{
				post.EntityConnections = EntityExtractor.ExtractEntityConnections(post, allPosts);
			}
			
			Console.WriteLine($"Entity extraction complete for {allPosts.Length} posts");
		}

        private static async Task<string> GetTemplate(string template)
        {
            string templatePath = Path.Combine(BasePath, "Templates", $"{template}.cshtml");
            using (StreamReader reader = new StreamReader(templatePath))
            {
                //Does not block the main thread
                string content = await reader.ReadToEndAsync();
                return content;
            }
        }

        private static async Task GenerateSitemap(IEnumerable<Post> posts)
        {
            var sitemap = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "urlset",
                    new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),
                    new XAttribute(XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance") + "schemaLocation", 
                        "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd"),
                    
                    // Homepage
                    new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "url",
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "loc", "https://codecube.net/"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "lastmod", DateTime.Now.ToString("yyyy-MM-dd")),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "changefreq", "weekly"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "priority", "1.0")
                    ),
                    
                    // About page
                    new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "url",
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "loc", "https://codecube.net/about/"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "lastmod", DateTime.Now.ToString("yyyy-MM-dd")),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "changefreq", "monthly"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "priority", "0.8")
                    ),
                    
                    // Resume page
                    new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "url",
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "loc", "https://codecube.net/resume/"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "lastmod", DateTime.Now.ToString("yyyy-MM-dd")),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "changefreq", "monthly"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "priority", "0.8")
                    ),
                    
                    // All published posts
                    from post in posts.Where(p => p.IsPublished)
                    select new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "url",
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "loc", post.URL),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "lastmod", post.PublishedOn.ToString("yyyy-MM-dd")),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "changefreq", "monthly"),
                        new XElement(XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9") + "priority", "0.6")
                    )
                )
            );

            string sitemapPath = Path.Combine(BasePath, "out", "sitemap.xml");
            Console.WriteLine("Generating sitemap at {0}", sitemapPath);
            Directory.CreateDirectory(Path.GetDirectoryName(sitemapPath));
            await using (var writer = new StreamWriter(sitemapPath))
            {
                await writer.WriteAsync(sitemap.ToString());
            }
        }

        private static async Task GenerateLinkMap(IEnumerable<Post> posts)
        {
            var publishedPosts = posts.Where(p => p.IsPublished).ToArray();
            var nodes = new List<LinkMapNode>();
            var edges = new List<LinkMapEdge>();
            
            // Create URL to post mapping for quick lookups
            var urlToPostMap = publishedPosts.ToDictionary(p => p.UrlPath, p => p);
            
            // Calculate recency factors
            var earliestDate = publishedPosts.Min(p => p.PublishedOn);
            var latestDate = publishedPosts.Max(p => p.PublishedOn);
            var dateRange = (latestDate - earliestDate).TotalDays;
            
            // Group posts by year for year nodes
            var postsByYear = publishedPosts.GroupBy(p => p.PublishedOn.Year).ToArray();
            
            // Create year nodes
            foreach (var yearGroup in postsByYear)
            {
                var year = yearGroup.Key.ToString();
                var yearNode = new LinkMapNode
                {
                    Id = $"#{year}",
                    Title = year,
                    Url = $"/{year}",
                    Description = $"{yearGroup.Count()} posts from {year}",
                    PublishedOn = new DateTime(yearGroup.Key, 1, 1),
                    NodeType = "year"
                };
                nodes.Add(yearNode);
                
                // Connect year node to all posts in that year
                foreach (var post in yearGroup)
                {
                    edges.Add(new LinkMapEdge
                    {
                        Source = yearNode.Id,
                        Target = post.UrlPath
                    });
                }
            }
            
            // Group posts by category for category nodes
            var postsByCategory = publishedPosts
                .Where(p => !string.IsNullOrEmpty(p.Category))
                .GroupBy(p => p.Category)
                .ToArray();
            
            // Create category nodes
            foreach (var categoryGroup in postsByCategory)
            {
                var category = categoryGroup.Key;
                var categoryNode = new LinkMapNode
                {
                    Id = $"#category-{category.Replace(" ", "-").ToLowerInvariant()}",
                    Title = category,
                    Url = null, // Category nodes are not clickable since there are no category pages
                    Description = $"{categoryGroup.Count()} posts in {category}",
                    PublishedOn = categoryGroup.Max(p => p.PublishedOn), // Use latest post date for category
                    NodeType = "category"
                };
                nodes.Add(categoryNode);
                
                // Connect category node to all posts in that category
                foreach (var post in categoryGroup)
                {
                    edges.Add(new LinkMapEdge
                    {
                        Source = categoryNode.Id,
                        Target = post.UrlPath
                    });
                }
            }
            
            foreach (var post in publishedPosts)
            {
                // Calculate recency factor (0.0 = oldest, 1.0 = newest)
                var recencyFactor = dateRange > 0 ? (post.PublishedOn - earliestDate).TotalDays / dateRange : 0.5;
                
                // Create node for this post
                var node = new LinkMapNode
                {
                    Id = post.UrlPath,
                    Title = post.Title,
                    Url = post.URL,
                    Description = post.BodySummary,
                    PublishedOn = post.PublishedOn,
                    NodeType = "post",
                    RecencyFactor = recencyFactor
                };
                nodes.Add(node);
                
                // Extract links from post content
                var links = ExtractInternalLinks(post.Body, post.UrlPath);
                
                foreach (var linkUrl in links)
                {
                    // Normalize the link URL to match our UrlPath format
                    var normalizedUrl = NormalizeLinkUrl(linkUrl);
                    
                    // Check if this link points to another published post
                    if (!string.IsNullOrEmpty(normalizedUrl) && 
                        urlToPostMap.ContainsKey(normalizedUrl) && 
                        normalizedUrl != post.UrlPath) // Avoid self-links
                    {
                        var edge = new LinkMapEdge
                        {
                            Source = post.UrlPath,
                            Target = normalizedUrl
                        };
                        edges.Add(edge);
                    }
                }
            }
            
            // Create entity nodes
            var allEntities = publishedPosts.SelectMany(p => p.Entities).Distinct().ToArray();
            Console.WriteLine($"Creating {allEntities.Length} entity nodes");
            
            foreach (var entity in allEntities)
            {
                var entityPosts = publishedPosts.Where(p => p.Entities.Contains(entity)).ToArray();
                var entityNode = new LinkMapNode
                {
                    Id = $"#entity-{entity.Replace(" ", "-").Replace("#", "sharp").Replace("+", "plus").ToLowerInvariant()}",
                    Title = entity,
                    Url = null, // Entity nodes are not clickable
                    Description = $"Entity mentioned in {entityPosts.Length} posts",
                    PublishedOn = entityPosts.Max(p => p.PublishedOn), // Use latest post date for entity
                    NodeType = "entity"
                };
                nodes.Add(entityNode);
                
                // Connect entity node to all posts that mention it
                foreach (var post in entityPosts)
                {
                    edges.Add(new LinkMapEdge
                    {
                        Source = entityNode.Id,
                        Target = post.UrlPath
                    });
                }
                
                // Create entity-to-entity connections based on co-occurrence in posts
                foreach (var otherEntity in allEntities.Where(e => e != entity))
                {
                    var sharedPosts = entityPosts.Where(p => p.Entities.Contains(otherEntity)).ToArray();
                    if (sharedPosts.Length > 0)
                    {
                        var otherEntityId = $"#entity-{otherEntity.Replace(" ", "-").Replace("#", "sharp").Replace("+", "plus").ToLowerInvariant()}";
                        edges.Add(new LinkMapEdge
                        {
                            Source = entityNode.Id,
                            Target = otherEntityId
                        });
                    }
                }
            }
            
            // Calculate connection counts for all nodes
            foreach (var node in nodes)
            {
                node.ConnectionCount = edges.Count(e => e.Source == node.Id || e.Target == node.Id);
            }
            
            var mapData = new LinkMapData
            {
                Nodes = nodes.ToArray(),
                Edges = edges.ToArray()
            };
            
            // Serialize to JSON and wrap in a JavaScript variable assignment
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            };
            
            var json = JsonSerializer.Serialize(mapData, jsonOptions);
            var jsContent = $"window.linkMapData = {json};";
            
            // Save to map.gen.js
            var mapJsPath = Path.Combine(BasePath, "out", "script", "map.gen.js");
            Directory.CreateDirectory(Path.GetDirectoryName(mapJsPath));
            Console.WriteLine("Generating link map data at {0}", mapJsPath);
            
            await using (var writer = new StreamWriter(mapJsPath))
            {
                await writer.WriteAsync(jsContent);
            }
        }
        
        private static List<string> ExtractInternalLinks(string htmlContent, string currentPostPath)
        {
            var links = new List<string>();
            
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml($"<html><body>{htmlContent}</body></html>");
                
                var linkNodes = doc.DocumentNode.SelectNodes("//a[@href]");
                if (linkNodes != null)
                {
                    foreach (var linkNode in linkNodes)
                    {
                        var href = linkNode.GetAttributeValue("href", "");
                        if (!string.IsNullOrEmpty(href))
                        {
                            links.Add(href);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error extracting links from {0}: {1}", currentPostPath, ex.Message);
            }
            
            return links;
        }
        
        private static string NormalizeLinkUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;
                
            // Handle year anchor links like /#2023
            if (url.StartsWith("/#") && url.Length >= 6)
            {
                var yearPart = url.Substring(2);
                if (int.TryParse(yearPart, out var year) && year >= 2000 && year <= DateTime.Now.Year + 10)
                {
                    return $"#{yearPart}";
                }
            }
                
            // Handle fully qualified URLs that point to our domain
            if (url.StartsWith("https://codecube.net"))
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }
            
            // Handle root-relative URLs (starting with /)
            if (url.StartsWith("/"))
            {
                return url;
            }
            
            // Skip external URLs and fragments (except year anchors handled above)
            if (url.StartsWith("http") || url.StartsWith("mailto:") || url.StartsWith("#"))
            {
                return string.Empty;
            }
            
            // For relative URLs, we'd need more context to resolve them properly
            // For now, return empty since the posts seem to use absolute paths
            return string.Empty;
        }
    }
}
