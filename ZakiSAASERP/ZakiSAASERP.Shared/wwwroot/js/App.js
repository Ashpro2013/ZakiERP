//local storage
function exportJsonToExcel(jsonData) {
    var data = JSON.parse(jsonData);

    // find all total-like rows dynamically
    data = data.map(row => {

        // check if any value in the row contains "total"
        const isTotalRow = Object.values(row).some(v =>
            typeof v === "string" && v.toLowerCase().includes("total")
        );

        if (!isTotalRow) return row;   // not total row → keep as is

        // modify total row dynamically
        const newRow = {};

        Object.keys(row).forEach(key => {
            const value = row[key];

            // keep TOTAL text as is
            if (typeof value === "string" && value.toLowerCase().includes("total")) {
                newRow[key] = value;
            }
            // if numeric 0 → empty string
            else if (typeof value === "number" && value === 0) {
                newRow[key] = "";
            }
            // otherwise keep value
            else {
                newRow[key] = value;
            }
        });

        return newRow;
    });

    // convert to worksheet
    var ws = XLSX.utils.json_to_sheet(data);
    var wb = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, "Sheet1");

    XLSX.writeFile(wb, "data.xlsx");
}

function setLocalData(val, key) {
    localStorage.setItem(key, val);
}
function getLocalData(key) {
    return localStorage.getItem(key);
}
function removeLocalData(key) {
    return localStorage.removeItem(key);
}
//local storage - end
function closeBrowserTab() {
    window.open('about:blank', '_self').close();
}
function openFullscreen() {
    try {
        var elem = document.documentElement;
        if (elem.requestFullscreen) {
            elem.requestFullscreen();
        } else if (elem.webkitRequestFullscreen) {
            elem.webkitRequestFullscreen();
        } else if (elem.msRequestFullscreen) {
            elem.msRequestFullscreen();
        }
        return true;
    } catch (e) {
        return false;
    }
}


function closeFullscreen() {
    if (document.exitFullscreen) {
        document.exitFullscreen();
    } else if (document.mozCancelFullScreen) { /* Firefox */
        document.mozCancelFullScreen();
    } else if (document.webkitExitFullscreen) { /* Chrome, Safari and Opera */
        document.webkitExitFullscreen();
    } else if (document.msExitFullscreen) { /* IE/Edge */
        document.msExitFullscreen();
    }
}
async function parseExcelFile(base64Data) {
    return new Promise((resolve, reject) => {
        // Decode base64 string to binary
        const binary = atob(base64Data);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) {
            bytes[i] = binary.charCodeAt(i);
        }

        // Parse the binary data with SheetJS
        const workbook = XLSX.read(bytes, { type: 'array' });
        const sheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[sheetName];
        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

        // Transform rows into JSON format with headers as keys
        const headers = jsonData[0];
        const rows = jsonData.slice(1);
        const jsonFormattedData = rows.map(row =>
            headers.reduce((acc, header, index) => {
                acc[header] = row[index];
                return acc;
            }, {})
        );

        resolve(JSON.stringify(jsonFormattedData));  // Return JSON string
    });
}

window.readExcelFile = async (buffer) => {
    const data = new Uint8Array(buffer);
    const workbook = XLSX.read(data, { type: 'array' });

    // Assuming the first sheet is the one you want
    const firstSheetName = workbook.SheetNames[0];
    const worksheet = workbook.Sheets[firstSheetName];

    // Convert the sheet to JSON
    const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1 });

    // Create an array of products
    const products = [];
    for (let i = 1; i < jsonData.length; i++) { // Start at 1 to skip header
        const row = jsonData[i];
        products.push({
            Id: parseInt(row[0]),
            Name: row[1],
            Price: parseFloat(row[2]),
        });
    }

    return products;
};
function printPdf(pdfBytes) {
    var blob = new Blob([pdfBytes], { type: 'application/pdf' });
    var url = URL.createObjectURL(blob);
    var iframe = document.createElement('iframe');
    iframe.src = url;
    iframe.style.width = '100%';
    iframe.style.height = '100vh';
    document.body.appendChild(iframe);
    window.print();
}
function printHtmlContent(htmlContent) {
    var iframe = document.getElementById('shift_print_iframe');
    if (!iframe) {
        iframe = document.createElement('iframe');
        iframe.id = 'shift_print_iframe';
        iframe.style.position = 'fixed';
        iframe.style.right = '0';
        iframe.style.bottom = '0';
        iframe.style.width = '0';
        iframe.style.height = '0';
        iframe.style.border = '0';
        document.body.appendChild(iframe);
    }
    var doc = iframe.contentWindow || iframe.contentDocument;
    if (doc.document) doc = doc.document;
    doc.open();
    doc.write(htmlContent);
    doc.close();
    setTimeout(function () {
        iframe.contentWindow.focus();
        iframe.contentWindow.print();
    }, 250);
}
window.printHtmlContent = printHtmlContent;
window.Ash = {
    focusNext: function () {
        let items = Array.from(
            document.querySelectorAll(
                "input, select, textarea, button, [tabindex]"
            )
        ).filter(x =>
            !x.disabled &&
            x.tabIndex >= 0 &&
            x.offsetParent !== null // visible
        );

        items.sort((a, b) => a.tabIndex - b.tabIndex);

        let active = document.activeElement;
        let index = items.indexOf(active);

        if (index >= 0 && index < items.length - 1) {
            items[index + 1].focus();
        }
    }
};


