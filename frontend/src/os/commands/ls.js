import { getVFS } from '../vfs/index.js';

export default async function ls(args) {
  try {
    const vfs = getVFS();
    
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      await vfs.initialize();
    }

    // Parse arguments
    const path = args.length > 0 ? args[0] : null;
    const showDetails = args.includes('-l');
    const showAll = args.includes('-a');

    // Get directory contents
    const entries = vfs.listDirectory(path);
    
    if (entries.length === 0) {
      return 'Directory is empty.';
    }

    // Format output
    let output = '';
    
    if (showDetails) {
      // Detailed listing
      for (const entry of entries) {
        const type = entry.isDirectory() ? 'd' : '-';
        const name = entry.name;
        const size = entry.isFile() ? entry.content.length : '-';
        const hasUrl = entry.getUrl() ? '*' : ' ';
        
        output += `${type}${hasUrl} ${size.toString().padStart(8)} ${name}\n`;
      }
    } else {
      // Simple listing
      const names = entries.map(entry => {
        const name = entry.name;
        if (entry.isDirectory()) {
          return name + '/';
        } else if (entry.getUrl()) {
          return name + '*';
        } else {
          return name;
        }
      });
      
      // Arrange in columns
      const maxWidth = Math.max(...names.map(n => n.length));
      const terminalWidth = 80; // Assume 80 character terminal
      const columns = Math.floor(terminalWidth / (maxWidth + 2));
      
      if (columns <= 1) {
        output = names.join('\n');
      } else {
        for (let i = 0; i < names.length; i += columns) {
          const row = names.slice(i, i + columns);
          output += row.map(name => name.padEnd(maxWidth + 2)).join('') + '\n';
        }
      }
    }

    return output.trim();
    
  } catch (error) {
    return `ls: ${error.message}`;
  }
}