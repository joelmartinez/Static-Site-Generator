// Command registry for the terminal
const commands = new Map();

export const registerCommand = (name, handler) => {
  commands.set(name.toLowerCase(), handler);
};

export const executeCommand = (commandLine) => {
  const trimmedCommand = commandLine.trim();
  if (!trimmedCommand) return '';
  
  const parts = trimmedCommand.split(' ');
  const commandName = parts[0].toLowerCase();
  const args = parts.slice(1);
  
  const handler = commands.get(commandName);
  if (handler) {
    return handler(args);
  } else {
    return `Command not found: ${commandName}. Type "help" for available commands.`;
  }
};

export const getCommands = () => {
  return Array.from(commands.keys()).sort();
};