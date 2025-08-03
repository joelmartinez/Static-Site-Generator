import { createMachine, createActor, assign } from 'xstate';

/**
 * XState machine for managing link map state
 */
export const linkMapMachine = createMachine({
  id: 'linkMap',
  initial: 'loading',
  context: {
    filter: 'entities', // Default to entity connections
    data: null,
    filteredData: null,
    selectedNode: null, // Currently selected node for focus behavior
    urlParams: null // URL parameters for state sharing
  },
  states: {
    loading: {
      on: {
        DATA_LOADED: {
          target: 'loaded',
          actions: [
            assign({ data: ({ event }) => event.data }),
            'loadUrlState',
            'applyFilter'
          ]
        }
      }
    },
    loaded: {
      initial: 'normal',
      states: {
        normal: {
          on: {
            NODE_SELECTED: {
              target: 'nodeSelected',
              actions: [
                assign({ selectedNode: ({ event }) => event.node }),
                'updateUrl',
                'applyFilter'
              ]
            }
          }
        },
        nodeSelected: {
          on: {
            NODE_SELECTED: {
              target: 'nodeSelected',
              actions: [
                assign({ selectedNode: ({ event }) => event.node }),
                'updateUrl',
                'applyFilter'
              ]
            },
            NODE_DESELECTED: {
              target: 'normal',
              actions: [
                assign({ selectedNode: null }),
                'updateUrl',
                'applyFilter'
              ]
            }
          }
        }
      },
      on: {
        FILTER_CHANGED: {
          actions: [
            assign({ filter: ({ event }) => event.filter }),
            'updateUrl',
            'applyFilter'
          ]
        },
        URL_STATE_CHANGED: {
          actions: [
            assign({ 
              filter: ({ event }) => event.filter || 'entities',
              selectedNode: ({ event }) => event.selectedNode || null
            }),
            'applyFilter'
          ]
        }
      }
    }
  }
}, {
  actions: {
    loadUrlState: assign(({ context }) => {
      const urlParams = new URLSearchParams(window.location.search);
      const filter = urlParams.get('filter') || 'entities';
      const selectedNodeId = urlParams.get('node');
      
      return {
        filter,
        selectedNode: selectedNodeId ? { id: selectedNodeId } : null,
        urlParams: urlParams
      };
    }),

    updateUrl: ({ context }) => {
      const params = new URLSearchParams();
      if (context.filter && context.filter !== 'entities') {
        params.set('filter', context.filter);
      }
      if (context.selectedNode) {
        params.set('node', context.selectedNode.id);
      }
      
      const newUrl = window.location.pathname + 
        (params.toString() ? '?' + params.toString() : '');
      window.history.replaceState({}, '', newUrl);
    },

    applyFilter: assign(({ context }) => {
      if (!context.data) {
        return { filteredData: null };
      }

      const { nodes, edges } = context.data;
      let filteredNodes = [];
      let filteredEdges = [];

      switch (context.filter) {
        case 'all':
          filteredNodes = nodes;
          filteredEdges = edges;
          break;
        
        case 'posts':
          // Show only posts and their direct connections
          filteredNodes = nodes.filter(node => node.nodeType === 'post');
          filteredEdges = edges.filter(edge => {
            const sourceNode = nodes.find(n => n.id === edge.source || n.id === edge.source.id);
            const targetNode = nodes.find(n => n.id === edge.target || n.id === edge.target.id);
            return sourceNode?.nodeType === 'post' && targetNode?.nodeType === 'post';
          });
          break;
        
        case 'years':
          // Show posts, years, and year-to-post connections
          filteredNodes = nodes.filter(node => 
            node.nodeType === 'post' || node.nodeType === 'year'
          );
          filteredEdges = edges.filter(edge => {
            const sourceNode = nodes.find(n => n.id === edge.source || n.id === edge.source.id);
            const targetNode = nodes.find(n => n.id === edge.target || n.id === edge.target.id);
            return (sourceNode?.nodeType === 'year' && targetNode?.nodeType === 'post') ||
                   (sourceNode?.nodeType === 'post' && targetNode?.nodeType === 'year');
          });
          break;
        
        case 'categories':
          // Show posts, categories, and category-to-post connections
          filteredNodes = nodes.filter(node => 
            node.nodeType === 'post' || node.nodeType === 'category'
          );
          filteredEdges = edges.filter(edge => {
            const sourceNode = nodes.find(n => n.id === edge.source || n.id === edge.source.id);
            const targetNode = nodes.find(n => n.id === edge.target || n.id === edge.target.id);
            return (sourceNode?.nodeType === 'category' && targetNode?.nodeType === 'post') ||
                   (sourceNode?.nodeType === 'post' && targetNode?.nodeType === 'category');
          });
          break;
        
        case 'entities':
          // Show posts, entities, and entity connections (default)
          filteredNodes = nodes.filter(node => 
            node.nodeType === 'post' || node.nodeType === 'entity'
          );
          filteredEdges = edges.filter(edge => {
            const sourceNode = nodes.find(n => n.id === edge.source || n.id === edge.source.id);
            const targetNode = nodes.find(n => n.id === edge.target || n.id === edge.target.id);
            return (sourceNode?.nodeType === 'entity' || targetNode?.nodeType === 'entity') &&
                   (sourceNode?.nodeType === 'post' || sourceNode?.nodeType === 'entity') &&
                   (targetNode?.nodeType === 'post' || targetNode?.nodeType === 'entity');
          });
          break;
        
        default:
          filteredNodes = nodes;
          filteredEdges = edges;
      }

      // Mark nodes as selected/connected/unconnected for styling
      if (context.selectedNode) {
        const selectedNodeId = context.selectedNode.id;
        const connectedNodeIds = new Set();
        
        // Find all nodes connected to the selected node
        filteredEdges.forEach(edge => {
          const sourceId = edge.source.id || edge.source;
          const targetId = edge.target.id || edge.target;
          
          if (sourceId === selectedNodeId) {
            connectedNodeIds.add(targetId);
          } else if (targetId === selectedNodeId) {
            connectedNodeIds.add(sourceId);
          }
        });

        // Add selection state to nodes
        filteredNodes = filteredNodes.map(node => ({
          ...node,
          selectionState: node.id === selectedNodeId ? 'selected' : 
                         connectedNodeIds.has(node.id) ? 'connected' : 'unconnected'
        }));
      } else {
        // Clear any selection state
        filteredNodes = filteredNodes.map(node => ({
          ...node,
          selectionState: 'normal'
        }));
      }

      return {
        filteredData: {
          nodes: filteredNodes,
          edges: filteredEdges
        }
      };
    })
  }
});

/**
 * Create and start the map state actor
 */
export function createMapStateActor() {
  const actor = createActor(linkMapMachine);
  actor.start();
  
  // Handle browser back/forward navigation
  window.addEventListener('popstate', () => {
    const urlParams = new URLSearchParams(window.location.search);
    const filter = urlParams.get('filter') || 'entities';
    const selectedNodeId = urlParams.get('node');
    
    actor.send({ 
      type: 'URL_STATE_CHANGED', 
      filter, 
      selectedNode: selectedNodeId ? { id: selectedNodeId } : null 
    });
  });
  
  return actor;
}

/**
 * Get current state values from actor
 */
export function getStateValues(actor) {
  const state = actor.getSnapshot();
  return {
    filter: state.context.filter,
    selectedNode: state.context.selectedNode,
    filteredData: state.context.filteredData,
    isNodeSelected: state.matches({ loaded: 'nodeSelected' })
  };
}