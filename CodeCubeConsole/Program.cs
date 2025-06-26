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

            // syndication
            string rsstemplate = await GetTemplate("rss");
            result = await Engine.CompileRenderStringAsync("rss", rsstemplate, model.Take(15).ToArray());
            await SaveFile(result, "/feed/");

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

            path = Path.Combine("out", path);

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
            XElement root = XElement.Load("content.xml");

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
			var allMarkdownFiles = Directory.EnumerateFiles ("content", "*.md", SearchOption.AllDirectories).ToArray();

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
            using (StreamReader reader = new StreamReader(string.Format("Templates/{0}.cshtml", template)))
            {
                //Does not block the main thread
                string content = await reader.ReadToEndAsync();
                return content;
            }
        }
    }
}
