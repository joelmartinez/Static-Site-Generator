import React, { useState, useRef, useEffect } from 'react';
import { makeStyles } from '@fluentui/react-components';
import Terminal from './Terminal';

const useStyles = makeStyles({
  osContainer: {
    height: '100vh',
    width: '100vw',
    backgroundColor: '#0c0c0c',
    color: '#cccccc',
    fontFamily: '"Cascadia Code", "Courier New", monospace',
    display: 'flex',
    flexDirection: 'column',
    overflow: 'hidden'
  },
  header: {
    backgroundColor: '#1e1e1e',
    borderBottom: '1px solid #333',
    padding: '8px 16px',
    fontSize: '14px',
    fontWeight: '600',
    display: 'flex',
    alignItems: 'center',
    gap: '8px'
  },
  windowControls: {
    marginLeft: 'auto',
    display: 'flex',
    gap: '4px'
  },
  controlButton: {
    width: '12px',
    height: '12px',
    borderRadius: '50%',
    border: 'none',
    cursor: 'pointer'
  },
  closeButton: {
    backgroundColor: '#ff5f57'
  },
  minimizeButton: {
    backgroundColor: '#ffbd2e'
  },
  maximizeButton: {
    backgroundColor: '#28ca42'
  },
  content: {
    flex: 1,
    padding: '16px',
    overflow: 'hidden'
  }
});

const OSInterface = () => {
  const classes = useStyles();

  return (
    <div className={classes.osContainer}>
      <div className={classes.header}>
        <span>CodeCube OS - Terminal</span>
        <div className={classes.windowControls}>
          <button className={`${classes.controlButton} ${classes.closeButton}`} />
          <button className={`${classes.controlButton} ${classes.minimizeButton}`} />
          <button className={`${classes.controlButton} ${classes.maximizeButton}`} />
        </div>
      </div>
      <div className={classes.content}>
        <Terminal />
      </div>
    </div>
  );
};

export default OSInterface;