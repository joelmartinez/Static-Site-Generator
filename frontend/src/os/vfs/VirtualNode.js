/**
 * Represents a node in the virtual file system
 */
export class VirtualNode {
  constructor(name, type = 'file', content = '', parent = null) {
    this.name = name;
    this.type = type; // 'file' or 'directory'
    this.content = content;
    this.parent = parent;
    this.children = new Map();
    this.metadata = {};
    this.url = null;
  }

  /**
   * Add a child node
   */
  addChild(node) {
    if (this.type !== 'directory') {
      throw new Error('Cannot add children to non-directory nodes');
    }
    node.parent = this;
    this.children.set(node.name, node);
    return node;
  }

  /**
   * Get a child by name
   */
  getChild(name) {
    return this.children.get(name);
  }

  /**
   * Get all children as an array
   */
  getChildren() {
    return Array.from(this.children.values());
  }

  /**
   * Check if node has children
   */
  hasChildren() {
    return this.children.size > 0;
  }

  /**
   * Get the full path of this node
   */
  getPath() {
    const parts = [];
    let current = this;
    while (current && current.name !== '/') {
      parts.unshift(current.name);
      current = current.parent;
    }
    return '/' + parts.join('/');
  }

  /**
   * Find a node by path from this node
   */
  findByPath(path) {
    if (!path || path === '.') {
      return this;
    }

    if (path === '..') {
      return this.parent || this;
    }

    // Handle absolute paths
    if (path.startsWith('/')) {
      let root = this;
      while (root.parent) {
        root = root.parent;
      }
      return root.findByPath(path.substring(1));
    }

    // Split path and process each part
    const parts = path.split('/').filter(part => part !== '');
    if (parts.length === 0) {
      return this;
    }

    let currentNode = this;
    for (const part of parts) {
      if (part === '.') {
        // Stay at current node
        continue;
      } else if (part === '..') {
        // Go to parent
        currentNode = currentNode.parent || currentNode;
      } else {
        // Go to child
        const child = currentNode.getChild(part);
        if (!child) {
          return null;
        }
        currentNode = child;
      }
    }

    return currentNode;
  }

  /**
   * Check if this is a directory
   */
  isDirectory() {
    return this.type === 'directory';
  }

  /**
   * Check if this is a file
   */
  isFile() {
    return this.type === 'file';
  }

  /**
   * Set metadata for this node
   */
  setMetadata(key, value) {
    this.metadata[key] = value;
  }

  /**
   * Get metadata for this node
   */
  getMetadata(key) {
    return this.metadata[key];
  }

  /**
   * Set URL for this node (if it represents a web resource)
   */
  setUrl(url) {
    this.url = url;
  }

  /**
   * Get URL for this node
   */
  getUrl() {
    return this.url;
  }

  /**
   * Get a displayable filename for this node
   * Ensures every node type has a well-defined file name
   */
  getDisplayName() {
    if (this.isFile()) {
      return this.name;
    }
    
    // For directories, return the name with a trailing slash
    return this.name + '/';
  }

  /**
   * Get content for display purposes (for cat command)
   * Every node type should be cat-able
   */
  getDisplayContent() {
    if (this.isFile()) {
      return this.content || '';
    }
    
    // For directories, show a listing-style content
    if (this.isDirectory()) {
      let content = `Directory: ${this.getPath()}\n`;
      content += `Type: ${this.type}\n`;
      
      if (this.url) {
        content += `URL: ${this.url}\n`;
      }
      
      const metadata = this.getMetadata('linkMapNode');
      if (metadata) {
        content += `Description: ${metadata.description || 'N/A'}\n`;
        if (metadata.connectionCount !== undefined) {
          content += `Connections: ${metadata.connectionCount}\n`;
        }
      }
      
      content += `\nContents (${this.children.size} items):\n`;
      const children = this.getChildren();
      if (children.length === 0) {
        content += '  (empty)\n';
      } else {
        children.forEach(child => {
          const indicator = child.isDirectory() ? '/' : '';
          content += `  ${child.name}${indicator}\n`;
        });
      }
      
      return content;
    }
    
    return `Node: ${this.name}\nType: ${this.type}\nPath: ${this.getPath()}`;
  }
}