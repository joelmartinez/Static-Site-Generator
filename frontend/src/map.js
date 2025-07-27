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
  
  // Create zoom behavior
  const zoom = d3.zoom()
    .scaleExtent([0.1, 4])
    .on('zoom', (event) => {
      g.attr('transform', event.transform);
    });
  
  svg.call(zoom);
  
  // Create main group for zoom/pan
  const g = svg.append('g');
  
  // Set up simulation
  const simulation = d3.forceSimulation(data.nodes)
    .force('link', d3.forceLink(data.edges).id(d => d.id).distance(100))
    .force('charge', d3.forceManyBody().strength(-300))
    .force('center', d3.forceCenter(width / 2, height / 2))
    .force('collision', d3.forceCollide().radius(25));
  
  // Create links
  const link = g.append('g')
    .selectAll('line')
    .data(data.edges)
    .enter().append('line')
    .attr('class', 'map-link');
  
  // Create nodes
  const node = g.append('g')
    .selectAll('circle')
    .data(data.nodes)
    .enter().append('circle')
    .attr('class', 'map-node')
    .attr('r', d => {
      // Size nodes based on how many connections they have
      const connections = data.edges.filter(e => e.source === d.id || e.target === d.id).length;
      return Math.max(8, Math.min(20, 8 + connections * 2));
    })
    .call(d3.drag()
      .on('start', dragstarted)
      .on('drag', dragged)
      .on('end', dragended))
    .on('click', (event, d) => {
      // Navigate to the article
      window.location.href = d.url;
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
      .attr('cx', d => d.x)
      .attr('cy', d => d.y);
    
    label
      .attr('x', d => d.x)
      .attr('y', d => d.y);
  });
  
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
    const formatDate = d3.timeFormat('%B %d, %Y');
    const tooltipContent = `
      <strong>${d.title}</strong><br>
      Published: ${formatDate(new Date(d.publishedOn))}<br>
      ${d.description}
    `;
    
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
    svg.transition().call(zoom.transform, d3.zoomIdentity);
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