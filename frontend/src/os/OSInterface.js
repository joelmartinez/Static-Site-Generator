import React from 'react';
import Terminal from './Terminal';

const OSInterface = () => {
  return (
    <div className="os-layout">
      <div className="os-panel">
        <div className="os-panel-header">Terminal</div>
        <Terminal />
      </div>
    </div>
  );
};

export default OSInterface;