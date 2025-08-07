using System.Text.Json;

namespace CodeCubeConsole
{
    public class VirtualFileSystem
    {
        private readonly string _basePath;
        private LinkMapData? _linkMapData;
        private readonly Dictionary<string, VirtualNode> _nodeCache = new();

        public VirtualFileSystem(string basePath)
        {
            _basePath = basePath;
        }

        public async Task<LinkMapData> GetLinkMapDataAsync()
        {
            if (_linkMapData == null)
            {
                await LoadLinkMapDataAsync();
            }
            return _linkMapData ?? new LinkMapData();
        }

        private async Task LoadLinkMapDataAsync()
        {
            var mapJsPath = Path.Combine(_basePath, "out", "script", "map.gen.js");
            
            if (!File.Exists(mapJsPath))
            {
                throw new FileNotFoundException($"Link map file not found: {mapJsPath}");
            }

            var jsContent = await File.ReadAllTextAsync(mapJsPath);
            
            // Extract JSON from the JavaScript assignment
            const string prefix = "window.linkMapData = ";
            const string suffix = ";";
            
            if (!jsContent.StartsWith(prefix) || !jsContent.EndsWith(suffix))
            {
                throw new InvalidDataException("Invalid link map format");
            }

            var jsonContent = jsContent.Substring(prefix.Length, jsContent.Length - prefix.Length - suffix.Length);
            
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            _linkMapData = JsonSerializer.Deserialize<LinkMapData>(jsonContent, options);
            
            if (_linkMapData == null)
            {
                throw new InvalidDataException("Failed to deserialize link map data");
            }

            BuildNodeCache();
        }

        private void BuildNodeCache()
        {
            if (_linkMapData == null) return;

            _nodeCache.Clear();

            // Add root directory
            _nodeCache["/"] = new VirtualNode
            {
                Path = "/",
                Name = "",
                IsDirectory = true,
                Children = new List<VirtualNode>()
            };

            // Create main directories
            var mainDirs = new[] { "content", "years", "categories", "entities" };
            foreach (var dir in mainDirs)
            {
                var dirPath = $"/{dir}";
                var dirNode = new VirtualNode
                {
                    Path = dirPath,
                    Name = dir,
                    IsDirectory = true,
                    Children = new List<VirtualNode>(),
                    Parent = _nodeCache["/"]
                };
                _nodeCache[dirPath] = dirNode;
                _nodeCache["/"].Children.Add(dirNode);
            }

            // Add all posts to /content
            var contentDir = _nodeCache["/content"];
            foreach (var node in _linkMapData.Nodes.Where(n => n.NodeType == "post"))
            {
                var fileName = GetFileNameFromNode(node);
                var filePath = $"/content/{fileName}";
                var fileNode = new VirtualNode
                {
                    Path = filePath,
                    Name = fileName,
                    IsDirectory = false,
                    LinkMapNode = node,
                    Parent = contentDir
                };
                _nodeCache[filePath] = fileNode;
                contentDir.Children.Add(fileNode);
            }

            // Build year directories
            BuildYearDirectories();
            BuildCategoryDirectories();
            BuildEntityDirectories();
        }

        private void BuildYearDirectories()
        {
            if (_linkMapData == null) return;

            var yearsDir = _nodeCache["/years"];
            var yearNodes = _linkMapData.Nodes
                .Where(n => n.NodeType == "year")
                .OrderByDescending(n => n.Title);

            foreach (var yearNode in yearNodes)
            {
                var yearName = yearNode.Title;
                var yearPath = $"/years/{yearName}";
                var yearDir = new VirtualNode
                {
                    Path = yearPath,
                    Name = yearName,
                    IsDirectory = true,
                    Children = new List<VirtualNode>(),
                    Parent = yearsDir,
                    LinkMapNode = yearNode
                };
                _nodeCache[yearPath] = yearDir;
                yearsDir.Children.Add(yearDir);

                // Add posts for this year
                var yearPosts = _linkMapData.Nodes
                    .Where(n => n.NodeType == "post" && n.PublishedOn.Year.ToString() == yearName);

                foreach (var post in yearPosts)
                {
                    var fileName = GetFileNameFromNode(post);
                    var filePath = $"{yearPath}/{fileName}";
                    var fileNode = new VirtualNode
                    {
                        Path = filePath,
                        Name = fileName,
                        IsDirectory = false,
                        LinkMapNode = post,
                        Parent = yearDir
                    };
                    _nodeCache[filePath] = fileNode;
                    yearDir.Children.Add(fileNode);
                }
            }
        }

