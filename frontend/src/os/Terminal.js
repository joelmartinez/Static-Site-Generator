import React, { useState, useRef, useEffect } from 'react';
import { makeStyles } from '@fluentui/react-components';

const useStyles = makeStyles({
  terminal: {
    height: '100%',
    backgroundColor: '#0c0c0c',
    color: '#cccccc',
    fontFamily: '"Cascadia Code", "Courier New", monospace',
    fontSize: '14px',
    padding: '8px',
    overflow: 'auto',
    whiteSpace: 'pre-wrap',
    wordBreak: 'break-word'
  },
  output: {
    marginBottom: '4px'
  },
  inputLine: {
    display: 'flex',
    alignItems: 'center',
    marginTop: '4px'
  },
  prompt: {
    color: '#0dbc79',
    marginRight: '8px',
    flexShrink: 0
  },
  input: {
    backgroundColor: 'transparent',
    border: 'none',
    outline: 'none',
    color: '#cccccc',
    fontFamily: 'inherit',
    fontSize: 'inherit',
    flex: 1,
    caretColor: '#cccccc'
  },
  cursor: {
    backgroundColor: '#cccccc',
    width: '8px',
    height: '16px',
    display: 'inline-block',
    animation: 'blink 1s infinite'
  }
});

const Terminal = () => {
  const classes = useStyles();
  const [history, setHistory] = useState([
    'CodeCube OS Terminal v1.0.0',
    'Type "help" for available commands.',
    ''
  ]);
  const [currentInput, setCurrentInput] = useState('');
  const [commandHistory, setCommandHistory] = useState([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const inputRef = useRef(null);
  const terminalRef = useRef(null);

  // Focus input when terminal is clicked
  useEffect(() => {
    const focusInput = () => {
      if (inputRef.current) {
        inputRef.current.focus();
      }
    };

    const terminal = terminalRef.current;
    if (terminal) {
      terminal.addEventListener('click', focusInput);
      focusInput(); // Focus immediately
      
      return () => {
        terminal.removeEventListener('click', focusInput);
      };
    }
  }, []);

  // Auto-scroll to bottom when history updates
  useEffect(() => {
    if (terminalRef.current) {
      terminalRef.current.scrollTop = terminalRef.current.scrollHeight;
    }
  }, [history]);

  const processCommand = (command) => {
    const trimmedCommand = command.trim();
    if (!trimmedCommand) return '';
    
    const parts = trimmedCommand.split(' ');
    const cmd = parts[0].toLowerCase();
    const args = parts.slice(1);

    switch (cmd) {
      case 'help':
        return `Available commands:
  help     - Show this help message
  clear    - Clear the terminal
  echo     - Echo back the arguments
  whoami   - Display current user
  pwd      - Show current directory
  ls       - List directory contents
  date     - Show current date and time
  about    - About CodeCube OS`;
      case 'clear':
        setHistory(['CodeCube OS Terminal v1.0.0', 'Type "help" for available commands.', '']);
        return null;
      case 'echo':
        return args.join(' ');
      case 'whoami':
        return 'codecube-user';
      case 'pwd':
        return '/home/codecube-user';
      case 'ls':
        return `documents/  projects/  downloads/  .config/`;
      case 'date':
        return new Date().toString();
      case 'about':
        return `CodeCube OS - A hybrid terminal environment
Built for rich interaction with web content
Version 1.0.0 - Powered by React and Fluent UI
Created by Joel Martinez - CodeCube Ventures`;
      default:
        return `Command not found: ${cmd}. Type "help" for available commands.`;
    }
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const command = currentInput;
      const newHistory = [...history, `codecube-user@codecube-os:~$ ${command}`];
      
      const output = processCommand(command);
      if (output !== null) {
        if (output) {
          newHistory.push(output);
        }
        newHistory.push('');
      }
      
      setHistory(newHistory);
      
      // Add to command history if not empty
      if (command.trim()) {
        setCommandHistory(prev => [...prev, command]);
      }
      setHistoryIndex(-1);
      setCurrentInput('');
    } else if (e.key === 'ArrowUp') {
      e.preventDefault();
      if (commandHistory.length > 0) {
        const newIndex = historyIndex === -1 ? commandHistory.length - 1 : Math.max(0, historyIndex - 1);
        setHistoryIndex(newIndex);
        setCurrentInput(commandHistory[newIndex]);
      }
    } else if (e.key === 'ArrowDown') {
      e.preventDefault();
      if (historyIndex !== -1) {
        const newIndex = historyIndex + 1;
        if (newIndex >= commandHistory.length) {
          setHistoryIndex(-1);
          setCurrentInput('');
        } else {
          setHistoryIndex(newIndex);
          setCurrentInput(commandHistory[newIndex]);
        }
      }
    }
  };

  return (
    <div className={classes.terminal} ref={terminalRef}>
      {history.map((line, index) => (
        <div key={index} className={classes.output}>
          {line}
        </div>
      ))}
      <div className={classes.inputLine}>
        <span className={classes.prompt}>codecube-user@codecube-os:~$</span>
        <input
          ref={inputRef}
          type="text"
          value={currentInput}
          onChange={(e) => setCurrentInput(e.target.value)}
          onKeyDown={handleKeyDown}
          className={classes.input}
          autoComplete="off"
          spellCheck="false"
        />
      </div>
      <style>
        {`
          @keyframes blink {
            0%, 50% { opacity: 1; }
            51%, 100% { opacity: 0; }
          }
        `}
      </style>
    </div>
  );
};

export default Terminal;