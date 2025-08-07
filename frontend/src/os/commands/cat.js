import { getVFS } from '../vfs/index.js';

export default async function cat(args) {
  try {
    const vfs = getVFS();
    
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      await vfs.initialize();
    }

    if (args.length === 0) {
      return 'cat: missing file operand\nUsage: cat <filename>';
    }

    const filename = args[0];
    
    try {
      // Try to find the node (could be file or directory)
      let targetNode;
      if (filename.startsWith('/')) {
        targetNode = vfs.root.findByPath(filename);
      } else {
        const currentDir = vfs.getCurrentDirectory();
        targetNode = currentDir ? currentDir.findByPath(filename) : null;
      }

      if (!targetNode) {
        return `cat: ${filename}: No such file or directory`;
      }

      // Use the new getDisplayContent method to cat any node type
      return targetNode.getDisplayContent();
    } catch (error) {
      return `cat: ${error.message}`;
    }
    
  } catch (error) {
    return `cat: Failed to initialize file system: ${error.message}`;
  }
}