        private void BuildCategoryDirectories()
        {
            if (_linkMapData == null) return;

            var categoriesDir = _nodeCache["/categories"];
            var categoryNodes = _linkMapData.Nodes
                .Where(n => n.NodeType == "category")
                .OrderBy(n => n.Title);

            foreach (var categoryNode in categoryNodes)
            {
                var categoryName = SanitizeDirectoryName(categoryNode.Title);
                var categoryPath = $"/categories/{categoryName}";
                var categoryDir = new VirtualNode
                {
                    Path = categoryPath,
                    Name = categoryName,
                    IsDirectory = true,
                    Children = new List<VirtualNode>(),
                    Parent = categoriesDir,
                    LinkMapNode = categoryNode
                };
                _nodeCache[categoryPath] = categoryDir;
                categoriesDir.Children.Add(categoryDir);

                // Find posts connected to this category
                var categoryConnections = _linkMapData.Edges
                    .Where(e => e.Source == categoryNode.Id)
                    .Select(e => e.Target);

                var categoryPosts = _linkMapData.Nodes
                    .Where(n => n.NodeType == "post" && categoryConnections.Contains(n.Id));

                foreach (var post in categoryPosts)
                {
                    var fileName = GetFileNameFromNode(post);
                    var filePath = $"{categoryPath}/{fileName}";
                    var fileNode = new VirtualNode
                    {
                        Path = filePath,
                        Name = fileName,
                        IsDirectory = false,
                        LinkMapNode = post,
                        Parent = categoryDir
                    };
                    _nodeCache[filePath] = fileNode;
                    categoryDir.Children.Add(fileNode);
                }
            }
        }

        private void BuildEntityDirectories()
        {
            if (_linkMapData == null) return;

            var entitiesDir = _nodeCache["/entities"];
            var entityNodes = _linkMapData.Nodes
                .Where(n => n.NodeType == "entity")
                .OrderBy(n => n.Title);

            foreach (var entityNode in entityNodes)
            {
                var entityName = SanitizeDirectoryName(entityNode.Title);
                var entityPath = $"/entities/{entityName}";
                var entityDir = new VirtualNode
                {
                    Path = entityPath,
                    Name = entityName,
                    IsDirectory = true,
                    Children = new List<VirtualNode>(),
                    Parent = entitiesDir,
                    LinkMapNode = entityNode
                };
                _nodeCache[entityPath] = entityDir;
                entitiesDir.Children.Add(entityDir);

                // Find posts connected to this entity
                var entityConnections = _linkMapData.Edges
                    .Where(e => e.Source == entityNode.Id)
                    .Select(e => e.Target);

                var entityPosts = _linkMapData.Nodes
                    .Where(n => n.NodeType == "post" && entityConnections.Contains(n.Id));

                foreach (var post in entityPosts)
                {
                    var fileName = GetFileNameFromNode(post);
                    var filePath = $"{entityPath}/{fileName}";
                    var fileNode = new VirtualNode
                    {
                        Path = filePath,
                        Name = fileName,
                        IsDirectory = false,
                        LinkMapNode = post,
                        Parent = entityDir
                    };
                    _nodeCache[filePath] = fileNode;
                    entityDir.Children.Add(fileNode);
                }
            }
        }

        private static string GetFileNameFromNode(LinkMapNode node)
        {
            if (node.NodeType == "post" && !string.IsNullOrEmpty(node.Id))
            {
                // Extract filename from URL path like "/2025/8/team-series-specialization/"
                var urlPath = node.Id.Trim('/');
                var segments = urlPath.Split('/');
                if (segments.Length >= 3)
                {
                    return segments[2] + ".md"; // e.g., "team-series-specialization.md"
                }
            }
            
            // Fallback to sanitized title
            return SanitizeFileName(node.Title) + ".md";
        }

        private static string SanitizeFileName(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var result = fileName;
            foreach (char c in invalid)
            {
                result = result.Replace(c, '-');
            }
            return result.Replace(' ', '-').ToLowerInvariant();
        }

        private static string SanitizeDirectoryName(string dirName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var result = dirName;
            foreach (char c in invalid)
            {
                result = result.Replace(c, '-');
            }
            return result.Replace(' ', '-').Replace("&", "and").ToLowerInvariant();
        }

        public VirtualNode? GetNode(string path)
        {
            path = NormalizePath(path);
            return _nodeCache.TryGetValue(path, out var node) ? node : null;
        }

        public List<VirtualNode> ListDirectory(string path)
        {
            var node = GetNode(path);
            if (node?.IsDirectory == true)
            {
                return node.Children.OrderBy(n => n.IsDirectory ? 0 : 1)
                    .ThenBy(n => n.Name)
                    .ToList();
            }
            return new List<VirtualNode>();
        }

        public string GetNodeContent(VirtualNode node)
        {
            if (node.LinkMapNode == null)
            {
                return $"Directory: {node.Name}";
            }

            var linkNode = node.LinkMapNode;
            var content = $"Title: {linkNode.Title}\n";
            content += $"Type: {linkNode.NodeType}\n";
            content += $"Published: {linkNode.PublishedOn:yyyy-MM-dd}\n";
            
            if (!string.IsNullOrEmpty(linkNode.Url))
            {
                content += $"URL: {linkNode.Url}\n";
            }
            
            content += $"Connections: {linkNode.ConnectionCount}\n";
            content += $"\nDescription:\n{linkNode.Description}";

            return content;
        }

        private static string NormalizePath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
                return "/";
                
            // Remove trailing slash except for root
            path = path.TrimEnd('/');
            
            // Ensure leading slash
            if (!path.StartsWith('/'))
                path = "/" + path;
                
            return path;
        }
    }

    public class VirtualNode
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public LinkMapNode? LinkMapNode { get; set; }
        public List<VirtualNode> Children { get; set; } = new();
        public VirtualNode? Parent { get; set; }
    }
}