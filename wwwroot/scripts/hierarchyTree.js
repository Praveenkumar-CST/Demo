window.renderHierarchyTree = (data, userRole, dotNetHelper) => {
    // Clear existing SVG
    d3.select("#treeContainer").selectAll("*").remove();

    // Set dimensions and margins
    const margin = { top: 40, right: 120, bottom: 40, left: 120 };
    const width = 1200 - margin.left - margin.right;
    const height = 800 - margin.top - margin.bottom;

    // Create SVG
    const svg = d3.select("#treeContainer").append("svg")
        .attr("width", width + margin.left + margin.right)
        .attr("height", height + margin.top + margin.bottom);

    // Create the group element
    const g = svg.append("g")
        .attr("transform", `translate(${margin.left},${margin.top})`);

    // Attach zoom behavior to the SVG
    svg.call(d3.zoom().on("zoom", function (event) {
        g.attr("transform", event.transform);
    }));

    // Create tree layout
    const treemap = d3.tree().size([width, height]);

    // Convert flat data to hierarchy
    const root = d3.hierarchy(data, d => d.Children || []);

    // Compute layout
    const treeData = treemap(root);

    // Add links
    const link = g.selectAll(".link") // Note: Append links to g, not svg
        .data(treeData.links())
        .enter()
        .append("path")
        .attr("class", "link")
        .attr("fill", "none")
        .attr("stroke", d => {
            const level = d.source.depth;
            return level === 0 ? "#1976d2" : level === 1 ? "#388e3c" : level === 2 ? "#f57c00" : "#7b1fa2";
        })
        .attr("stroke-width", 2)
        .attr("d", d3.linkVertical()
            .x(d => d.x)
            .y(d => d.y));

    // Add nodes
    const node = g.selectAll(".node") // Note: Append nodes to g, not svg
        .data(treeData.descendants())
        .enter()
        .append("g")
        .attr("class", "node")
        .attr("transform", d => `translate(${d.x},${d.y})`);

    // Add MudBlazor-style cards (as foreignObject for HTML content)
    node.append("foreignObject")
        .attr("width", 200)
        .attr("height", 150)
        .attr("x", -100)
        .attr("y", -75)
        .append("xhtml:div")
        .attr("class", "mud-card")
        .style("width", "190px")
        .style("background", "white")
        .style("border-radius", "6px")
        .style("box-shadow", "0 1px 6px rgba(0, 0, 0, 0.1)")
        .style("border", "1px solid #e0e0e0")
        .style("padding-top", "8px")
        .html(d => {
            // Ensure Children is an array
            const children = d.data.Children || [];
            return `
                <div class="mud-card-header" style="padding: 8px 32px 8px 12px; display: flex; align-items: center; position: relative;">
                    <div class="mud-avatar" style="width: 32px; height: 32px; background: #1976d2; margin-right: 8px;">
                        <svg class="mud-icon" style="width: 20px; height: 20px; fill: white;" viewBox="0 0 24 24">
                            <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                        </svg>
                    </div>
                    <div class="mud-card-header-content" style="flex-grow: 1;">
                        <div class="mud-typography mud-typography-subtitle2 employee-name" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 140px; font-weight: 500;">
                            ${d.data.Name || 'Unknown'}
                        </div>
                    </div>
                    ${(userRole === "Admin" || userRole === "Hr") && d.data.ParentMentorId && children.length === 0 ? `
                        <div class="card-circle-btn" data-employee-id="${d.data.EmployeeID}" data-action="unassign" style="position: absolute; top: -15px; right: -25px; width: 25px; height: 25px; background-color: #ff5252; border-radius: 50%; z-index: 10; cursor: pointer; display: flex; justify-content: center; align-items: center; border: 2px solid white;">
                            <span class="x-mark" style="color: white; font-weight: bold; font-size: 12px;">X</span>
                        </div>
                    ` : ""}
                </div>
                <div class="mud-card-content" style="padding: 0 12px 8px;">
                    <div class="mud-typography mud-typography-caption" style="display: flex; align-items: center;">
                        <svg class="mud-icon" style="width: 16px; height: 16px; fill: #757575; margin-right: 4px;" viewBox="0 0 24 24">
                            <path d="M22 6c0-1.1-.9-2-2-2H4c-1.1 0-2 .9-2 2v12c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V6zm-2 0l-8 5-8-5h16zm0 12H4V8l8 5 8-5v10z"/>
                        </svg>
                        ID: ${d.data.EmployeeID || 'N/A'}
                    </div>
                </div>
                <div class="mud-card-actions" style="display: flex; justify-content: space-between; padding: 0 12px 8px; flex-direction: column;">
                    <div class="action-row" style="display: flex; justify-content: space-between; width: 100%; align-items: center;">
                        ${(userRole === "Admin" || userRole === "Hr") ? `
                            <button class="mud-button mud-button-text mud-button-primary action-btn" data-employee-id="${d.data.EmployeeID}" data-action="add-mentee" style="padding: 0; margin: 0; display: flex; align-items: center; font-size: 12px;">
                                <svg class="mud-icon" style="width: 16px; height: 16px; fill: #1976d2; margin-right: 4px;" viewBox="0 0 24 24">
                                    <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                                </svg>
                                ${children.length > 0 ? `<span class="mud-badge" style="background: #1976d2; color: white; border-radius: 50%; width: 16px; height: 16px; display: flex; align-items: center; justify-content: center; margin-left: 4px; font-size: 10px;">${children.length}</span>` : ""}
                            </button>
                            <button class="mud-button mud-button-icon mud-button-error action-btn" data-employee-id="${d.data.EmployeeID}" data-action="toggle-xmarks" style="padding: 0; margin: 0; width: 24px; height: 24px;" ${children.length === 0 ? "disabled" : ""}>
                                <svg class="mud-icon" style="width: 16px; height: 16px; fill: #d32f2f;" viewBox="0 0 24 24">
                                    <path d="M12 12c2.21 0 4-1.79 4-4s-1.79-4-4-4-4 1.79-4 4 1.79 4 4 4zm0 2c-2.67 0-8 1.34-8 4v2h16v-2c0-2.66-5.33-4-8-4z"/>
                                </svg>
                            </button>
                        ` : `
                            <button class="mud-button mud-button-text mud-button-primary action-btn" style="padding: 0; margin: 0; display: flex; align-items: center; font-size: 12px;">
                                <svg class="mud-icon" style="width: 16px; height: 16px; fill: #1976d2; margin-right: 4px;" viewBox="0 0 24 24">
                                    <path d="M16 9v10H8V9h8m-1.5-6h-5l-1 1H5v2h14V4h-3.5l-1-1zM18 7H6v12c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7z"/>
                                </svg>
                                ${children.length > 0 ? `<span class="mud-badge" style="background: #1976d2; color: white; border-radius: 50%; width: 16px; height: 16px; display: flex; align-items: center; justify-content: center; margin-left: 4px; font-size: 10px;">${children.length}</span>` : ""}
                            </button>
                        `}
                    </div>
                    ${children.length > 0 ? `
                        <button class="mud-button mud-button-icon centered-expand" data-employee-id="${d.data.EmployeeID}" data-action="toggle-expand" style="margin-top: 8px; width: 24px; height: 24px; align-self: center;">
                            <svg class="mud-icon" style="width: 16px; height: 16px; fill: #616161;" viewBox="0 0 24 24">
                                <path d="${d.data.IsExpanded ? 'M12 8l-6 6 1.41 1.41L12 10.83l4.59 4.58L18 14l-6-6z' : 'M12 16l6-6-1.41-1.41L12 13.17l-4.59-4.58L6 10l6 6z'}"/>
                            </svg>
                        </button>
                    ` : ""}
                </div>
                <div class="mud-chip level-chip" style="position: absolute; top: -34px; left: 50%; transform: translateX(-50%); font-size: 0.75rem; padding: 2px 8px; height: 20px; background-color: ${d.depth === 0 ? '#1976d2' : d.depth === 1 ? '#388e3c' : d.depth === 2 ? '#f57c00' : '#7b1fa2'}; color: white;">
                    L${d.depth + 1}
                </div>
            `;
        });

    // Add event listeners for actions
    node.selectAll(".action-btn, .card-circle-btn, .centered-expand")
        .on("click", function (event, d) {
            const action = d3.select(this).attr("data-action");
            const employeeId = d3.select(this).attr("data-employee-id");
            if (action && employeeId) {
                dotNetHelper.invokeMethodAsync("HandleNodeAction", action, employeeId);
            }
        });

    // Add level chip styles
    g.append("style")
        .text(`
            .link { fill: none; }
            .mud-card:hover {
                transform: translateY(-2px);
                box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15) !important;
                border-color: #1976d2 !important;
            }
            .card-circle-btn:hover {
                background-color: #ff1744 !important;
            }
        `);
};