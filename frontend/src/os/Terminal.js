import React, { useState, useRef, useEffect } from 'react';
import { executeCommand } from './commands';
import vfs from './VirtualFileSystem.js';

const Terminal = () => {
  const [history, setHistory] = useState([
    'CodeCube OS Terminal',
    'Type "help" for available commands.',
    ''
  ]);
  const [currentInput, setCurrentInput] = useState('');
  const [commandHistory, setCommandHistory] = useState([]);
  const [historyIndex, setHistoryIndex] = useState(-1);
  const [currentDir, setCurrentDir] = useState('/');
  const [vfsInitialized, setVfsInitialized] = useState(false);
  const inputRef = useRef(null);
  const terminalRef = useRef(null);

  // Initialize virtual file system
  useEffect(() => {
    const initVfs = async () => {
      try {
        // Wait a bit for linkMapData to be available
        if (typeof window !== 'undefined' && window.linkMapData) {
          await vfs.initialize();
          setVfsInitialized(true);
          setHistory(prev => [...prev, 'Virtual file system initialized. Type "ls" to explore content.', '']);
        } else {
          // Retry after a short delay
          setTimeout(initVfs, 1000);
        }
      } catch (error) {
        console.error('Failed to initialize virtual file system:', error);
        setHistory(prev => [...prev, `VFS Error: ${error.message}`, '']);
      }
    };

    initVfs();
  }, []);

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

  // Update current directory display
  useEffect(() => {
    if (vfsInitialized) {
      setCurrentDir(vfs.getCurrentPath());
    }
  }, [vfsInitialized, history]); // Update when history changes (after commands)

  const getPrompt = () => {
    if (!vfsInitialized) {
      return 'codecube-user@codecube-os:~$';
    }
    return `codecube-user@codecube-os:${currentDir}$`;
  };

  const handleKeyDown = (e) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      const command = currentInput;
      const prompt = getPrompt();
      const newHistory = [...history, `${prompt} ${command}`];
      
      const output = executeCommand(command);
      
      // Handle special commands like clear
      if (output && typeof output === 'object' && output.special === 'clear') {
        setHistory(['CodeCube OS Terminal', 'Type "help" for available commands.', '']);
      } else if (output) {
        newHistory.push(output);
        newHistory.push('');
        setHistory(newHistory);
      } else {
        newHistory.push('');
        setHistory(newHistory);
      }
      
      // Update current directory after command execution
      if (vfsInitialized) {
        setCurrentDir(vfs.getCurrentPath());
      }
      
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
    <div className="os-terminal" ref={terminalRef}>
      {history.map((line, index) => (
        <div key={index} className="terminal-output">
          {line}
        </div>
      ))}
      <div className="terminal-input-line">
        <span className="terminal-prompt">{getPrompt()}</span>
        <input
          ref={inputRef}
          type="text"
          value={currentInput}
          onChange={(e) => setCurrentInput(e.target.value)}
          onKeyDown={handleKeyDown}
          className="terminal-input"
          autoComplete="off"
          spellCheck="false"
        />
      </div>
    </div>
  );
};

export default Terminal;