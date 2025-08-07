/**
 * Mock data source for testing VFS functionality
 * This replaces the need for mock data in production code
 */
export class MockDataSource {
  constructor() {
    this.data = null;
    this.lastFetched = null;
  }

  /**
   * Load mock data for testing
   */
  async loadData() {
    if (!this.data) {
      this.data = this.getMockData();
      this.lastFetched = Date.now();
    }
    return this.data;
  }

  /**
   * Get mock data for testing
   */
  getMockData() {
    return {
      nodes: [
        {
          id: '#2025',
          title: '2025',
          url: '/2025',
          description: 'Posts from 2025',
          nodeType: 'year',
          connectionCount: 5
        },
        {
          id: '/2025/7/test-post/',
          title: 'Test Post',
          url: 'https://codecube.net/2025/7/test-post/',
          description: 'A test post for development',
          nodeType: 'post',
          publishedOn: '2025-07-15T10:00:00Z',
          connectionCount: 2
        },
        {
          id: '#category-ai',
          title: 'AI',
          url: null,
          description: 'AI related posts',
          nodeType: 'category',
          connectionCount: 3
        },
        {
          id: '#entity-programming',
          title: 'Programming',
          url: null,
          description: 'Programming concepts and tools',
          nodeType: 'entity',
          connectionCount: 10
        },
        {
          id: '/2024/12/another-post/',
          title: 'Another Post',
          url: 'https://codecube.net/2024/12/another-post/',
          description: 'Another test post',
          nodeType: 'post',
          publishedOn: '2024-12-01T08:30:00Z',
          connectionCount: 1
        },
        {
          id: '#2024',
          title: '2024',
          url: '/2024',
          description: 'Posts from 2024',
          nodeType: 'year',
          connectionCount: 3
        }
      ],
      edges: [
        {
          source: '#2025',
          target: '/2025/7/test-post/'
        },
        {
          source: '#category-ai',
          target: '/2025/7/test-post/'
        },
        {
          source: '#2024',
          target: '/2024/12/another-post/'
        },
        {
          source: '#entity-programming',
          target: '/2025/7/test-post/'
        }
      ]
    };
  }

  /**
   * Get cached data without fetching
   */
  getCachedData() {
    return this.data;
  }

  /**
   * Clear cached data
   */
  clearCache() {
    this.data = null;
    this.lastFetched = null;
  }

  /**
   * Check if data is available
   */
  isDataAvailable() {
    return this.data !== null;
  }
}