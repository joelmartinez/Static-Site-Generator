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

    const parts = path.split('/').filter(part => part !== '');
    if (parts.length === 0) {
      return this;
    }

    const [first, ...rest] = parts;
    const child = this.getChild(first);
    
    if (!child) {
      return null;
    }

    if (rest.length === 0) {
      return child;
    }

    return child.findByPath(rest.join('/'));
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
}