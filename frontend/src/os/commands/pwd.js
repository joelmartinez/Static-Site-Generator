import vfs from '../VirtualFileSystem.js';

export default function pwd(args) {
  try {
    if (!vfs.isInitialized()) {
      return '/home/codecube-user';
    }
    
    const currentPath = vfs.getCurrentPath();
    return `/home/codecube-user${currentPath}`;
  } catch (error) {
    return '/home/codecube-user';
  }
}