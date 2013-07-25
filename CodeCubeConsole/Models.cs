using System;
using System.Text.RegularExpressions;

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
}
