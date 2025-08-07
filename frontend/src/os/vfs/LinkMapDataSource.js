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
      console.warn('Failed to load linkmap data:', error);
      
      // In production, throw the error so the VFS can handle it appropriately
      throw new Error(`Unable to load linkmap data: ${error.message}`);
    }
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