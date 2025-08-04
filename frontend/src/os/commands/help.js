import { getCommands } from './registry.js';

export default function help(args) {
  const availableCommands = getCommands();
  return `Available commands:
  ${availableCommands.map(cmd => `${cmd.padEnd(10)} - ${getCommandDescription(cmd)}`).join('\n  ')}`;
}

function getCommandDescription(commandName) {
  const descriptions = {
    'help': 'Show this help message',
    'clear': 'Clear the terminal',
    'echo': 'Echo back the arguments',
    'whoami': 'Display current user',
    'pwd': 'Show current directory',
    'ls': 'List directory contents',
    'date': 'Show current date and time',
    'about': 'About CodeCube OS'
  };
  return descriptions[commandName] || 'No description available';
}