// JS interop backing MapPage.razor (/map) — multi-pin, clustered, stage-
// layered map. Separate from pin-map.js (PinDropMap.razor's single
// draggable-marker form picker).
const maps = {};

const STAGE_COLORS = {
    "1": "#94a3b8", // Direktori — slate
    "2": "#60a5fa", // Plotting — blue
    "3": "#38bdf8", // Prospek — sky
    "4": "#34d399", // Survei — emerald
    "5": "#fbbf24", // A1 — amber
    "6": "#fb923c", // Permohonan NOL — orange
    "nol": "#22c55e", // NOL Terbit — green
};

const MIN_RADIUS = 6;
const MAX_RADIUS = 24;
const SIZE_SCALE_FACTOR = 4;

const defaultCenter = [-2.5, 118.0];
const defaultZoom = 5;

export function initMap(elementId, dotNetRef, pins, canDrag) {
    const map = L.map(elementId).setView(defaultCenter, defaultZoom);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap contributors",
        maxZoom: 19,
    }).addTo(map);

    const layers = {};
    const markersById = {};

    for (const pin of pins) {
        const layer = (layers[pin.stageBucket] ??= L.markerClusterGroup());

        const radius = Math.min(MAX_RADIUS, MIN_RADIUS + pin.size * SIZE_SCALE_FACTOR);
        const marker = L.circleMarker([pin.lat, pin.lng], {
            radius,
            color: "#1f2937",
            weight: 2,
            dashArray: pin.dashed ? "4,3" : null,
            fillColor: STAGE_COLORS[pin.stageBucket] ?? STAGE_COLORS["1"],
            fillOpacity: 0.85,
        });

        marker.bindPopup(popupHtml(pin));

        if (canDrag) {
            makeDraggable(marker, map, pin.id, dotNetRef);
        }

        layer.addLayer(marker);
        markersById[pin.id] = marker;
    }

    for (const layer of Object.values(layers)) {
        layer.addTo(map);
    }

    maps[elementId] = { map, layers, markersById };
}

function makeDraggable(marker, map, companyId, dotNetRef) {
    let dragging = false;
    let start = null;

    marker.on("mousedown", (e) => {
        dragging = true;
        start = marker.getLatLng();
        map.dragging.disable();
        L.DomEvent.stop(e.originalEvent);
    });

    map.on("mousemove", (e) => {
        if (!dragging) return;
        marker.setLatLng(e.latlng);
    });

    map.on("mouseup", () => {
        if (!dragging) return;
        dragging = false;
        map.dragging.enable();

        const pos = marker.getLatLng();
        if (start && (pos.lat !== start.lat || pos.lng !== start.lng)) {
            dotNetRef.invokeMethodAsync("OnPinDragged", companyId, pos.lat, pos.lng);
        }
    });
}

function popupHtml(pin) {
    const nomor = pin.nomor ? escapeHtml(pin.nomor) : "-";
    const nama = escapeHtml(pin.nama);
    const industry = pin.industryTypeName ? escapeHtml(pin.industryTypeName) : "";
    const stage = escapeHtml(pin.stageLabel);
    const posisi = pin.posisiPelangganLabel ? escapeHtml(pin.posisiPelangganLabel) : "";
    const sales = pin.salesUserName ? escapeHtml(pin.salesUserName) : "";

    const stageBg = STAGE_COLORS[pin.stageBucket] ?? "#94a3b8";

    return `
        <div style="min-width: 240px; max-width: 280px; font-family: system-ui, -apple-system, sans-serif; font-size: 12px; line-height: 1.4; color: #1e293b;">
            <div style="margin-bottom: 8px;">
                <div style="font-weight: 700; font-size: 13px; color: #0f172a; margin-bottom: 2px;">${nama}</div>
                <div style="font-size: 11px; color: #64748b;">${nomor} ${industry ? '· ' + industry : ''}</div>
            </div>

            <div style="display: inline-block; padding: 2px 8px; border-radius: 4px; font-size: 11px; font-weight: 600; color: #ffffff; background-color: ${stageBg}; margin-bottom: 8px;">
                ${stage}
            </div>

            <div style="border-top: 1px solid #e2e8f0; padding-top: 6px; margin-bottom: 8px; font-size: 11px; display: flex; flex-direction: column; gap: 3px;">
                ${posisi ? `<div style="display: flex; justify-content: space-between;"><span style="color: #64748b;">Posisi:</span> <strong style="color: #334155;">${posisi}</strong></div>` : ''}
                ${sales ? `<div style="display: flex; justify-content: space-between;"><span style="color: #64748b;">Sales PIC:</span> <strong style="color: #334155;">${sales}</strong></div>` : ''}
            </div>

            <div style="text-align: right; border-top: 1px solid #f1f5f9; padding-top: 6px;">
                <a href="/companies/${pin.id}" style="display: inline-flex; items-center; justify-content: center; gap: 4px; padding: 5px 10px; border-radius: 5px; background-color: #0284c7; color: #ffffff; font-weight: 600; text-decoration: none; font-size: 11px;">
                    <span>Buka Record Hub</span>
                    <svg style="width: 12px; height: 12px; display: inline-block; vertical-align: middle;" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14 5l7 7m0 0l-7 7m7-7H3"></path></svg>
                </a>
            </div>
        </div>`;
}

function escapeHtml(value) {
    const div = document.createElement("div");
    div.textContent = value ?? "";
    return div.innerHTML;
}

export function setLayerVisible(elementId, bucketKey, visible) {
    const entry = maps[elementId];
    if (!entry) return;

    const layer = entry.layers[bucketKey];
    if (!layer) return;

    if (visible && !entry.map.hasLayer(layer)) {
        layer.addTo(entry.map);
    } else if (!visible && entry.map.hasLayer(layer)) {
        entry.map.removeLayer(layer);
    }
}

export function revertPin(elementId, companyId, lat, lng) {
    const entry = maps[elementId];
    const marker = entry?.markersById[companyId];
    if (marker) {
        marker.setLatLng([lat, lng]);
    }
}

export function dispose(elementId) {
    const entry = maps[elementId];
    if (entry) {
        entry.map.remove();
        delete maps[elementId];
    }
}
