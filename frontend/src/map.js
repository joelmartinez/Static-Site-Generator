import * as d3 from 'd3';

/**
 * Create the link map visualization using D3.js
 */
export function createLinkMapVisualization(data) {
  const container = d3.select('#link-map-container');
  const svg = d3.select('#link-map-svg');
  const tooltip = d3.select('#map-tooltip');
  
  // Clear any existing content
  svg.selectAll('*').remove();
  
  // Get container dimensions
  const containerNode = container.node();
  const width = containerNode.clientWidth;
  const height = containerNode.clientHeight;
  
  // Set up SVG
  svg.attr('viewBox', `0 0 ${width} ${height}`);
  
  // Create zoom behavior with middle mouse button support
  const zoom = d3.zoom()
    .scaleExtent([0.1, 4])
    .filter((event) => {
      // Allow zoom/pan with:
      // - Left mouse button (default)
      // - Middle mouse button (wheel click) for panning
      // - Mouse wheel for zooming
      return !event.ctrlKey && !event.button || event.button === 1;
    })
    .on('zoom', (event) => {
      g.attr('transform', event.transform);
    });
  
  svg.call(zoom);
  
  // Create main group for zoom/pan
  const g = svg.append('g');
  
  // Set up simulation with different forces for different node types
  const simulation = d3.forceSimulation(data.nodes)
    .force('link', d3.forceLink(data.edges).id(d => d.id).distance(80)) // Reduced from 100
    .force('charge', d3.forceManyBody().strength(d => {
      // Weaker repulsion for categories, stronger for years, normal for posts
      if (d.nodeType === 'category') return -120; // Weaker pull for categories
      if (d.nodeType === 'year') return -250; // Strong pull for years
      return -200; // Normal for posts
    }))
    .force('center', d3.forceCenter(width / 2, height / 2))
    .force('collision', d3.forceCollide().radius(30)) // Increased from 25
    .force('x', d3.forceX(width / 2).strength(0.1)) // Add X centering force
    .force('y', d3.forceY(height / 2).strength(0.1)); // Add Y centering force
  
  // Create links
  const link = g.append('g')
    .selectAll('line')
    .data(data.edges)
    .enter().append('line')
    .attr('class', 'map-link');
  
  // Create nodes - use different shapes for different types
  const node = g.append('g')
    .selectAll('.map-node')
    .data(data.nodes)
    .enter()
    .append(d => {
      // Use rect for year nodes, polygon for categories, circle for posts
      if (d.nodeType === 'year') {
        return document.createElementNS('http://www.w3.org/2000/svg', 'rect');
      } else if (d.nodeType === 'category') {
        return document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
      } else {
        return document.createElementNS('http://www.w3.org/2000/svg', 'circle');
      }
    })
    .attr('class', d => {
      if (d.nodeType === 'year') return 'map-node map-node-year';
      if (d.nodeType === 'category') return 'map-node map-node-category';
      return 'map-node map-node-post';
    })
    .each(function(d) {
      const element = d3.select(this);
      // Size nodes based on connections and recency
      const baseSize = d.nodeType === 'year' ? 12 : d.nodeType === 'category' ? 10 : 8; // Categories smaller than years but larger than posts
      const connectionBonus = d.connectionCount ? d.connectionCount * (d.nodeType === 'category' ? 1 : 2) : 0; // Reduced bonus for categories
      // Stronger recency scaling: newest posts should be ~2x size of oldest
      const recencyBonus = d.nodeType === 'post' ? (d.recencyFactor || 0) * 16 : 0; // Only apply recency to posts
      const radius = Math.max(baseSize, Math.min(30, baseSize + connectionBonus + recencyBonus));
      
      if (d.nodeType === 'year') {
        // Square for year nodes
        const size = radius * 1.4; // Make squares slightly larger to match visual weight
        element
          .attr('width', size)
          .attr('height', size)
          .attr('x', -size/2)
          .attr('y', -size/2);
      } else if (d.nodeType === 'category') {
        // Diamond/triangle for category nodes
        const size = radius * 1.2;
        // Create a diamond shape using points (store original points on the element)
        const originalPoints = [
          [0, -size], // top
          [size * 0.8, 0], // right
          [0, size], // bottom
          [-size * 0.8, 0] // left
        ];
        const points = originalPoints.map(p => p.join(',')).join(' ');
        element.attr('points', points);
        // Store original points for later translation
        element.datum().originalPoints = originalPoints;
      } else {
        // Circle for post nodes
        element.attr('r', radius);
      }
    })
    .call(d3.drag()
      .on('start', dragstarted)
      .on('drag', dragged)
      .on('end', dragended))
    .on('click', (event, d) => {
      // Navigate to the article or year section (categories are not clickable)
      if (d.nodeType === 'year') {
        // For year nodes, navigate to the home page with the year anchor
        window.location.href = `/${d.id}`;
      } else if (d.nodeType === 'post') {
        // For post nodes, navigate to the article
        window.location.href = d.url;
      }
      // Categories don't have URLs so no navigation
    })
    .on('mouseover', (event, d) => {
      showTooltip(event, d);
    })
    .on('mouseout', hideTooltip)
    .on('mousemove', (event) => {
      moveTooltip(event);
    });
  
  // Create labels
  const label = g.append('g')
    .selectAll('text')
    .data(data.nodes)
    .enter().append('text')
    .attr('class', 'map-node-label')
    .text(d => {
      // Truncate long titles
      return d.title.length > 20 ? d.title.substring(0, 17) + '...' : d.title;
    })
    .attr('dy', 4);
  
  // Update positions on simulation tick
  simulation.on('tick', () => {
    link
      .attr('x1', d => d.source.x)
      .attr('y1', d => d.source.y)
      .attr('x2', d => d.target.x)
      .attr('y2', d => d.target.y);
    
    node
      .each(function(d) {
        const element = d3.select(this);
        if (d.nodeType === 'year') {
          // For rectangles, we need to update x and y (since we set them relative to center)
          element
            .attr('x', d.x - parseFloat(element.attr('width'))/2)
            .attr('y', d.y - parseFloat(element.attr('height'))/2);
        } else if (d.nodeType === 'category') {
          // For polygons, translate the original points to the new position
          if (d.originalPoints) {
            const points = d.originalPoints.map(point => {
              return [point[0] + d.x, point[1] + d.y].join(',');
            }).join(' ');
            element.attr('points', points);
          }
        } else {
          // For circles, just update cx and cy
          element
            .attr('cx', d.x)
            .attr('cy', d.y);
        }
      });
    
    label
      .attr('x', d => d.x)
      .attr('y', d => d.y);
  });

  // Fit the view to show all nodes after simulation settles
  simulation.on('end', () => {
    fitMapToView();
  });

  // Function to fit all nodes in view
  function fitMapToView() {
    const bounds = g.node().getBBox();
    const fullWidth = containerNode.clientWidth;
    const fullHeight = containerNode.clientHeight;
    const width = bounds.width;
    const height = bounds.height;
    const midX = bounds.x + width / 2;
    const midY = bounds.y + height / 2;
    
    if (width === 0 || height === 0) return; // Nothing to fit
    
    const scale = Math.min(fullWidth / width, fullHeight / height) * 0.9; // 90% to add padding
    const translate = [fullWidth / 2 - scale * midX, fullHeight / 2 - scale * midY];
    
    svg.transition()
      .duration(750)
      .call(zoom.transform, d3.zoomIdentity.translate(translate[0], translate[1]).scale(scale));
  }
  
  // Drag functions
  function dragstarted(event, d) {
    if (!event.active) simulation.alphaTarget(0.3).restart();
    d.fx = d.x;
    d.fy = d.y;
  }
  
  function dragged(event, d) {
    d.fx = event.x;
    d.fy = event.y;
  }
  
  function dragended(event, d) {
    if (!event.active) simulation.alphaTarget(0);
    d.fx = null;
    d.fy = null;
  }
  
  // Tooltip functions
  function showTooltip(event, d) {
    let tooltipContent;
    
    if (d.nodeType === 'year') {
      tooltipContent = `
        <strong>${d.title}</strong><br>
        ${d.description}<br>
        Click to view archive
      `;
    } else if (d.nodeType === 'category') {
      tooltipContent = `
        <strong>Category: ${d.title}</strong><br>
        ${d.description}<br>
        Groups related posts
      `;
    } else {
      const formatDate = d3.timeFormat('%B %d, %Y');
      tooltipContent = `
        <strong>${d.title}</strong><br>
        Published: ${formatDate(new Date(d.publishedOn))}<br>
        Connections: ${d.connectionCount || 0}<br>
        ${d.description}
      `;
    }
    
    tooltip
      .style('opacity', 1)
      .html(tooltipContent);
    
    moveTooltip(event);
  }
  
  function hideTooltip() {
    tooltip.style('opacity', 0);
  }
  
  function moveTooltip(event) {
    const containerRect = containerNode.getBoundingClientRect();
    const x = event.clientX - containerRect.left;
    const y = event.clientY - containerRect.top;
    
    tooltip
      .style('left', (x + 10) + 'px')
      .style('top', (y - 10) + 'px');
  }
  
  // Control button handlers
  d3.select('#zoom-in').on('click', () => {
    svg.transition().call(zoom.scaleBy, 1.5);
  });
  
  d3.select('#zoom-out').on('click', () => {
    svg.transition().call(zoom.scaleBy, 1 / 1.5);
  });
  
  d3.select('#reset-view').on('click', () => {
    fitMapToView();
  });
  
  // Handle window resize
  function handleResize() {
    const newWidth = containerNode.clientWidth;
    const newHeight = containerNode.clientHeight;
    svg.attr('viewBox', `0 0 ${newWidth} ${newHeight}`);
    simulation.force('center', d3.forceCenter(newWidth / 2, newHeight / 2));
    simulation.alpha(0.3).restart();
  }
  
  window.addEventListener('resize', handleResize);
  
  // Store cleanup function
  container.node().__mapCleanup = () => {
    window.removeEventListener('resize', handleResize);
    simulation.stop();
  };
}