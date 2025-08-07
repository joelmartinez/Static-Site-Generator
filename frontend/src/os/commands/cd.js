import vfs from '../VirtualFileSystem.js';

export default function cd(args) {
  try {
    // Check if VFS is initialized
    if (!vfs.isInitialized()) {
      return 'Virtual file system not initialized. Loading...';
    }

    // Default to root if no argument
    let targetPath = '/';
    
    if (args.length > 0) {
      targetPath = args[0];
    }

    // Change directory
    const newPath = vfs.changeDirectory(targetPath);
    
    // Return empty string for successful cd (like real cd command)
    return '';

  } catch (error) {
    return error.message;
  }
}