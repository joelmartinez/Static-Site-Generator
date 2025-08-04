module.exports = {
  // Test environment
  testEnvironment: 'node',
  
  // Test file patterns
  testMatch: [
    '**/tests/**/*.test.js',
    '**/src/**/*.test.js'
  ],
  
  // Coverage settings
  collectCoverage: false,
  
  // Clear mocks between tests
  clearMocks: true,
  
  // Verbose output for CI/CD
  verbose: true,
  
  // Transform settings for ES modules
  transform: {
    '^.+\\.js$': 'babel-jest'
  },
  
  // Module file extensions
  moduleFileExtensions: ['js', 'json'],
  
  // Test timeout
  testTimeout: 10000
};