window.downloadBase64File = function (base64, fileName) {
    const link = document.createElement('a');
    link.href = base64;
    link.download = fileName;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
};



window.settingsShortcutHandler = null;

window.registerSettingsShortcut = (dotNetHelper) => {
    if (window.settingsShortcutHandler) {
        document.removeEventListener('keydown', window.settingsShortcutHandler);
    }

    window.settingsShortcutHandler = (event) => {
        if (!event.ctrlKey && !event.metaKey && event.altKey && event.shiftKey && (event.key.toLowerCase() === 's' || event.code === 'KeyS')) {
            event.preventDefault();
            dotNetHelper.invokeMethodAsync('OpenPermanentSettings')
                .catch(err => console.error(err));
        }
    };

    document.addEventListener('keydown', window.settingsShortcutHandler);
};
window.designer = {
    _activeHelper: null,
    _keyHandler: null,

    attach: function (dotnetHelper) {
        function move(e) {
            dotnetHelper.invokeMethodAsync("OnMouseMove", e.clientX, e.clientY);
        }

        function up(e) {
            dotnetHelper.invokeMethodAsync("OnMouseUp");
            document.removeEventListener("mousemove", move);
            document.removeEventListener("mouseup", up);
        }

        document.addEventListener("mousemove", move);
        document.addEventListener("mouseup", up);
    },

    registerKeyboard: function (dotnetHelper) {
        window.designer._activeHelper = dotnetHelper;
        if (!window.designer._keyHandler) {
            window.designer._keyHandler = function (e) {
                var active = document.activeElement;
                if (active && (active.tagName === 'INPUT' || active.tagName === 'TEXTAREA' || active.isContentEditable)) {
                    return;
                }

                var keys = ['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Up', 'Down', 'Left', 'Right', 'Escape'];
                if (keys.includes(e.key) || (e.ctrlKey && (e.key === 'a' || e.key === 'A'))) {
                    if (window.designer._activeHelper) {
                        e.preventDefault();
                        window.designer._activeHelper.invokeMethodAsync("HandleKeyDownFromJs", e.key, e.shiftKey, e.ctrlKey, e.altKey)
                            .catch(function () {});
                    }
                }
            };
            document.addEventListener("keydown", window.designer._keyHandler);
        }
    },

    unregisterKeyboard: function () {
        if (window.designer._keyHandler) {
            document.removeEventListener("keydown", window.designer._keyHandler);
            window.designer._keyHandler = null;
        }
        window.designer._activeHelper = null;
    }
};

window.selectTextById = (id) => {
    var el = document.getElementById(id);
    if (!el) return;
    if (el.tagName === 'INPUT') {
        el.focus();
        el.select();
    } else {
        var input = el.querySelector('input');
        if (input) {
            input.focus();
            input.select();
        }
    }
};

window.focusBarcodeInput = () => {
    const doFocus = () => {
        if (document.querySelector('.mud-dialog') || document.querySelector('.mud-overlay')) {
            return;
        }
        var el = document.getElementById('barcode-input') || 
                 document.querySelector('.barcode-input') || 
                 document.getElementById('mob-barcode-input') || 
                 document.querySelector('.mob-barcode-input');
        if (el && document.activeElement !== el) {
            el.focus();
        }
    };
    doFocus();
    requestAnimationFrame(doFocus);
};

