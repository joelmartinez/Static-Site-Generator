using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CodeCubeConsole
{
    public static class EntityExtractor
    {
        // High-level container categories that aggregate related concepts
        private static readonly Dictionary<string, string[]> EntityContainers = new Dictionary<string, string[]>
        {
            ["AI"] = new[] {
                "AI", "ML", "Machine Learning", "Neural Network", "Deep Learning", "TensorFlow", "PyTorch", 
                "OpenAI", "ChatGPT", "GPT", "Copilot", "Semantic Kernel", "LangChain", "Vector Store", "RAG", 
                "Transformer", "LLM", "NLP", "Inference", "Anthropic"
            },
            ["Microsoft"] = new[] {
                "Microsoft", "Azure", "Visual Studio", "VS Code", ".NET", "ASP.NET", "Blazor", "SignalR", 
                "Entity Framework", "LINQ", "PowerShell", "Windows", "SQL Server", "Xamarin", "C#", "F#"
            },
            ["Web Development"] = new[] {
                "JavaScript", "TypeScript", "HTML", "CSS", "React", "Angular", "Vue", "Node.js", "Express", 
                "REST", "GraphQL", "API", "JSON", "XML", "HTTP", "HTTPS", "OAuth", "JWT", "MVC", "MVP", "MVVM"
            },
            ["GameDev"] = new[] {
                "XNA", "Unity", "OpenGL", "DirectX", "Xbox", "Unity Technologies", "Epic Games", "Valve", "Steam",
                "Game Development", "3D Graphics", "Shader", "Physics"
            },
            ["Mobile Development"] = new[] {
                "iOS", "Android", "Xamarin", "React Native", "Flutter", "Swift", "Kotlin", "Mobile", "App Store", "Google Play"
            },
            ["Cloud & DevOps"] = new[] {
                "AWS", "Azure", "Docker", "Kubernetes", "DevOps", "CI/CD", "Continuous Integration", "Continuous Deployment",
                "Microservices", "Containerization", "Orchestration", "Load Balancing", "Monitoring", "Git", "GitHub"
            },
            ["Databases"] = new[] {
                "SQL", "MongoDB", "PostgreSQL", "Redis", "Database", "NoSQL", "Relational Database", "ORM", 
                "Entity Framework", "Data", "Storage"
            },
            ["Programming"] = new[] {
                "Python", "Java", "C#", "F#", "JavaScript", "TypeScript", "Open Source", "Architecture", "Design Patterns",
                "SOLID", "Clean Architecture", "Dependency Injection", "Inversion of Control", "IoC", "Factory Pattern", 
                "Singleton", "Observer Pattern", "Command Pattern", "Strategy Pattern", "Repository Pattern", 
                "Unit Testing", "Integration Testing", "End-to-End Testing", "TDD", "BDD", "Debugging", "Performance",
                "Scalability", "Caching", "Security", "Authentication", "Authorization", "Encryption", "Logging"
            },
            ["Companies"] = new[] {
                "Google", "Apple", "Amazon", "Meta", "Facebook", "Twitter", "LinkedIn", "Netflix", "Spotify", 
                "Slack", "Discord", "Zoom", "Salesforce", "Oracle", "IBM", "Intel", "AMD", "NVIDIA", "Tesla", 
                "Uber", "Airbnb", "Stripe", "PayPal", "Shopify", "Atlassian", "JetBrains", "Twilio", "Heroku", 
                "DigitalOcean", "Vercel", "Netlify", "Stack Overflow"
            },
            ["Platforms"] = new[] {
                "Windows", "Linux", "macOS", "GitHub", "Visual Studio", "VS Code"
            },
            ["Methodologies"] = new[] {
                "Agile", "Scrum", "Kanban", "Pair Programming", "Test Driven Development", "Domain Driven Design", "DDD"
            },
            ["Networking"] = new[] {
                "TCP", "UDP", "HTTP", "HTTPS", "REST", "GraphQL", "API", "Networking", "Protocol"
            }
        };

        public static string[] ExtractEntities(string content, string title)
        {
            var containerEntities = new HashSet<string>();
            
            // Clean HTML tags from content
            var cleanContent = StripHtmlTags(content);
            var fullText = $"{title} {cleanContent}";
            
            // Check each container for matching entities
            foreach (var container in EntityContainers)
            {
                var containerName = container.Key;
                var entities = container.Value;
                
                foreach (var entity in entities)
                {
                    if (ContainsEntity(fullText, entity))
                    {
                        containerEntities.Add(containerName);
                        break; // Found at least one entity in this container, no need to check others
                    }
                }
            }
            
            // Ensure every post has at least one container relationship
            // If no containers were found, assign a default based on content analysis
            if (!containerEntities.Any())
            {
                var defaultContainer = DetermineDefaultContainer(fullText);
                if (!string.IsNullOrEmpty(defaultContainer))
                {
                    containerEntities.Add(defaultContainer);
                }
            }
            
            return containerEntities.ToArray();
        }

        public static EntityConnection[] ExtractEntityConnections(Post post, IEnumerable<Post> allPosts)
        {
            var connections = new List<EntityConnection>();
            
            // Find connections between container entities in this post and container entities in other posts
            foreach (var otherPost in allPosts.Where(p => p != post && p.IsPublished))
            {
                foreach (var containerInThisPost in post.Entities)
                {
                    foreach (var containerInOtherPost in otherPost.Entities)
                    {
                        // If both posts contain the same container, create a connection
                        if (containerInThisPost.Equals(containerInOtherPost, StringComparison.OrdinalIgnoreCase))
                        {
                            connections.Add(new EntityConnection
                            {
                                FromEntity = containerInThisPost,
                                ToEntity = containerInOtherPost,
                                Relationship = "also_mentioned_in",
                                ToPostUrl = otherPost.UrlPath
                            });
                        }
                        
                        // Check for related container connections
                        if (AreRelatedContainers(containerInThisPost, containerInOtherPost))
                        {
                            connections.Add(new EntityConnection
                            {
                                FromEntity = containerInThisPost,
                                ToEntity = containerInOtherPost,
                                Relationship = "related_to",
                                ToPostUrl = otherPost.UrlPath
                            });
                        }
                    }
                }
            }
            
            return connections.ToArray();
        }

        private static bool AreRelatedContainers(string container1, string container2)
        {
            // Define relationships between high-level containers
            var containerRelationships = new Dictionary<string, string[]>
            {
                ["AI"] = new[] { "Programming", "Microsoft", "Cloud & DevOps" },
                ["Microsoft"] = new[] { "Programming", "Web Development", "Cloud & DevOps", "AI" },
                ["Web Development"] = new[] { "Programming", "Cloud & DevOps", "Microsoft" },
                ["GameDev"] = new[] { "Programming", "Microsoft", "Platforms" },
                ["Mobile Development"] = new[] { "Programming", "Platforms" },
                ["Cloud & DevOps"] = new[] { "Programming", "Microsoft", "Web Development", "Databases" },
                ["Databases"] = new[] { "Programming", "Cloud & DevOps", "Microsoft" },
                ["Programming"] = new[] { "Microsoft", "Web Development", "GameDev", "AI", "Mobile Development" }
            };

            foreach (var kvp in containerRelationships)
            {
                if ((container1.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) && 
                     kvp.Value.Contains(container2, StringComparer.OrdinalIgnoreCase)) ||
                    (container2.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) && 
                     kvp.Value.Contains(container1, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ContainsEntity(string text, string entity)
        {
            // Use word boundaries to avoid partial matches
            var pattern = $@"\b{Regex.Escape(entity)}\b";
            return Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase);
        }

        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;
                
            try
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc.DocumentNode.InnerText ?? string.Empty;
            }
            catch
            {
                // Fallback to regex if HTML parsing fails
                return Regex.Replace(html, "<.*?>", string.Empty);
            }
        }

        private static string DetermineDefaultContainer(string text)
        {
            // Use simple heuristics to determine a default container if no specific entities are found
            var lowercaseText = text.ToLowerInvariant();
            
            // Check for programming-related keywords
            if (lowercaseText.Contains("code") || lowercaseText.Contains("programming") || 
                lowercaseText.Contains("software") || lowercaseText.Contains("development") ||
                lowercaseText.Contains("application") || lowercaseText.Contains("system") ||
                lowercaseText.Contains("technical") || lowercaseText.Contains("engineer"))
            {
                return "Programming";
            }
            
            // Check for web-related keywords
            if (lowercaseText.Contains("web") || lowercaseText.Contains("site") || 
                lowercaseText.Contains("browser") || lowercaseText.Contains("online"))
            {
                return "Web Development";
            }
            
            // Check for business/leadership keywords
            if (lowercaseText.Contains("team") || lowercaseText.Contains("management") || 
                lowercaseText.Contains("leadership") || lowercaseText.Contains("business") ||
                lowercaseText.Contains("work") || lowercaseText.Contains("career"))
            {
                return "Methodologies";
            }
            
            // If no specific indicators, default to Programming
            return "Programming";
        }
    }
}