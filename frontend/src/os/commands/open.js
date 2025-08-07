import { getVFS } from '../vfs/index.js';

export default async function open(args) {
  try {
    const vfs = getVFS();
    
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      await vfs.initialize();
    }

    if (args.length === 0) {
      return 'open: missing file operand\nUsage: open <filename or title>';
    }

    const target = args.join(' '); // Join all args in case title has spaces
    
    try {
      // First try to find by exact path
      let targetNode = null;
      
      try {
        const currentDir = vfs.getCurrentDirectory();
        targetNode = currentDir ? currentDir.findByPath(target) : null;
        
        if (!targetNode && !target.startsWith('/')) {
          // Try absolute path
          targetNode = vfs.root?.findByPath('/' + target);
        }
      } catch (e) {
        // Path not found, try searching by title
      }

      // If not found by path, search by title/name
      if (!targetNode) {
        const searchResults = vfs.find(target);
        
        // Look for exact title match first
        targetNode = searchResults.find(node => {
          const linkMapNode = node.getMetadata('linkMapNode');
          return linkMapNode && linkMapNode.title.toLowerCase() === target.toLowerCase();
        });

        // If no exact match, try partial match
        if (!targetNode && searchResults.length > 0) {
          targetNode = searchResults.find(node => {
            const linkMapNode = node.getMetadata('linkMapNode');
            return linkMapNode && linkMapNode.title.toLowerCase().includes(target.toLowerCase());
          });
        }

        // If still no match, take first result
        if (!targetNode && searchResults.length > 0) {
          targetNode = searchResults[0];
        }
      }

      if (!targetNode) {
        return `open: could not find '${target}'`;
      }

      const url = targetNode.getUrl();
      if (!url) {
        return `open: '${targetNode.name}' has no associated URL`;
      }

      // Open the URL in a new window/tab
      if (typeof window !== 'undefined') {
        window.open(url, '_blank');
        return `Opening ${targetNode.name} in browser: ${url}`;
      } else {
        return `URL: ${url}`;
      }
      
    } catch (error) {
      return `open: ${error.message}`;
    }
    
  } catch (error) {
    return `open: Failed to initialize file system: ${error.message}`;
  }
}