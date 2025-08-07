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
      const content = vfs.readFile(filename);
      return content;
    } catch (error) {
      return `cat: ${error.message}`;
    }
    
  } catch (error) {
    return `cat: Failed to initialize file system: ${error.message}`;
  }
}