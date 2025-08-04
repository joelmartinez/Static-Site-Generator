module.exports = {
  // Test environment for React components
  testEnvironment: 'jsdom',
  
  // Test file patterns
  testMatch: [
    '**/tests/**/*.test.js',
    '**/src/**/*.test.js'
  ],
  
  // Setup files for React testing
  setupFilesAfterEnv: ['<rootDir>/tests/setup.js'],
  
  // Coverage settings
  collectCoverage: false,
  
  // Clear mocks between tests
  clearMocks: true,
  
  // Verbose output for CI/CD
  verbose: true,
  
  // Transform settings for ES modules and JSX
  transform: {
    '^.+\\.(js|jsx)$': 'babel-jest'
  },
  
  // Module file extensions
  moduleFileExtensions: ['js', 'jsx', 'json'],
  
  // Test timeout
  testTimeout: 10000
};