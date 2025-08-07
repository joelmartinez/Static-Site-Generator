/**
 * Virtual File System for CodeCube OS
 * Provides file system-like navigation over the link map data
 */
class VirtualFileSystem {
  constructor() {
    this.nodeCache = new Map();
    this.initialized = false;
    this.currentPath = '/';
  }

  /**
   * Initialize the virtual file system with link map data
   */
  async initialize() {
    if (this.initialized) return;

    // Get link map data from the global variable
    const linkMapData = window.linkMapData;
    if (!linkMapData || !linkMapData.nodes || !linkMapData.edges) {
      throw new Error('Link map data not available');
    }

    this.linkMapData = linkMapData;
    this.buildNodeCache();
    this.initialized = true;
  }

  /**
   * Build the virtual file system structure
   */
  buildNodeCache() {
    this.nodeCache.clear();

    // Add root directory
    this.nodeCache.set('/', {
      path: '/',
      name: '',
      isDirectory: true,
      children: []
    });

    // Create main directories
    const mainDirs = ['content', 'years', 'categories', 'entities'];
    mainDirs.forEach(dir => {
      const dirPath = `/${dir}`;
      const dirNode = {
        path: dirPath,
        name: dir,
        isDirectory: true,
        children: [],
        parent: this.nodeCache.get('/')
      };
      this.nodeCache.set(dirPath, dirNode);
      this.nodeCache.get('/').children.push(dirNode);
    });

    // Add all posts to /content
    const contentDir = this.nodeCache.get('/content');
    this.linkMapData.nodes
      .filter(n => n.nodeType === 'post')
      .forEach(node => {
        const fileName = this.getFileNameFromNode(node);
        const filePath = `/content/${fileName}`;
        const fileNode = {
          path: filePath,
          name: fileName,
          isDirectory: false,
          linkMapNode: node,
          parent: contentDir
        };
        this.nodeCache.set(filePath, fileNode);
        contentDir.children.push(fileNode);
      });

    // Build year directories
    this.buildYearDirectories();
    this.buildCategoryDirectories();
    this.buildEntityDirectories();
  }

  /**
   * Build year-based directory structure
   */
  buildYearDirectories() {
    const yearsDir = this.nodeCache.get('/years');
    const yearNodes = this.linkMapData.nodes
      .filter(n => n.nodeType === 'year')
      .sort((a, b) => b.title.localeCompare(a.title)); // Newest first

    yearNodes.forEach(yearNode => {
      const yearName = yearNode.title;
      const yearPath = `/years/${yearName}`;
      const yearDir = {
        path: yearPath,
        name: yearName,
        isDirectory: true,
        children: [],
        parent: yearsDir,
        linkMapNode: yearNode
      };
      this.nodeCache.set(yearPath, yearDir);
      yearsDir.children.push(yearDir);

      // Add posts for this year
      const yearPosts = this.linkMapData.nodes
        .filter(n => n.nodeType === 'post' && 
          new Date(n.publishedOn).getFullYear().toString() === yearName);

      yearPosts.forEach(post => {
        const fileName = this.getFileNameFromNode(post);
        const filePath = `${yearPath}/${fileName}`;
        const fileNode = {
          path: filePath,
          name: fileName,
          isDirectory: false,
          linkMapNode: post,
          parent: yearDir
        };
        this.nodeCache.set(filePath, fileNode);
        yearDir.children.push(fileNode);
      });
    });
  }

  /**
   * Build category-based directory structure
   */
  buildCategoryDirectories() {
    const categoriesDir = this.nodeCache.get('/categories');
    const categoryNodes = this.linkMapData.nodes
      .filter(n => n.nodeType === 'category')
      .sort((a, b) => a.title.localeCompare(b.title));

    categoryNodes.forEach(categoryNode => {
      const categoryName = this.sanitizeDirectoryName(categoryNode.title);
      const categoryPath = `/categories/${categoryName}`;
      const categoryDir = {
        path: categoryPath,
        name: categoryName,
        isDirectory: true,
        children: [],
        parent: categoriesDir,
        linkMapNode: categoryNode
      };
      this.nodeCache.set(categoryPath, categoryDir);
      categoriesDir.children.push(categoryDir);

      // Find posts connected to this category
      const categoryConnections = this.linkMapData.edges
        .filter(e => e.source === categoryNode.id)
        .map(e => e.target);

      const categoryPosts = this.linkMapData.nodes
        .filter(n => n.nodeType === 'post' && categoryConnections.includes(n.id));

      categoryPosts.forEach(post => {
        const fileName = this.getFileNameFromNode(post);
        const filePath = `${categoryPath}/${fileName}`;
        const fileNode = {
          path: filePath,
          name: fileName,
          isDirectory: false,
          linkMapNode: post,
          parent: categoryDir
        };
        this.nodeCache.set(filePath, fileNode);
        categoryDir.children.push(fileNode);
      });
    });
  }

