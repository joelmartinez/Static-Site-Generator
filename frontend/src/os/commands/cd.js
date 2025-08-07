import { getVFS } from '../vfs/index.js';

export default async function cd(args) {
  try {
    const vfs = getVFS();
    
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      await vfs.initialize();
    }

    // If no arguments, go to root
    const path = args.length > 0 ? args[0] : '/';
    
    try {
      vfs.changeDirectory(path);
      return ''; // cd typically doesn't output anything on success
    } catch (error) {
      return `cd: ${error.message}`;
    }
    
  } catch (error) {
    return `cd: Failed to initialize file system: ${error.message}`;
  }
}