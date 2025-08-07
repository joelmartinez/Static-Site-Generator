import vfs from '../VirtualFileSystem.js';

export default function cat(args) {
  try {
    // Check if VFS is initialized
    if (!vfs.isInitialized()) {
      return 'Virtual file system not initialized. Loading...';
    }

    // Check if file argument provided
    if (args.length === 0) {
      return 'cat: missing file operand\nUsage: cat <file>';
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
      return `cat: ${fileName}: No such file or directory`;
    }

    if (node.isDirectory) {
      return `cat: ${fileName}: Is a directory`;
    }

    // Get and return the content
    const content = vfs.getNodeContent(node);
    return content;

  } catch (error) {
    return `cat: ${error.message}`;
  }
}