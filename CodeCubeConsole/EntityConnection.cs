namespace CodeCubeConsole
{
    public class EntityConnection
    {
        public string FromEntity { get; set; } = string.Empty;
        public string ToEntity { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
        public string ToPostUrl { get; set; } = string.Empty; // URL of the post containing the connected entity
    }
}