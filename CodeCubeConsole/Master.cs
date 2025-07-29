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
    }
}