using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeCubeConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildSite();
            Console.ReadKey();
        }

        private static async void BuildSite()
        {
            var model = GetContent();
            string masterTemplate = await GetTemplate("master");
            Razor.Compile(masterTemplate, typeof(Master), "master");

            // start with the index page
            string templateName = "index";
            Console.WriteLine("Generating {0}", templateName);
            string indexTemplate = await GetTemplate(templateName);
            var groupedModel = model
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
            string result = Razor.Parse(indexTemplate, new { Years = groupedModel });
            await SaveFile("Joel Martinez", result, string.Format("{0}.html", templateName));

            // the 'about' page
            string abouttemplate = await GetTemplate("about");
            result = Razor.Parse(abouttemplate);
            SaveFile("About Joel Martinez", result, "/about/");

            // syndication
            string rsstemplate = await GetTemplate("rss");
            result = Razor.Parse(rsstemplate, model.Take(15).ToArray());
            SaveFile(result, "/feed/");

            // now generate each individual content page
            string postTemplate = await GetTemplate("post");
            Razor.Compile(postTemplate, typeof(Post), "post");
            Parallel.ForEach(model, post =>
            {
                string postResult = Razor.Run("post", post);
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

                SaveFile(master, post.UrlPath);
            });
        }

        private static async Task SaveFile(string title, string content, string path)
        {
            await SaveFile(new Master { Title = title, Content = content }, path);
        }
        private static async Task SaveFile(Master master, string path)
        {
            string content = Razor.Run("master", master);

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

        private static  IEnumerable<Post> GetContent()
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

            return query.OrderByDescending(p => p.PublishedOn).ToArray();

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
