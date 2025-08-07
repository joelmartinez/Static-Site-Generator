import vfs from '../VirtualFileSystem.js';

export default function ls(args) {
  try {
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      return 'Virtual file system not initialized. Loading...';
    }

    // Determine path to list
    let path = vfs.getCurrentPath();
    if (args.length > 0) {
      // If path argument provided, use it
      path = args[0];
      
      // Handle relative paths
      if (!path.startsWith('/')) {
        if (vfs.getCurrentPath() === '/') {
          path = '/' + path;
        } else {
          path = vfs.getCurrentPath() + '/' + path;
        }
      }
    }

    // Get directory contents
    const items = vfs.listDirectory(path);
    
    if (items.length === 0) {
      return `ls: ${path}: Directory is empty or does not exist`;
    }

    // Format output similar to Unix ls
    let output = '';
    const showDetails = args.includes('-l');
    
    if (showDetails) {
      // Long format
      items.forEach(item => {
        const type = item.isDirectory ? 'd' : '-';
        const size = item.isDirectory ? 
          (item.children ? item.children.length.toString().padStart(4) : '   0') :
          '   1';
        const date = item.linkMapNode ? 
          new Date(item.linkMapNode.publishedOn).toLocaleDateString() : 
          new Date().toLocaleDateString();
        
        output += `${type}rw-r--r-- ${size} ${date.padEnd(10)} ${item.name}\n`;
      });
    } else {
      // Simple format - just names
      const names = items.map(item => 
        item.isDirectory ? item.name + '/' : item.name
      );
      
      // Display in columns if we have many items
      if (names.length > 10) {
        const cols = Math.ceil(Math.sqrt(names.length));
        const rows = Math.ceil(names.length / cols);
        
        for (let row = 0; row < rows; row++) {
          const rowItems = [];
          for (let col = 0; col < cols; col++) {
            const index = col * rows + row;
            if (index < names.length) {
              rowItems.push(names[index].padEnd(20));
            }
          }
          output += rowItems.join(' ').trimEnd() + '\n';
        }
      } else {
        output = names.join('  ');
      }
    }

    return output.trimEnd();
    
  } catch (error) {
    return `ls: ${error.message}`;
  }
}