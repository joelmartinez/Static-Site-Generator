import React from 'react';
import { createRoot } from 'react-dom/client';
import { FluentProvider, webLightTheme } from '@fluentui/react-components';
import OSInterface from './OSInterface';

// Initialize the OS when the DOM is ready
const initOS = () => {
  const container = document.getElementById('os-root');
  if (container) {
    const root = createRoot(container);
    root.render(
      <FluentProvider theme={webLightTheme}>
        <OSInterface />
      </FluentProvider>
    );
  }
};

// Initialize when DOM is ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initOS);
} else {
  initOS();
}