  /**
   * Build entity-based directory structure
   */
  buildEntityDirectories() {
    const entitiesDir = this.nodeCache.get('/entities');
    const entityNodes = this.linkMapData.nodes
      .filter(n => n.nodeType === 'entity')
      .sort((a, b) => a.title.localeCompare(b.title));

    entityNodes.forEach(entityNode => {
      const entityName = this.sanitizeDirectoryName(entityNode.title);
      const entityPath = `/entities/${entityName}`;
      const entityDir = {
        path: entityPath,
        name: entityName,
        isDirectory: true,
        children: [],
        parent: entitiesDir,
        linkMapNode: entityNode
      };
      this.nodeCache.set(entityPath, entityDir);
      entitiesDir.children.push(entityDir);

      // Find posts connected to this entity
      const entityConnections = this.linkMapData.edges
        .filter(e => e.source === entityNode.id)
        .map(e => e.target);

      const entityPosts = this.linkMapData.nodes
        .filter(n => n.nodeType === 'post' && entityConnections.includes(n.id));

      entityPosts.forEach(post => {
        const fileName = this.getFileNameFromNode(post);
        const filePath = `${entityPath}/${fileName}`;
        const fileNode = {
          path: filePath,
          name: fileName,
          isDirectory: false,
          linkMapNode: post,
          parent: entityDir
        };
        this.nodeCache.set(filePath, fileNode);
        entityDir.children.push(fileNode);
      });
    });
  }

  /**
   * Extract filename from a node
   */
  getFileNameFromNode(node) {
    if (node.nodeType === 'post' && node.id) {
      // Extract filename from URL path like "/2025/8/team-series-specialization/"
      const urlPath = node.id.replace(/^\/|\/$/g, ''); // Remove leading/trailing slashes
      const segments = urlPath.split('/');
      if (segments.length >= 3) {
        return segments[2] + '.md'; // e.g., "team-series-specialization.md"
      }
    }
    
    // Fallback to sanitized title
    return this.sanitizeFileName(node.title) + '.md';
  }

  /**
   * Sanitize filename for virtual file system
   */
  sanitizeFileName(fileName) {
    return fileName
      .replace(/[^a-zA-Z0-9\s\-_.]/g, '-') // Replace invalid chars with dash
      .replace(/\s+/g, '-') // Replace spaces with dash
      .toLowerCase();
  }

  /**
   * Sanitize directory name
   */
  sanitizeDirectoryName(dirName) {
    return dirName
      .replace(/[^a-zA-Z0-9\s\-_.]/g, '-')
      .replace(/\s+/g, '-')
      .replace(/&/g, 'and')
      .toLowerCase();
  }

  /**
   * Normalize path
   */
  normalizePath(path) {
    if (!path || path === '/') return '/';
    
    // Remove trailing slash except for root
    path = path.replace(/\/+$/, '');
    
    // Ensure leading slash
    if (!path.startsWith('/')) {
      path = '/' + path;
    }
    
    return path;
  }

  /**
   * Get node at path
   */
  getNode(path) {
    path = this.normalizePath(path);
    return this.nodeCache.get(path) || null;
  }

  /**
   * List directory contents
   */
  listDirectory(path) {
    const node = this.getNode(path);
    if (!node || !node.isDirectory) {
      return [];
    }

    return node.children
      .sort((a, b) => {
        // Directories first, then files
        if (a.isDirectory && !b.isDirectory) return -1;
        if (!a.isDirectory && b.isDirectory) return 1;
        return a.name.localeCompare(b.name);
      });
  }

  /**
   * Get content for a node
   */
  getNodeContent(node) {
    if (!node) return 'Node not found';

    if (node.isDirectory) {
      const childCount = node.children ? node.children.length : 0;
      const linkNode = node.linkMapNode;
      
      let content = `Directory: ${node.name}\n`;
      content += `Items: ${childCount}\n`;
      
      if (linkNode) {
        content += `Type: ${linkNode.nodeType}\n`;
        if (linkNode.description) {
          content += `\nDescription:\n${linkNode.description}`;
        }
      }
      
      return content;
    }

    if (!node.linkMapNode) {
      return `File: ${node.name}\nNo content available`;
    }

    const linkNode = node.linkMapNode;
    let content = `Title: ${linkNode.title}\n`;
    content += `Type: ${linkNode.nodeType}\n`;
    content += `Published: ${new Date(linkNode.publishedOn).toLocaleDateString()}\n`;
    
    if (linkNode.url) {
      content += `URL: ${linkNode.url}\n`;
    }
    
    content += `Connections: ${linkNode.connectionCount || 0}\n`;
    
    if (linkNode.description) {
      content += `\nDescription:\n${linkNode.description}`;
    }

    return content;
  }

  /**
   * Get current working directory
   */
  getCurrentPath() {
    return this.currentPath;
  }

  /**
   * Change directory
   */
  changeDirectory(path) {
    if (!path) return this.currentPath;

    // Handle relative paths
    if (!path.startsWith('/')) {
      if (this.currentPath === '/') {
        path = '/' + path;
      } else {
        path = this.currentPath + '/' + path;
      }
    }

    // Handle .. and .
    const segments = path.split('/').filter(s => s.length > 0);
    const resolved = [];
    
    for (const segment of segments) {
      if (segment === '..') {
        resolved.pop();
      } else if (segment !== '.') {
        resolved.push(segment);
      }
    }

    const resolvedPath = resolved.length === 0 ? '/' : '/' + resolved.join('/');
    const node = this.getNode(resolvedPath);
    
    if (node && node.isDirectory) {
      this.currentPath = resolvedPath;
      return this.currentPath;
    } else {
      throw new Error(`cd: ${path}: No such directory`);
    }
  }

  /**
   * Check if system is initialized
   */
  isInitialized() {
    return this.initialized;
  }
}

// Create and export a singleton instance
const vfs = new VirtualFileSystem();
export default vfs;