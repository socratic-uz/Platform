// Reusable Konva.js Seating Layout Designer JS Module

const stages = new Map(); // Store stage instances by containerId

export async function loadKonva() {
    if (window.Konva) return Promise.resolve();
    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = 'https://unpkg.com/konva@9.3.15/konva.min.js';
        script.onload = () => resolve();
        script.onerror = (err) => reject(err);
        document.head.appendChild(script);
    });
}

export async function initDesigner(containerId, dotNetHelper, isReadOnly, layoutJson) {
    await loadKonva();

    const container = document.getElementById(containerId);
    if (!container) {
        console.error(`Container #${containerId} not found`);
        return;
    }

    const width = container.offsetWidth || 800;
    const height = container.offsetHeight || 500;

    let stage;
    let layer;

    if (layoutJson) {
        try {
            stage = window.Konva.Node.create(layoutJson, containerId);
            stage.width(width);
            stage.height(height);
            layer = stage.findOne('Layer');
            if (!layer) {
                layer = new window.Konva.Layer();
                stage.add(layer);
            }
        } catch (e) {
            console.error("Failed to parse seat layout JSON, initializing empty:", e);
        }
    }

    if (!stage) {
        stage = new window.Konva.Stage({
            container: containerId,
            width: width,
            height: height
        });
        layer = new window.Konva.Layer();
        stage.add(layer);
    }

    const state = {
        stage,
        layer,
        dotNetHelper,
        isReadOnly,
        selectedNode: null
    };

    stages.set(containerId, state);

    // Make shapes non-draggable if read-only, otherwise draggable
    layer.getChildren().forEach(node => {
        node.draggable(!isReadOnly);
        // Setup events
        setupNodeEvents(containerId, node);
    });

    stage.on('click tap', (e) => {
        console.log("Konva Click/Tap Event:", e.target);
        // If clicked on stage empty space, deselect
        if (e.target === stage) {
            console.log("Clicked empty stage area, deselecting all.");
            deselectAll(containerId);
            return;
        }

        const node = e.target.getParent();
        console.log("Clicked node parent:", node, "Name:", node ? node.name() : null, "Attrs:", node ? node.attrs : null);
        if (!node || node.className === 'Layer' || node.className === 'Stage' || node.name() === 'Layer') return;

        if (isReadOnly) {
            const attrs = node.attrs;
            const isBookable = attrs.isSeat || (attrs.id && (attrs.id.startsWith('seat_') || attrs.id.startsWith('table_')));
            console.log("Checking if node is an available seat/table:", isBookable, "isReserved:", attrs.isReserved);
            if (isBookable && !attrs.isReserved) {
                console.log("Selecting seat/table via Blazor helper:", attrs.seatId || node.id(), "product:", attrs.productId);
                dotNetHelper.invokeMethodAsync('SelectSeat', attrs.seatId || node.id(), attrs.productId || '');
                highlightSeat(containerId, node);
            }
        } else {
            selectNode(containerId, node);
        }
    });

    layer.draw();
}

function setupNodeEvents(containerId, node) {
    const state = stages.get(containerId);
    if (!state) return;

    if (!state.isReadOnly) {
        node.on('dragend', () => {
            // Snap to grid
            const grid = 10;
            node.x(Math.round(node.x() / grid) * grid);
            node.y(Math.round(node.y() / grid) * grid);
            state.layer.draw();
        });
    }
}

function deselectAll(containerId) {
    const state = stages.get(containerId);
    if (!state) return;

    if (state.selectedNode) {
        const border = state.selectedNode.findOne('.border');
        if (border) {
            const isSeat = state.selectedNode.attrs.isSeat || (state.selectedNode.attrs.id && state.selectedNode.attrs.id.startsWith('seat_'));
            border.stroke(isSeat ? '#2832d4' : '#a0a0a0');
            border.strokeWidth(2);
        }
        state.selectedNode = null;
    }
    state.layer.draw();
    state.dotNetHelper.invokeMethodAsync('OnDeselectedCallback');
}

function selectNode(containerId, node) {
    deselectAll(containerId);

    const state = stages.get(containerId);
    if (!state) return;

    state.selectedNode = node;
    const border = node.findOne('.border');
    if (border) {
        border.stroke('#ff007f'); // Highlight pink
        border.strokeWidth(3);
    }
    state.layer.draw();

    state.dotNetHelper.invokeMethodAsync('OnNodeSelectedCallback', {
        id: node.id(),
        isSeat: !!(node.attrs.isSeat || (node.attrs.id && node.attrs.id.startsWith('seat_'))),
        seatId: node.attrs.seatId || '',
        productId: node.attrs.productId || '',
        label: node.attrs.label || '',
        price: node.attrs.price || 0
    });
}

