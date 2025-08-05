import { getAllCommandsWithHelp } from './registry.js';

export default function help(args) {
  const commands = getAllCommandsWithHelp();
  return `Available commands:
  ${commands.map(cmd => `${cmd.name.padEnd(10)} - ${cmd.description}`).join('\n  ')}`;
}