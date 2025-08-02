using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace CodeCubeConsole
{
    public static class EntityExtractor
    {
        // Common technology and programming-related entities
        private static readonly string[] TechEntities = {
            "C#", "JavaScript", "TypeScript", "Python", "Java", "F#", "SQL", "HTML", "CSS", "React", "Angular", "Vue",
            ".NET", "ASP.NET", "Node.js", "Express", "MongoDB", "PostgreSQL", "Redis", "Docker", "Kubernetes", "AWS", "Azure",
            "Git", "GitHub", "Visual Studio", "VS Code", "Xamarin", "Unity", "XNA", "WPF", "Blazor", "SignalR", "Entity Framework",
            "LINQ", "PowerShell", "Windows", "Linux", "macOS", "iOS", "Android", "Xbox", "REST", "GraphQL", "API", "JSON", "XML",
            "OAuth", "JWT", "HTTP", "HTTPS", "TCP", "UDP", "Microservices", "DevOps", "CI/CD", "Machine Learning", "AI", "ML",
            "Neural Network", "Deep Learning", "TensorFlow", "PyTorch", "OpenAI", "ChatGPT", "GPT", "Copilot", "Semantic Kernel",
            "LangChain", "Vector Store", "RAG", "Transformer", "LLM", "NLP"
        };

        // Company and organization entities
        private static readonly string[] CompanyEntities = {
            "Microsoft", "Google", "Apple", "Amazon", "Meta", "Facebook", "Twitter", "LinkedIn", "GitHub", "Stack Overflow",
            "OpenAI", "Anthropic", "Netflix", "Spotify", "Slack", "Discord", "Zoom", "Salesforce", "Oracle", "IBM", "Intel",
            "AMD", "NVIDIA", "Tesla", "Uber", "Airbnb", "Stripe", "PayPal", "Shopify", "Atlassian", "JetBrains", "Unity Technologies",
            "Epic Games", "Valve", "Steam", "Xamarin", "Twilio", "Heroku", "DigitalOcean", "Vercel", "Netlify"
        };

        // Concept entities
        private static readonly string[] ConceptEntities = {
            "Open Source", "Agile", "Scrum", "Kanban", "Pair Programming", "Test Driven Development", "TDD", "BDD",
            "Domain Driven Design", "DDD", "SOLID", "Design Patterns", "Architecture", "Microservices", "Monolith",
            "Database", "NoSQL", "Relational Database", "ORM", "MVC", "MVP", "MVVM", "Clean Architecture", "Dependency Injection",
            "Inversion of Control", "IoC", "Factory Pattern", "Singleton", "Observer Pattern", "Command Pattern", "Strategy Pattern",
            "Repository Pattern", "Unit Testing", "Integration Testing", "End-to-End Testing", "Continuous Integration",
            "Continuous Deployment", "Containerization", "Orchestration", "Load Balancing", "Caching", "Security", "Authentication",
            "Authorization", "Encryption", "HTTPS", "Performance", "Scalability", "Monitoring", "Logging", "Debugging"
        };

        public static string[] ExtractEntities(string content, string title)
        {
            var entities = new HashSet<string>();
            
            // Clean HTML tags from content
            var cleanContent = StripHtmlTags(content);
            var fullText = $"{title} {cleanContent}";
            
            // Extract technology entities
            foreach (var entity in TechEntities)
            {
                if (ContainsEntity(fullText, entity))
                {
                    entities.Add(entity);
                }
            }
            
            // Extract company entities
            foreach (var entity in CompanyEntities)
            {
                if (ContainsEntity(fullText, entity))
                {
                    entities.Add(entity);
                }
            }
            
            // Extract concept entities
            foreach (var entity in ConceptEntities)
            {
                if (ContainsEntity(fullText, entity))
                {
                    entities.Add(entity);
                }
            }
            
            return entities.ToArray();
        }

        public static EntityConnection[] ExtractEntityConnections(Post post, IEnumerable<Post> allPosts)
        {
            var connections = new List<EntityConnection>();
            
            // Find connections between entities in this post and entities in other posts
            foreach (var otherPost in allPosts.Where(p => p != post && p.IsPublished))
            {
                foreach (var entityInThisPost in post.Entities)
                {
                    foreach (var entityInOtherPost in otherPost.Entities)
                    {
                        // If both posts contain the same entity, create a connection
                        if (entityInThisPost.Equals(entityInOtherPost, StringComparison.OrdinalIgnoreCase))
                        {
                            connections.Add(new EntityConnection
                            {
                                FromEntity = entityInThisPost,
                                ToEntity = entityInOtherPost,
                                Relationship = "also_mentioned_in",
                                ToPostUrl = otherPost.UrlPath
                            });
                        }
                        
                        // Check for related technology connections
                        if (AreRelatedEntities(entityInThisPost, entityInOtherPost))
                        {
                            connections.Add(new EntityConnection
                            {
                                FromEntity = entityInThisPost,
                                ToEntity = entityInOtherPost,
                                Relationship = "related_to",
                                ToPostUrl = otherPost.UrlPath
                            });
                        }
                    }
                }
            }
            
            return connections.ToArray();
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

        private static bool AreRelatedEntities(string entity1, string entity2)
        {
            // Define some simple relationships between entities
            var relationships = new Dictionary<string, string[]>
            {
                ["C#"] = new[] { ".NET", "ASP.NET", "Entity Framework", "LINQ", "Visual Studio", "Xamarin", "Blazor" },
                ["JavaScript"] = new[] { "TypeScript", "Node.js", "React", "Angular", "Vue", "HTML", "CSS" },
                ["Microsoft"] = new[] { "C#", ".NET", "Azure", "Visual Studio", "Xamarin", "PowerShell", "Windows" },
                ["AI"] = new[] { "Machine Learning", "ML", "OpenAI", "ChatGPT", "GPT", "Neural Network", "Deep Learning" },
                ["DevOps"] = new[] { "CI/CD", "Docker", "Kubernetes", "Git", "GitHub" },
                ["Database"] = new[] { "SQL", "NoSQL", "MongoDB", "PostgreSQL", "Entity Framework", "ORM" }
            };

            foreach (var kvp in relationships)
            {
                if ((entity1.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) && 
                     kvp.Value.Contains(entity2, StringComparer.OrdinalIgnoreCase)) ||
                    (entity2.Equals(kvp.Key, StringComparison.OrdinalIgnoreCase) && 
                     kvp.Value.Contains(entity1, StringComparer.OrdinalIgnoreCase)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}