using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using RazorLight;
using Markdig;

namespace CodeCubeConsole
{
    public class IndexModel
    {
        public Year[] Years { get; set; }
    }

    class Program
    {
        static readonly RazorLightEngine Engine = new RazorLightEngineBuilder()
            .UseMemoryCachingProvider()
            .Build();

        static readonly string BasePath = AppContext.BaseDirectory;

        static string MasterTemplate = string.Empty;

        public static async Task Main(string[] args)
        {
            await BuildSite();
        }

        private static async Task BuildSite()
        {
            var model = GetContent();
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
            await SaveFile("Joel Martinez", result, string.Format("{0}.html", templateName));

            // the 'about' page
            string abouttemplate = await GetTemplate("about");
            result = await Engine.CompileRenderStringAsync("about", abouttemplate, (object?)null);
            await SaveFile("About Joel Martinez", result, "/about/");

            // the 'resume' page
            string resumetemplate = await GetTemplate("resume");
            result = await Engine.CompileRenderStringAsync("resume", resumetemplate, (object?)null);
            await SaveFile("Joel Martinez - Resume", result, "/resume/");

            // syndication
            string rsstemplate = await GetTemplate("rss");
            result = await Engine.CompileRenderStringAsync("rss", rsstemplate, model.Take(15).ToArray());
            await SaveFile(result, "/feed/");

            // sitemap
            await GenerateSitemap(model);

            // now generate each individual content page
            string postTemplate = await GetTemplate("post");
            foreach (var post in model)
            {
                string postResult = await Engine.CompileRenderStringAsync("post", postTemplate, post);
                Master master = new Master()
                {
                    Title = post.Title,
                    Content = postResult
                };

                // set up meta tags
                string summary = post.BodySummary;

                master.Meta["title"] = post.Title;
                master.Meta["description"] = summary;

                master.Meta["twitter:card"] = "summary";
                master.Meta["twitter:site"] = "@joelmartinez";
                master.Meta["twitter:creator"] = "@joelmartinez";
                master.Meta["twitter:title"] = post.Title;
                master.Meta["twitter:description"] = summary;

                master.Meta["og:type"] = "article";
                master.Meta["og:title"] = post.Title;
                master.Meta["og:description"] = summary;
                if (post.HasImage)
                {
                    master.Meta["twitter:image"] = post.ImageUrl;
                    master.Meta["og:image"] = post.ImageUrl;
                    master.Meta["image"] = post.ImageUrl;
                }

                await SaveFile(master, post.UrlPath);
            }
        }

        private static async Task SaveFile(string title, string content, string path)
        {
            await SaveFile(new Master { Title = title, Content = content }, path);
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
                                 PublishedOn = DateTime.Parse(xElem.Element("pubDate").Value)
                             }
                        )
                       ;

			IEnumerable<Post> markdownPosts = GetMarkdownContent ();

			return query.Union(markdownPosts).OrderByDescending(p => p.PublishedOn).ToArray();

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
				yield return new Post {
					Title = meta["Title"],
					Body = transformedContent,
					PublishedOn = pubdate,
					URL = url,
					IsPublished = isPublished,
					ShouldRenderDoubleNewLine = false
				};
			}
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
    }
}