function highlightSeat(containerId, node) {
    const state = stages.get(containerId);
    if (!state) return;

    // Reset previous selection highlight
    state.layer.getChildren().forEach(n => {
        const isBookable = n.attrs.isSeat || (n.attrs.id && (n.attrs.id.startsWith('seat_') || n.attrs.id.startsWith('table_')));
        if (isBookable && !n.attrs.isReserved) {
            const b = n.findOne('.border');
            if (b) {
                const isSeat = n.attrs.isSeat || n.attrs.id.startsWith('seat_');
                b.fill('rgba(40, 50, 212, 0.1)');
                b.stroke(isSeat ? '#2832d4' : '#a0a0a0');
            }
        }
    });

    // Highlight current
    const border = node.findOne('.border');
    if (border) {
        border.fill('rgba(0, 255, 127, 0.25)'); // Light green highlight
        border.stroke('#00ff7f');
    }
    state.layer.draw();
}

export function addTable(containerId, type) {
    const state = stages.get(containerId);
    if (!state || state.isReadOnly) return;

    const group = new window.Konva.Group({
        x: 100,
        y: 100,
        draggable: true,
        id: 'table_' + Date.now()
    });

    let shape;
    if (type === 'circle') {
        shape = new window.Konva.Circle({
            radius: 50,
            fill: '#202028',
            stroke: '#a0a0a0',
            strokeWidth: 2,
            name: 'border'
        });
    } else {
        shape = new window.Konva.Rect({
            width: 120,
            height: 80,
            fill: '#202028',
            stroke: '#a0a0a0',
            strokeWidth: 2,
            cornerRadius: 8,
            name: 'border',
            offsetX: 60,
            offsetY: 40
        });
    }

    const text = new window.Konva.Text({
        text: 'Стол',
        fontSize: 14,
        fontFamily: 'Montserrat, sans-serif',
        fill: '#ffffff',
        align: 'center',
        verticalAlign: 'middle',
        offsetX: type === 'circle' ? 20 : 25,
        offsetY: 7
    });

    group.add(shape);
    group.add(text);
    state.layer.add(group);
    setupNodeEvents(containerId, group);
    state.layer.draw();
}

export function addSeat(containerId, label, productId, price) {
    const state = stages.get(containerId);
    if (!state || state.isReadOnly) return;

    const group = new window.Konva.Group({
        x: 150,
        y: 150,
        draggable: true,
        id: 'seat_' + Date.now(),
        isSeat: true,
        productId: productId,
        seatId: 'seat_' + Date.now(),
        label: label,
        price: price
    });

    const shape = new window.Konva.Circle({
        radius: 20,
        fill: 'rgba(40, 50, 212, 0.1)',
        stroke: '#2832d4',
        strokeWidth: 2,
        name: 'border'
    });

    const text = new window.Konva.Text({
        text: label || 'M1',
        fontSize: 12,
        fontFamily: 'Montserrat, sans-serif',
        fill: '#ffffff',
        offsetX: 8,
        offsetY: 6
    });

    group.add(shape);
    group.add(text);
    state.layer.add(group);
    setupNodeEvents(containerId, group);
    state.layer.draw();
}

export function updateSelectedProperties(containerId, label, productId, price) {
    const state = stages.get(containerId);
    if (!state || !state.selectedNode) return;

    const node = state.selectedNode;
    node.attrs.label = label;
    node.attrs.productId = productId;
    node.attrs.price = price;

    const text = node.findOne('Text');
    if (text) {
        text.text(label);
        // Recenter text
        text.offsetX(label.length * 4);
    }
    state.layer.draw();
}

export function removeSelected(containerId) {
    const state = stages.get(containerId);
    if (!state || !state.selectedNode) return;

    state.selectedNode.destroy();
    state.selectedNode = null;
    state.layer.draw();
    state.dotNetHelper.invokeMethodAsync('OnDeselected');
}

export function getLayoutJson(containerId) {
    const state = stages.get(containerId);
    if (!state) return '{}';
    return state.stage.toJSON();
}

export function updateSeatStatuses(containerId, reservedProductIdsJson) {
    const state = stages.get(containerId);
    if (!state) return;

    let reservedIds = [];
    try {
        reservedIds = JSON.parse(reservedProductIdsJson) || [];
    } catch(e) {
        console.error("Failed to parse reserved product IDs:", e);
    }

    state.layer.getChildren().forEach(node => {
        const isBookable = node.attrs.isSeat || (node.attrs.id && (node.attrs.id.startsWith('seat_') || node.attrs.id.startsWith('table_')));
        if (isBookable) {
            const productId = node.attrs.productId;
            const isReserved = productId ? reservedIds.includes(productId) : false;
            node.attrs.isReserved = isReserved;

            const border = node.findOne('.border');
            if (border) {
                if (isReserved) {
                    border.fill('rgba(255, 0, 0, 0.15)'); // Red background
                    border.stroke('#ff0000'); // Red border
                } else {
                    const isSeat = node.attrs.isSeat || node.attrs.id.startsWith('seat_');
                    border.fill('rgba(40, 50, 212, 0.1)'); // Reset to default blue/gray
                    border.stroke(isSeat ? '#2832d4' : '#a0a0a0');
                }
            }
        }
    });
    state.layer.draw();
}

export function dispose(containerId) {
    const state = stages.get(containerId);
    if (state) {
        state.stage.destroy();
        stages.delete(containerId);
    }
}
