import { VirtualNode } from './VirtualNode.js';
import { LinkMapDataSource } from './LinkMapDataSource.js';

/**
 * Virtual File System for codecube OS
 * Provides a file system interface to the linkmap data
 */
export class VirtualFileSystem {
  constructor() {
    this.dataSource = new LinkMapDataSource();
    this.root = null;
    this.currentPath = '/';
    this.initialized = false;
  }

  /**
   * Initialize the virtual file system
   */
  async initialize() {
    if (this.initialized) {
      return;
    }

    try {
      const data = await this.dataSource.loadData();
      this.buildFileSystem(data);
      this.initialized = true;
    } catch (error) {
      console.error('Failed to initialize VFS:', error);
      // Create a minimal file system even if data loading fails
      this.createMinimalFileSystem();
      this.initialized = true;
    }
  }

  /**
   * Build the virtual file system from linkmap data
   */
  buildFileSystem(data) {
    // Create root directory
    this.root = new VirtualNode('/', 'directory');
    
    // Create main directories
    const contentDir = this.root.addChild(new VirtualNode('content', 'directory'));
    const postsDir = contentDir.addChild(new VirtualNode('posts', 'directory'));
    const yearsDir = contentDir.addChild(new VirtualNode('years', 'directory'));
    const categoriesDir = contentDir.addChild(new VirtualNode('categories', 'directory'));
    const entitiesDir = contentDir.addChild(new VirtualNode('entities', 'directory'));

    // Process nodes from linkmap data
    const yearNodes = new Map();
    const categoryNodes = new Map();
    const entityNodes = new Map();
    const postNodes = new Map();

    // Group nodes by type
    for (const node of data.nodes) {
      switch (node.nodeType) {
        case 'year':
          yearNodes.set(node.id, node);
          break;
        case 'category':
          categoryNodes.set(node.id, node);
          break;
        case 'entity':
          entityNodes.set(node.id, node);
          break;
        case 'post':
          postNodes.set(node.id, node);
          break;
      }
    }

    // Create year directories and organize posts by year
    for (const [yearId, yearData] of yearNodes) {
      const yearName = yearData.title;
      const yearDir = yearsDir.addChild(new VirtualNode(yearName, 'directory'));
      yearDir.setMetadata('linkMapNode', yearData);
      yearDir.setUrl(yearData.url);

      // Add year info file
      const yearInfo = yearDir.addChild(new VirtualNode('info.txt', 'file', 
        `Year: ${yearData.title}\nDescription: ${yearData.description}\nPosts: ${yearData.connectionCount}\nURL: ${yearData.url || 'N/A'}`));
      yearInfo.setMetadata('linkMapNode', yearData);
      yearInfo.setUrl(yearData.url);
    }

    // Create category directories
    for (const [categoryId, categoryData] of categoryNodes) {
      const categoryName = this.sanitizeFileName(categoryData.title);
      const categoryDir = categoriesDir.addChild(new VirtualNode(categoryName, 'directory'));
      categoryDir.setMetadata('linkMapNode', categoryData);

      // Add category info file
      const categoryInfo = categoryDir.addChild(new VirtualNode('info.txt', 'file',
        `Category: ${categoryData.title}\nDescription: ${categoryData.description}\nPosts: ${categoryData.connectionCount}`));
      categoryInfo.setMetadata('linkMapNode', categoryData);
    }

    // Create entity directories
    for (const [entityId, entityData] of entityNodes) {
      const entityName = this.sanitizeFileName(entityData.title);
      const entityDir = entitiesDir.addChild(new VirtualNode(entityName, 'directory'));
      entityDir.setMetadata('linkMapNode', entityData);

      // Add entity info file
      const entityInfo = entityDir.addChild(new VirtualNode('info.txt', 'file',
        `Entity: ${entityData.title}\nDescription: ${entityData.description}\nConnections: ${entityData.connectionCount}`));
      entityInfo.setMetadata('linkMapNode', entityData);
    }

    // Create post files organized by year
    for (const [postId, postData] of postNodes) {
      const postFileName = this.sanitizeFileName(postData.title) + '.md';
      
      // Determine year from post data
      let postYear = '2025'; // default
      if (postData.publishedOn) {
        const pubDate = new Date(postData.publishedOn);
        postYear = pubDate.getFullYear().toString();
      }

      // Find or create year directory in posts
      let yearDir = postsDir.getChild(postYear);
      if (!yearDir) {
        yearDir = postsDir.addChild(new VirtualNode(postYear, 'directory'));
      }

      // Create post file
      const postContent = this.generatePostContent(postData);
      const postFile = yearDir.addChild(new VirtualNode(postFileName, 'file', postContent));
      postFile.setMetadata('linkMapNode', postData);
      postFile.setUrl(postData.url);

      // Also add to appropriate year directory
      const mainYearDir = yearsDir.getChild(postYear);
      if (mainYearDir) {
        const postLink = mainYearDir.addChild(new VirtualNode(postFileName, 'file', postContent));
        postLink.setMetadata('linkMapNode', postData);
        postLink.setUrl(postData.url);
      }
    }

    // Create edges file showing connections
    const edgesContent = this.generateEdgesContent(data.edges);
    contentDir.addChild(new VirtualNode('connections.txt', 'file', edgesContent));
  }

