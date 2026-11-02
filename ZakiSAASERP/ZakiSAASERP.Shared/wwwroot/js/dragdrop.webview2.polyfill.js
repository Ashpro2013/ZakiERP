/**
 * Polyfill for WebView2 Drag and Drop Bug (MicrosoftEdge/WebView2Feedback/issues/2805)
 * Enables HTML5 Drag and Drop events in WebView2 when hosted in WinUI 3 / MAUI.
 */
(function () {
    if (typeof window === 'undefined' || typeof document === 'undefined') return;

    let isDragging = false;
    let potentialDraggable = null;
    let draggedElement = null;
    let startPosition = { x: 0, y: 0 };
    let currentOverElement = null;
    let ghostElement = null;
    const DRAG_THRESHOLD = 5;

    function findDraggableAncestor(element) {
        while (element && element !== document.body && element !== document.documentElement) {
            if (element.getAttribute && element.getAttribute('draggable') === 'true') {
                return element;
            }
            element = element.parentElement;
        }
        return null;
    }

    function MockDataTransfer() {
        this.dropEffect = 'move';
        this.effectAllowed = 'all';
        this.files = [];
        this.items = [];
        this.types = [];
        this.setData = function (format, data) {
            this.items.push({ format, data });
            if (!this.types.includes(format)) this.types.push(format);
        };
        this.getData = function (format) {
            let item = this.items.find(i => i.format === format);
            return item ? item.data : '';
        };
        this.clearData = function (format) {
            if (format) {
                this.items = this.items.filter(i => i.format !== format);
                this.types = this.types.filter(t => t !== format);
            } else {
                this.items = [];
                this.types = [];
            }
        };
    }

    function createMockDragEvent(type, options) {
        let event;
        try {
            event = new DragEvent(type, options);
        } catch (e) {
            event = new Event(type, options);
        }
        if (!event.dataTransfer) {
            event.dataTransfer = new MockDataTransfer();
        }
        return event;
    }

    function cleanup() {
        if (ghostElement && ghostElement.parentNode) {
            ghostElement.parentNode.removeChild(ghostElement);
        }
        if (draggedElement) {
            draggedElement.setAttribute('draggable', 'true');
        }
        isDragging = false;
        potentialDraggable = null;
        draggedElement = null;
        ghostElement = null;
        currentOverElement = null;
    }

    function applyStylesToGhost(original, ghost) {
        let computedStyles = window.getComputedStyle(original);
        for (let prop of computedStyles) {
            ghost.style[prop] = computedStyles[prop];
        }
        let rect = original.getBoundingClientRect();
        ghost.style.position = 'fixed';
        ghost.style.left = `${rect.left}px`;
        ghost.style.top = `${rect.top}px`;
        ghost.style.zIndex = '999999';
        ghost.style.opacity = '0.75';
        ghost.style.pointerEvents = 'none';
        ghost.style.boxShadow = '0 8px 20px rgba(0,0,0,0.35)';
        document.body.appendChild(ghost);
    }

    document.addEventListener('mousedown', function (e) {
        if (e.button !== 0) return;

        let draggable = findDraggableAncestor(e.target);
        if (!draggable) return;

        potentialDraggable = draggable;
        startPosition = { x: e.clientX, y: e.clientY };
    }, true);

    document.addEventListener('mousemove', function (e) {
        if (!potentialDraggable && !isDragging) return;

        if (!isDragging && potentialDraggable) {
            let distance = Math.hypot(e.clientX - startPosition.x, e.clientY - startPosition.y);
            if (distance >= DRAG_THRESHOLD) {
                isDragging = true;
                draggedElement = potentialDraggable;
                potentialDraggable = null;

                draggedElement.setAttribute('draggable', 'false');
                ghostElement = draggedElement.cloneNode(true);
                applyStylesToGhost(draggedElement, ghostElement);

                draggedElement.dispatchEvent(createMockDragEvent('dragstart', {
                    bubbles: true,
                    cancelable: true,
                    clientX: e.clientX,
                    clientY: e.clientY
                }));
            } else {
                return;
            }
        }

        if (isDragging && ghostElement) {
            let x = e.clientX - startPosition.x;
            let y = e.clientY - startPosition.y;
            ghostElement.style.transform = `translate(${x}px, ${y}px)`;

            ghostElement.style.display = 'none';
            let elementBelow = document.elementFromPoint(e.clientX, e.clientY);
            ghostElement.style.display = '';

            if (elementBelow && elementBelow !== currentOverElement) {
                elementBelow.dispatchEvent(createMockDragEvent('dragover', {
                    bubbles: true,
                    cancelable: true,
                    clientX: e.clientX,
                    clientY: e.clientY
                }));
                currentOverElement = elementBelow;
            }
        }
    }, true);

    document.addEventListener('mouseup', function (e) {
        if (!isDragging) {
            potentialDraggable = null;
            return;
        }

        if (currentOverElement) {
            currentOverElement.dispatchEvent(createMockDragEvent('drop', {
                bubbles: true,
                cancelable: true,
                clientX: e.clientX,
                clientY: e.clientY
            }));
        }

        if (draggedElement) {
            draggedElement.dispatchEvent(createMockDragEvent('dragend', {
                bubbles: true,
                cancelable: true,
                clientX: e.clientX,
                clientY: e.clientY
            }));
        }

        cleanup();
    }, true);

    window.addEventListener('blur', cleanup);
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && isDragging) cleanup();
    });
})();

