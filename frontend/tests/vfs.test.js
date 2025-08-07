import { VirtualNode } from '../src/os/vfs/VirtualNode.js';
import { LinkMapDataSource } from '../src/os/vfs/LinkMapDataSource.js';
import { VirtualFileSystem } from '../src/os/vfs/VirtualFileSystem.js';
import { MockDataSource } from './MockDataSource.js';

describe('VirtualNode', () => {
  test('should create a file node', () => {
    const node = new VirtualNode('test.txt', 'file', 'Hello World');
    expect(node.name).toBe('test.txt');
    expect(node.type).toBe('file');
    expect(node.content).toBe('Hello World');
    expect(node.isFile()).toBe(true);
    expect(node.isDirectory()).toBe(false);
  });

  test('should create a directory node', () => {
    const node = new VirtualNode('testdir', 'directory');
    expect(node.name).toBe('testdir');
    expect(node.type).toBe('directory');
    expect(node.isDirectory()).toBe(true);
    expect(node.isFile()).toBe(false);
  });

  test('should add and retrieve children', () => {
    const parent = new VirtualNode('parent', 'directory');
    const child = new VirtualNode('child.txt', 'file', 'content');
    
    parent.addChild(child);
    
    expect(parent.hasChildren()).toBe(true);
    expect(parent.getChild('child.txt')).toBe(child);
    expect(child.parent).toBe(parent);
  });

  test('should get correct paths', () => {
    const root = new VirtualNode('/', 'directory');
    const content = root.addChild(new VirtualNode('content', 'directory'));
    const file = content.addChild(new VirtualNode('test.txt', 'file', 'test'));
    
    expect(root.getPath()).toBe('/');
    expect(content.getPath()).toBe('/content');
    expect(file.getPath()).toBe('/content/test.txt');
  });

  test('should find nodes by path', () => {
    const root = new VirtualNode('/', 'directory');
    const content = root.addChild(new VirtualNode('content', 'directory'));
    const file = content.addChild(new VirtualNode('test.txt', 'file', 'test'));
    
    expect(root.findByPath('content')).toBe(content);
    expect(root.findByPath('content/test.txt')).toBe(file);
    expect(root.findByPath('/content/test.txt')).toBe(file);
    expect(root.findByPath('nonexistent')).toBe(null);
  });

  test('should handle metadata and URLs', () => {
    const node = new VirtualNode('test.txt', 'file', 'content');
    
    node.setMetadata('type', 'post');
    node.setUrl('https://example.com/test');
    
    expect(node.getMetadata('type')).toBe('post');
    expect(node.getUrl()).toBe('https://example.com/test');
  });

  test('should provide display names for different node types', () => {
    const file = new VirtualNode('test.txt', 'file', 'content');
    const dir = new VirtualNode('testdir', 'directory');
    
    expect(file.getDisplayName()).toBe('test.txt');
    expect(dir.getDisplayName()).toBe('testdir/');
  });

  test('should provide display content for cat functionality', () => {
    const file = new VirtualNode('test.txt', 'file', 'Hello World');
    const dir = new VirtualNode('testdir', 'directory');
    
    expect(file.getDisplayContent()).toBe('Hello World');
    expect(dir.getDisplayContent()).toContain('Directory: /testdir');
    expect(dir.getDisplayContent()).toContain('Contents (0 items)');
  });

  test('should handle relative paths with .. and ../', () => {
    const root = new VirtualNode('/', 'directory');
    const home = root.addChild(new VirtualNode('home', 'directory'));
    const user = home.addChild(new VirtualNode('user', 'directory'));
    const docs = user.addChild(new VirtualNode('documents', 'directory'));
    
    // Test .. navigation
    expect(docs.findByPath('..')).toBe(user);
    expect(docs.findByPath('../..')).toBe(home);
    expect(docs.findByPath('../../..')).toBe(root);
    
    // Test ../ with additional path
    expect(docs.findByPath('../documents')).toBe(docs);
    expect(user.findByPath('../user/documents')).toBe(docs);
  });
});

describe('LinkMapDataSource', () => {
  test('should fail gracefully when real data unavailable', async () => {
    const dataSource = new LinkMapDataSource();
    
    // Should throw error instead of returning mock data
    await expect(dataSource.loadData()).rejects.toThrow('Unable to load linkmap data');
  });

  test('should clear cache', () => {
    const dataSource = new LinkMapDataSource();
    
    // Set some dummy data
    dataSource.data = { nodes: [], edges: [] };
    expect(dataSource.getCachedData()).toBeDefined();
    
    dataSource.clearCache();
    expect(dataSource.getCachedData()).toBe(null);
  });
});