  /**
   * Create a minimal file system when data loading fails
   */
  createMinimalFileSystem() {
    this.root = new VirtualNode('/', 'directory');
    const homeDir = this.root.addChild(new VirtualNode('home', 'directory'));
    const userDir = homeDir.addChild(new VirtualNode('codecube-user', 'directory'));
    
    // Add a welcome file
    userDir.addChild(new VirtualNode('welcome.txt', 'file', 
      'Welcome to CodeCube OS!\n\nThe content data could not be loaded.\nThis may be because:\n- The server is not running\n- Network connectivity issues\n- The linkmap data file is missing\n\nPlease try reloading the page or contact support.'));
    
    // Set current path to user home directory
    this.currentPath = '/home/codecube-user';
  }

  /**
   * Generate content for a post file
   */
  generatePostContent(postData) {
    const publishedDate = postData.publishedOn ? new Date(postData.publishedOn).toDateString() : 'Unknown';
    return `Title: ${postData.title}
Published: ${publishedDate}
URL: ${postData.url || 'N/A'}
Connections: ${postData.connectionCount || 0}

Description:
${postData.description || 'No description available'}

To open this post in your browser, use: open "${postData.title}"`;
  }

  /**
   * Generate content for the edges/connections file
   */
  generateEdgesContent(edges) {
    let content = 'Content Connections\n===================\n\n';
    content += `Total connections: ${edges.length}\n\n`;
    
    for (const edge of edges.slice(0, 50)) { // Limit to first 50 to avoid huge files
      content += `${edge.source} -> ${edge.target}\n`;
    }
    
    if (edges.length > 50) {
      content += `\n... and ${edges.length - 50} more connections`;
    }
    
    return content;
  }

  /**
   * Sanitize filename for file system use
   */
  sanitizeFileName(name) {
    return name.replace(/[^a-zA-Z0-9\-_\.]/g, '_').replace(/_+/g, '_');
  }

  /**
   * Get current working directory
   */
  getCurrentDirectory() {
    if (!this.root) {
      return null;
    }
    return this.root.findByPath(this.currentPath);
  }

  /**
   * Change current directory
   */
  changeDirectory(path) {
    if (!this.root) {
      throw new Error('VFS not initialized');
    }

    let targetNode;
    if (path.startsWith('/')) {
      targetNode = this.root.findByPath(path);
    } else {
      const currentDir = this.getCurrentDirectory();
      targetNode = currentDir ? currentDir.findByPath(path) : null;
    }

    if (!targetNode) {
      throw new Error(`Directory not found: ${path}`);
    }

    if (!targetNode.isDirectory()) {
      throw new Error(`Not a directory: ${path}`);
    }

    this.currentPath = targetNode.getPath();
    return targetNode;
  }

  /**
   * List directory contents
   */
  listDirectory(path = null) {
    if (!this.root) {
      throw new Error('VFS not initialized');
    }

    let targetDir;
    if (path) {
      if (path.startsWith('/')) {
        targetDir = this.root.findByPath(path);
      } else {
        const currentDir = this.getCurrentDirectory();
        targetDir = currentDir ? currentDir.findByPath(path) : null;
      }
    } else {
      targetDir = this.getCurrentDirectory();
    }

    if (!targetDir) {
      throw new Error(`Directory not found: ${path || this.currentPath}`);
    }

    if (!targetDir.isDirectory()) {
      throw new Error(`Not a directory: ${path || this.currentPath}`);
    }

    return targetDir.getChildren();
  }

  /**
   * Read file content
   */
  readFile(path) {
    if (!this.root) {
      throw new Error('VFS not initialized');
    }

    let targetFile;
    if (path.startsWith('/')) {
      targetFile = this.root.findByPath(path);
    } else {
      const currentDir = this.getCurrentDirectory();
      targetFile = currentDir ? currentDir.findByPath(path) : null;
    }

    if (!targetFile) {
      throw new Error(`File not found: ${path}`);
    }

    if (!targetFile.isFile()) {
      throw new Error(`Not a file: ${path}`);
    }

    return targetFile.content;
  }

  /**
   * Find files/directories by name pattern
   */
  find(pattern, startPath = '/') {
    if (!this.root) {
      throw new Error('VFS not initialized');
    }

    const results = [];
    const regex = new RegExp(pattern.replace(/\*/g, '.*'), 'i');
    
    const search = (node) => {
      if (regex.test(node.name)) {
        results.push(node);
      }
      
      if (node.isDirectory()) {
        for (const child of node.getChildren()) {
          search(child);
        }
      }
    };

    const startNode = this.root.findByPath(startPath);
    if (startNode) {
      search(startNode);
    }

    return results;
  }

  /**
   * Get current path
   */
  getCurrentPath() {
    return this.currentPath;
  }

  /**
   * Check if VFS is initialized
   */
  isInitialized() {
    return this.initialized;
  }

  /**
   * Reload data from source
   */
  async reload() {
    this.dataSource.clearCache();
    this.initialized = false;
    await this.initialize();
  }
}