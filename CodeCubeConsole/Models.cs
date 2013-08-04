﻿using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace CodeCubeConsole
{
    public class Master
    {
        public Master()
        {
            this.Meta = new Dictionary<string, string>();
        }
        public string Title { get; set; }
        public string Content { get; set; }
        public Dictionary<string, string> Meta { get; private set; }
    }

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
        public bool HasImage { get; private set; }
        public string ImageUrl { get; private set; }
        public string BodySummary
        {
            get
            {
                HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(string.Format("<html><body>{0}</body></html>",this.Body));

                // figure out if we've got an image
                var img = doc.DocumentNode.SelectSingleNode("//img");
                if (img != null)
                {
                    this.HasImage = true;
                    this.ImageUrl = img.Attributes["src"].Value;
                }

                // now pull together the summary
                var textArray = GetHtml(doc.DocumentNode).ToArray();
                int len = 197;
                StringBuilder sb = new StringBuilder(len + 3);
                foreach (var t in textArray)
                {
                    // make sure appending the next word will not go over the limit
                    if (sb.Length <= len && (sb.Length + t.Length) <= len)
                    {
                        sb.Append(t);
                    }
                    else
                    {
                        sb.Append("...");
                        break;
                    }
                }
                return sb.ToString();
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

        private IEnumerable<string> GetHtml(HtmlNode node)
        {
            if (node.Name == "#text")
            {
                yield return node.InnerText;
            }

            foreach (var child in node.ChildNodes)
            {
                foreach(var childText in GetHtml(child))
                {
                    yield return childText;
                }
            }
        }
    }
}