describe('MockDataSource', () => {
  test('should return mock data for testing', async () => {
    const dataSource = new MockDataSource();
    const data = await dataSource.loadData();
    
    expect(data).toBeDefined();
    expect(data.nodes).toBeDefined();
    expect(data.edges).toBeDefined();
    expect(Array.isArray(data.nodes)).toBe(true);
    expect(Array.isArray(data.edges)).toBe(true);
    expect(data.nodes.length).toBeGreaterThan(0);
  });

  test('should cache data', async () => {
    const dataSource = new MockDataSource();
    
    const data1 = await dataSource.loadData();
    const data2 = await dataSource.loadData();
    
    expect(data1).toBe(data2); // Should be same object reference due to caching
  });

  test('should clear cache', async () => {
    const dataSource = new MockDataSource();
    
    await dataSource.loadData();
    expect(dataSource.getCachedData()).toBeDefined();
    
    dataSource.clearCache();
    expect(dataSource.getCachedData()).toBe(null);
  });
});

describe('VirtualFileSystem', () => {
  let vfs;

  beforeEach(async () => {
    // Create VFS with mock data source for testing
    vfs = new VirtualFileSystem();
    vfs.dataSource = new MockDataSource(); // Replace with mock data source
    await vfs.initialize();
  });

  test('should initialize with directory structure', () => {
    expect(vfs.isInitialized()).toBe(true);
    expect(vfs.getCurrentPath()).toBe('/');
    
    const contentDir = vfs.listDirectory('/content');
    expect(contentDir).toBeDefined();
    expect(contentDir.length).toBeGreaterThan(0);
    
    const subdirs = contentDir.map(d => d.name);
    expect(subdirs).toContain('posts');
    expect(subdirs).toContain('years');
    expect(subdirs).toContain('categories');
    expect(subdirs).toContain('entities');
  });

  test('should change directories with relative navigation', () => {
    expect(() => vfs.changeDirectory('/content')).not.toThrow();
    expect(vfs.getCurrentPath()).toBe('/content');
    
    expect(() => vfs.changeDirectory('posts')).not.toThrow();
    expect(vfs.getCurrentPath()).toBe('/content/posts');
    
    expect(() => vfs.changeDirectory('..')).not.toThrow();
    expect(vfs.getCurrentPath()).toBe('/content');
    
    // Test ../.. navigation
    vfs.changeDirectory('posts');
    expect(() => vfs.changeDirectory('../..')).not.toThrow();
    expect(vfs.getCurrentPath()).toBe('/');
    
    // Test ../ with path
    vfs.changeDirectory('/content/posts');
    expect(() => vfs.changeDirectory('../years')).not.toThrow();
    expect(vfs.getCurrentPath()).toBe('/content/years');
  });

  test('should list directory contents', () => {
    const rootContents = vfs.listDirectory('/');
    expect(rootContents.length).toBeGreaterThan(0);
    expect(rootContents.some(item => item.name === 'content')).toBe(true);
    
    const contentContents = vfs.listDirectory('/content');
    expect(contentContents.length).toBeGreaterThan(0);
  });

  test('should cat files and directories', () => {
    // Test reading a file
    const contentContents = vfs.listDirectory('/content');
    const connectionsFile = contentContents.find(item => item.name === 'connections.txt');
    
    if (connectionsFile) {
      const content = vfs.readFile('/content/connections.txt');
      expect(typeof content).toBe('string');
      expect(content.length).toBeGreaterThan(0);
    }
    
    // Test that directories can also provide display content
    const rootNode = vfs.root;
    const displayContent = rootNode.getDisplayContent();
    expect(displayContent).toContain('Directory: /');
    expect(displayContent).toContain('Contents');
  });

  test('should handle file not found errors', () => {
    expect(() => vfs.readFile('/nonexistent.txt')).toThrow('File not found');
    expect(() => vfs.listDirectory('/nonexistent')).toThrow('Directory not found');
    expect(() => vfs.changeDirectory('/nonexistent')).toThrow('Directory not found');
  });

  test('should find files by pattern', () => {
    const results = vfs.find('*.txt');
    expect(Array.isArray(results)).toBe(true);
    
    const infoResults = vfs.find('info*');
    expect(infoResults.length).toBeGreaterThan(0);
  });

  test('should sanitize filenames', () => {
    expect(vfs.sanitizeFileName('Test & Example')).toBe('Test_Example');
    expect(vfs.sanitizeFileName('Special/Characters?')).toBe('Special_Characters_');
    expect(vfs.sanitizeFileName('Normal-Name_123')).toBe('Normal-Name_123');
  });
});

describe('VFS Commands Integration', () => {
  // These would be integration tests for the actual commands
  // For now, just ensure the VFS can be imported and used
  test('should be able to import VFS', async () => {
    const vfsModule = await import('../src/os/vfs/index.js');
    const { getVFS, initializeVFS } = vfsModule;
    
    expect(getVFS).toBeDefined();
    expect(initializeVFS).toBeDefined();
    
    const vfs = getVFS();
    expect(vfs).toBeDefined();
  });
});