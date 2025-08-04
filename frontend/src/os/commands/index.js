import { registerCommand } from './registry.js';
import help from './help.js';
import echo from './echo.js';
import whoami from './whoami.js';
import pwd from './pwd.js';
import ls from './ls.js';
import date from './date.js';
import about from './about.js';
import clear from './clear.js';

// Register all available commands
registerCommand('help', help);
registerCommand('echo', echo);
registerCommand('whoami', whoami);
registerCommand('pwd', pwd);
registerCommand('ls', ls);
registerCommand('date', date);
registerCommand('about', about);
registerCommand('clear', clear);

// Export the registry functions for use by terminal
export { executeCommand, getCommands } from './registry.js';