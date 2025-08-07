// Command registry for the terminal
const commands = new Map();

export const registerCommand = (name, handler, description) => {
  commands.set(name.toLowerCase(), { handler, description });
};

export const executeCommand = async (commandLine) => {
  const trimmedCommand = commandLine.trim();
  if (!trimmedCommand) return '';
  
  const parts = trimmedCommand.split(' ');
  const commandName = parts[0].toLowerCase();
  const args = parts.slice(1);
  
  const command = commands.get(commandName);
  if (command) {
    try {
      const result = await command.handler(args);
      return result;
    } catch (error) {
      return `Error executing ${commandName}: ${error.message}`;
    }
  } else {
    return `Command not found: ${commandName}. Type "help" for available commands.`;
  }
};

export const getCommands = () => {
  return Array.from(commands.keys()).sort();
};

export const getCommandHelp = (commandName) => {
  const command = commands.get(commandName.toLowerCase());
  return command ? command.description : 'No description available';
};

export const getAllCommandsWithHelp = () => {
  return Array.from(commands.entries()).map(([name, { description }]) => ({
    name,
    description
  })).sort((a, b) => a.name.localeCompare(b.name));
};