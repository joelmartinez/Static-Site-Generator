namespace CodeCubeConsole
{
    public class PostPageModel
    {
        public required Post CurrentPost { get; set; }
        public Post? LatestPost { get; set; }
    }
}