import { registerCommand } from './registry.js';
import help from './help.js';
import echo from './echo.js';
import whoami from './whoami.js';
import pwd from './pwd.js';
import ls from './ls.js';
import cat from './cat.js';
import cd from './cd.js';
import open from './open.js';
import date from './date.js';
import about from './about.js';
import clear from './clear.js';

// Register all available commands with their descriptions
registerCommand('help', help, 'Show this help message');
registerCommand('echo', echo, 'Echo back the arguments');
registerCommand('whoami', whoami, 'Display current user');
registerCommand('pwd', pwd, 'Show current directory');
registerCommand('ls', ls, 'List directory contents (use -l for details)');
registerCommand('cat', cat, 'Display file contents');
registerCommand('cd', cd, 'Change directory');
registerCommand('open', open, 'Open URL for posts/nodes in browser');
registerCommand('date', date, 'Show current date and time');
registerCommand('about', about, 'About CodeCube OS');
registerCommand('clear', clear, 'Clear the terminal');

// Export the registry functions for use by terminal
export { executeCommand, getCommands } from './registry.js';