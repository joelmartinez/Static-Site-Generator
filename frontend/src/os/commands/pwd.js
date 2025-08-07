import { getVFS } from '../vfs/index.js';

export default async function pwd(args) {
  try {
    const vfs = getVFS();
    
    // Initialize VFS if not already done
    if (!vfs.isInitialized()) {
      await vfs.initialize();
    }

    return vfs.getCurrentPath();
    
  } catch (error) {
    // Fallback to default path if VFS fails
    return '/home/codecube-user';
  }
}