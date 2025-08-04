// Clear command is special - it needs to communicate with the terminal component
export default function clear(args) {
  return { special: 'clear' };
}