window.clearPosBarcodeInputs = () => {
    ['barcode-input', 'mob-barcode-input'].forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.value = '';
        }
    });
    document.querySelectorAll('.pos-search-area input, .barcode-input, .mob-barcode-input').forEach(el => {
        el.value = '';
    });
};

window.initPosGlobalFocusHandler = () => {
    if (window._posClickHandler) {
        document.removeEventListener('click', window._posClickHandler, true);
    }
    window._posClickHandler = (e) => {
        if (document.querySelector('.mud-dialog') || document.querySelector('.mud-overlay')) {
            return;
        }
        const posContainer = document.querySelector('.pos-container');
        if (!posContainer || !posContainer.contains(e.target)) {
            return;
        }
        const tag = e.target.tagName ? e.target.tagName.toUpperCase() : '';
        if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT') {
            return;
        }
        setTimeout(() => {
            if (!document.activeElement || document.activeElement.tagName !== 'INPUT') {
                window.focusBarcodeInput();
            }
        }, 120);
    };
    document.addEventListener('click', window._posClickHandler, true);
};

window.unregisterPosGlobalFocusHandler = () => {
    if (window._posClickHandler) {
        document.removeEventListener('click', window._posClickHandler, true);
        window._posClickHandler = null;
    }
};
window.openPdfFromBytes = (base64) => {
    const byteCharacters = atob(base64);
    const byteNumbers = new Array(byteCharacters.length);

    for (let i = 0; i < byteCharacters.length; i++) {
        byteNumbers[i] = byteCharacters.charCodeAt(i);
    }

    const byteArray = new Uint8Array(byteNumbers);
    const blob = new Blob([byteArray], { type: "application/pdf" });
    const url = URL.createObjectURL(blob);
    window.open(url, "_blank");
};

window.barcodeDesigner = {
    dotNetHelper: null,
    dragElement: null,
    resizeElement: null,
    startX: 0,
    startY: 0,
    startLeft: 0,
    startTop: 0,
    startWidth: 0,
    startHeight: 0,

    init: function (helper) {
        console.log("BarcodeDesigner: Init called");
        this.dotNetHelper = helper;
    },

    makeDraggable: function (elementId, containerId) {
        const elm = document.getElementById(elementId);
        const container = document.getElementById(containerId);
        if (!elm || !container) {
            console.warn("BarcodeDesigner: Element or Container not found", elementId, containerId);
            return;
        }
        console.log("BarcodeDesigner: makeDraggable attached to", elementId);

        elm.onmousedown = (e) => {
            if (e.target.classList.contains('resize-handle')) return; // Ignore if clicking resize handle

            e.preventDefault();
            this.dragElement = elm;
            this.startX = e.clientX;
            this.startY = e.clientY;
            this.startLeft = elm.offsetLeft;
            this.startTop = elm.offsetTop;

            document.onmousemove = (e) => this.elementDrag(e, container);
            document.onmouseup = () => this.stopDrag();
        };
    },

    elementDrag: function (e, container) {
        if (!this.dragElement) return;
        e.preventDefault();

        let dx = e.clientX - this.startX;
        let dy = e.clientY - this.startY;

        let newLeft = this.startLeft + dx;
        let newTop = this.startTop + dy;

        this.dragElement.style.left = newLeft + "px";
        this.dragElement.style.top = newTop + "px";
    },

    stopDrag: function () {
        if (this.dragElement) {
            document.onmousemove = null;
            document.onmouseup = null;

            const x = parseInt(this.dragElement.style.left) || 0;
            const y = parseInt(this.dragElement.style.top) || 0;

            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('UpdateElementPosition', x, y);
            }
            this.dragElement = null;
        }
    },

    makeResizable: function (elementId, handleId) {
        const elm = document.getElementById(elementId);
        const handle = document.getElementById(handleId);
        if (!elm || !handle) return;

        handle.onmousedown = (e) => {
            e.preventDefault();
            e.stopPropagation(); // Prevent drag start
            this.resizeElement = elm;
            this.startX = e.clientX;
            this.startY = e.clientY;
            this.startWidth = elm.offsetWidth;
            this.startHeight = elm.offsetHeight;

            document.onmousemove = (e) => this.elementResize(e);
            document.onmouseup = () => this.stopResize();
        };
    },

    elementResize: function (e) {
        if (!this.resizeElement) return;
        e.preventDefault();

        let dx = e.clientX - this.startX;
        let dy = e.clientY - this.startY;

        let newWidth = this.startWidth + dx;
        let newHeight = this.startHeight + dy;

        // Minimum size
        newWidth = Math.max(20, newWidth);
        newHeight = Math.max(20, newHeight);

        this.resizeElement.style.width = newWidth + "px";
        this.resizeElement.style.height = newHeight + "px";
    },

    stopResize: function () {
        if (this.resizeElement) {
            document.onmousemove = null;
            document.onmouseup = null;

            const w = parseInt(this.resizeElement.style.width) || 0;
            const h = parseInt(this.resizeElement.style.height) || 0;

            if (this.dotNetHelper) {
                this.dotNetHelper.invokeMethodAsync('UpdateElementSize', w, h);
            }
            this.resizeElement = null;
        }
    }
};
window.createPdfUrl = (bytes) => {
    const blob = new Blob([new Uint8Array(bytes)], {
        type: "application/pdf"
    });

    return URL.createObjectURL(blob);
};

