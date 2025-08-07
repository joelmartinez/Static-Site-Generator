import { VirtualNode } from './VirtualNode.js';
import { LinkMapDataSource } from './LinkMapDataSource.js';
import { VirtualFileSystem } from './VirtualFileSystem.js';

// Global VFS instance
let globalVFS = null;

/**
 * Get or create the global VFS instance
 */
export function getVFS() {
  if (!globalVFS) {
    globalVFS = new VirtualFileSystem();
  }
  return globalVFS;
}

/**
 * Initialize the global VFS
 */
export async function initializeVFS() {
  const vfs = getVFS();
  if (!vfs.isInitialized()) {
    await vfs.initialize();
  }
  return vfs;
}