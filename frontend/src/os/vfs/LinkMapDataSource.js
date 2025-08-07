/**
 * Data source for loading linkmap data
 */
export class LinkMapDataSource {
  constructor() {
    this.data = null;
    this.lastFetched = null;
    this.cacheExpiry = 5 * 60 * 1000; // 5 minutes
  }

  /**
   * Load linkmap data from the server
   */
  async loadData() {
    try {
      // Check if we have cached data that's still valid
      if (this.data && this.lastFetched && 
          (Date.now() - this.lastFetched < this.cacheExpiry)) {
        return this.data;
      }

      // First try to get data from window.linkMapData (if already loaded)
      if (typeof window !== 'undefined' && window.linkMapData) {
        this.data = window.linkMapData;
        this.lastFetched = Date.now();
        return this.data;
      }

      // Fetch from server
      const response = await fetch('/script/map.gen.js');
      if (!response.ok) {
        throw new Error(`Failed to fetch linkmap data: ${response.status}`);
      }

      const scriptText = await response.text();
      
      // Extract JSON data from the script
      // The script contains: window.linkMapData = {json data here};
      const match = scriptText.match(/window\.linkMapData\s*=\s*({.*});/s);
      if (!match) {
        throw new Error('Could not parse linkmap data from script');
      }

      try {
        this.data = JSON.parse(match[1]);
        this.lastFetched = Date.now();
        return this.data;
      } catch (parseError) {
        throw new Error(`Failed to parse linkmap JSON: ${parseError.message}`);
      }

    } catch (error) {
      console.error('Failed to load linkmap data:', error);
      
      // Return mock data for development/testing
      return this.getMockData();
    }
  }

  /**
   * Get mock data for testing when real data isn't available
   */
  getMockData() {
    if (!this.data) {
      this.data = {
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
          }
        ]
      };
      this.lastFetched = Date.now();
    }
    return this.data;
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
   * Check if data is available (cached or from window)
   */
  isDataAvailable() {
    return this.data !== null || 
           (typeof window !== 'undefined' && window.linkMapData);
  }
}