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
    filteredData: null
  },
  states: {
    loading: {
      on: {
        DATA_LOADED: {
          target: 'loaded',
          actions: [
            assign({ data: ({ event }) => event.data }),
            'applyFilter'
          ]
        }
      }
    },
    loaded: {
      on: {
        FILTER_CHANGED: {
          actions: [
            assign({ filter: ({ event }) => event.filter }),
            'applyFilter'
          ]
        }
      }
    }
  }
}, {
  actions: {
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
  return actor;
}