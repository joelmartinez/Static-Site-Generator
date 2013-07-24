using RazorEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeCubeConsole
{
    public class Year
    {
        public string Name { get; set; }
        public Month[] Months { get; set; }
    }
    public class Month
    {
        public string Name { get; set; }
        public Post[] Posts { get; set; }
    }
    public class Post
    {

        public string Title { get; set; }
        public string Body { get; set; }
        public string ParsedBody
        {
            get
            {
                string body = this.Body;
                body = body.Replace("\n\n", "<br /><br />");
                
                if (body.Contains("[gist"))
                {
                    body = Regex.Replace(body, "\\[gist id=([0-9]+)\\]", "<script src=\"https://gist.github.com/joelmartinez/$1.js\"></script>");
                }
                return body;
            }
        }
        public string URL { get; set; }
        public string UrlPath
        {
            get
            {
                return new Uri(this.URL).AbsolutePath;
            }
        }
        public DateTime PublishedOn { get; set; }

    }
    
    class Program
    {
        static void Main(string[] args)
        {
            BuildSite();
            Console.ReadKey();
        }

        private static async void BuildSite()
        {
            //Directory.Delete("out", true);

            var model = GetContent();

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
            await SaveFile(result, string.Format("{0}.html", templateName));

            // now generate each individual content page
            Parallel.ForEach(model, post =>
            {
                string postTemplate = GetTemplate("post").Result;
                string postResult = Razor.Parse(postTemplate, post);
                SaveFile(postResult, post.UrlPath);
            });
        }

        private static async Task SaveFile(string content, string path)
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
