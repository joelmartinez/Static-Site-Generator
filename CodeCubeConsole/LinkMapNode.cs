using System;

namespace CodeCubeConsole
{
    public class LinkMapNode
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime PublishedOn { get; set; }
        public string NodeType { get; set; } = "post"; // "post" or "year"
        public int ConnectionCount { get; set; } = 0;
        public double RecencyFactor { get; set; } = 0.0; // 0.0 = oldest, 1.0 = newest
    }
}