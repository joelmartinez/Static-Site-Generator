/**
 * CodeCube Frontend Entry Point
 * 
 * This is the main entry point for the CodeCube frontend application.
 * Future frontend functionality can be added here or imported from other modules.
 */

// Import utility functions (example of how to structure larger frontend apps)
import { addInteractiveElements, initializeThemeToggle, enhanceNavigation } from './utils.js';

// Import map functionality
import { createLinkMapVisualization } from './map.js';

// Simple initialization function
function initializeCodeCube() {
    console.log('CodeCube frontend initialized - modern build pipeline active!');
    
    // Make map function globally available for the map page
    window.createLinkMapVisualization = createLinkMapVisualization;
    
    // Add any initialization logic here
    // For example: analytics, theme switching, interactive elements, etc.
    
    // Sample: Add a simple interaction to demonstrate the bundled JS is working
    document.addEventListener('DOMContentLoaded', function() {
        // This is just a placeholder for future functionality
        const body = document.body;
        if (body) {
            body.setAttribute('data-codecube-frontend', 'loaded');
            console.log('CodeCube frontend: DOM ready, attributes set');
        }
        
        // Initialize enhanced features (commented out as examples)
        // Uncomment these when the corresponding HTML elements exist:
        // addInteractiveElements();
        // initializeThemeToggle();
        // enhanceNavigation();
    });
}

// Initialize when script loads
initializeCodeCube();

// Export for potential future module usage
export { initializeCodeCube };