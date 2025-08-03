using System.Collections.Generic;

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
        public string? Version { get; set; }
        
        /// <summary>
        /// Gets the version query string for static assets. Returns empty string if no version is set.
        /// </summary>
        public string VersionQueryString => string.IsNullOrEmpty(Version) ? "" : $"?v={Version}";
    }
}