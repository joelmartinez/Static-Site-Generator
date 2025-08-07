import vfs from '../VirtualFileSystem.js';

export default function open(args) {
  try {
    // Check if VFS is initialized
    if (!vfs.isInitialized()) {
      return 'Virtual file system not initialized. Loading...';
    }

    // Check if file argument provided
    if (args.length === 0) {
      return 'open: missing file operand\nUsage: open <file>';
    }

    const fileName = args[0];
    let filePath = fileName;

    // Handle relative paths
    if (!filePath.startsWith('/')) {
      if (vfs.getCurrentPath() === '/') {
        filePath = '/' + filePath;
      } else {
        filePath = vfs.getCurrentPath() + '/' + filePath;
      }
    }

    // Get the node
    const node = vfs.getNode(filePath);
    
    if (!node) {
      return `open: ${fileName}: No such file or directory`;
    }

    if (node.isDirectory) {
      return `open: ${fileName}: Is a directory (use 'cd' to navigate)`;
    }

    // Check if the node has a URL
    if (!node.linkMapNode || !node.linkMapNode.url) {
      return `open: ${fileName}: No URL available for this item`;
    }

    // Open the URL
    const url = node.linkMapNode.url;
    
    try {
      window.open(url, '_blank');
      return `Opening: ${url}`;
    } catch (error) {
      return `open: Failed to open URL: ${url}`;
    }

  } catch (error) {
    return `open: ${error.message}`;
  }
}