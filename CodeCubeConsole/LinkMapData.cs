using System;

namespace CodeCubeConsole
{
    public class LinkMapData
    {
        public LinkMapNode[] Nodes { get; set; } = Array.Empty<LinkMapNode>();
        public LinkMapEdge[] Edges { get; set; } = Array.Empty<LinkMapEdge>();
    }
}