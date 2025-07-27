// CLI: node validate.js <scenario>
// Basic JSON validation for interactive flows

const fs = require('fs');
const path = require('path');

function validateFlow(scenario) {
    if (!scenario) {
        console.error('Usage: node validate.js <scenario>');
        process.exit(1);
    }
    
    const basePath = path.join(process.cwd(), 'interactive-flows', 'content', scenario);
    
    if (!fs.existsSync(basePath)) {
        console.error(`Flow directory not found: ${basePath}`);
        process.exit(1);
    }
    
    const files = ['nodes.json'];
    let allValid = true;
    
    files.forEach(file => {
        const filePath = path.join(basePath, file);
        if (!fs.existsSync(filePath)) {
            console.error(`Missing file: ${file}`);
            allValid = false;
            return;
        }
        
        try {
            const content = fs.readFileSync(filePath, 'utf8');
            JSON.parse(content);
            console.log(`✓ ${file} - valid JSON`);
        } catch (error) {
            console.error(`✗ ${file} - invalid JSON: ${error.message}`);
            allValid = false;
        }
    });
    
    if (allValid) {
        console.log('All good! Basic validation passed.');
    } else {
        console.error('Validation failed.');
        process.exit(1);
    }
}

// Get command line arguments
const args = process.argv.slice(2);
validateFlow(args[0]);