window.revokePdfUrl = (url) => {
    URL.revokeObjectURL(url);
};

// Global shortcuts for Login Window
window.registerLoginShortcuts = function (dotNetHelper) {
    if (window._loginKeyHandler) {
        window.removeEventListener('keydown', window._loginKeyHandler, true);
    }
    window._loginKeyHandler = function (e) {
        if (e.altKey && (e.key === 'F3' || e.code === 'F3' || e.keyCode === 114)) {
            e.preventDefault();
            e.stopPropagation();
            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnAltF3Pressed');
            }
        }
    };
    window.addEventListener('keydown', window._loginKeyHandler, true);
};

window.unregisterLoginShortcuts = function () {
    if (window._loginKeyHandler) {
        window.removeEventListener('keydown', window._loginKeyHandler, true);
        window._loginKeyHandler = null;
    }
};

// Global shortcuts for POS View (F1 through F12)
window.registerPosShortcuts = function (dotNetHelper) {
    if (window._posKeyHandler) {
        window.removeEventListener('keydown', window._posKeyHandler, true);
    }
    window._posKeyHandler = function (e) {
        const key = e.key;
        if (key && /^F(1[0-2]|[1-9])$/i.test(key)) {
            // Only handle if POS view container is active in DOM
            if (!document.querySelector('.pos-container')) {
                return;
            }

            // Always prevent browser defaults for function keys (F1 help, F3 search, F5 refresh, F7 caret, F11 fullscreen, F12 devtools)
            e.preventDefault();
            e.stopPropagation();

            // Do not trigger background POS shortcut actions if a modal dialog or overlay is currently open
            if (document.querySelector('.mud-dialog') || document.querySelector('.mud-overlay')) {
                return;
            }

            if (dotNetHelper) {
                dotNetHelper.invokeMethodAsync('OnPosShortcutPressed', key.toUpperCase());
            }
        }
    };
    window.addEventListener('keydown', window._posKeyHandler, true);
};

window.unregisterPosShortcuts = function () {
    if (window._posKeyHandler) {
        window.removeEventListener('keydown', window._posKeyHandler, true);
        window._posKeyHandler = null;
    }
};

// POS Audio Beep (Web Audio API Synthesizer)
window.playPosBeepSound = function () {
    try {
        const AudioContextClass = window.AudioContext || window.webkitAudioContext;
        if (!AudioContextClass) return;

        if (!window._posAudioCtx) {
            window._posAudioCtx = new AudioContextClass();
        }
        const ctx = window._posAudioCtx;

        if (ctx.state === 'suspended') {
            ctx.resume();
        }

        const playTone = (freq, startTime, duration, type = 'square') => {
            const osc = ctx.createOscillator();
            const gain = ctx.createGain();
            osc.type = type;
            osc.frequency.setValueAtTime(freq, startTime);
            gain.gain.setValueAtTime(0.3, startTime);
            gain.gain.exponentialRampToValueAtTime(0.001, startTime + duration);
            osc.connect(gain);
            gain.connect(ctx.destination);
            osc.start(startTime);
            osc.stop(startTime + duration);
        };

        const now = ctx.currentTime;
        // Two-tone barcode scanner warning beep: 550Hz for 140ms, pause, 400Hz for 180ms
        playTone(550, now, 0.14, 'square');
        playTone(400, now + 0.18, 0.18, 'square');
    } catch (e) {
        console.warn("Could not play POS beep sound:", e);
    }
};
