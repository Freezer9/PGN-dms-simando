// Leaflet interop for the Peta page and the directory map picker.
// Leaflet is vendored at wwwroot/lib/leaflet and loaded as a classic script on first use —
// the dist bundle is UMD, not an ES module, so it cannot be `import`ed directly.

let leafletReady = null;

function loadLeaflet() {
    if (window.L) return Promise.resolve(window.L);

    leafletReady ??= new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = 'lib/leaflet/leaflet.js';
        script.onload = () => resolve(window.L);
        script.onerror = () => reject(new Error('Gagal memuat Leaflet.'));
        document.head.appendChild(script);
    });

    return leafletReady;
}

// Default marker images resolve relative to the CSS by default; point them at the vendored copies.
function configureIcons(L) {
    L.Icon.Default.mergeOptions({
        iconRetinaUrl: 'lib/leaflet/images/marker-icon-2x.png',
        iconUrl: 'lib/leaflet/images/marker-icon.png',
        shadowUrl: 'lib/leaflet/images/marker-shadow.png'
    });
}

// Indonesia, roughly centred on Java where the seeded areas sit.
const FALLBACK_CENTER = [-6.9, 107.6];
const FALLBACK_ZOOM = 8;

const instances = new Map();

export async function init(element, pins, editable, dotNetRef) {
    const L = await loadLeaflet();
    configureIcons(L);

    if (!element) return;

    // Blazor can re-run OnAfterRender against the same node; drop the previous map first.
    const existing = instances.get(element);
    if (existing) existing.map.remove();

    const map = L.map(element).setView(FALLBACK_CENTER, FALLBACK_ZOOM);

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap'
    }).addTo(map);

    const state = { map, markers: [], L, dotNetRef };
    instances.set(element, state);

    if (editable && dotNetRef) {
        map.on('click', e => {
            dotNetRef.invokeMethodAsync('OnMapClick', e.latlng.lat, e.latlng.lng);
        });
    }

    setPins(element, pins);
    return true;
}

export function setPins(element, pins) {
    const state = instances.get(element);
    if (!state) return;

    const { map, L } = state;
    state.markers.forEach(marker => marker.remove());
    state.markers = [];

    const points = (pins ?? []).filter(p => p.latitude != null && p.longitude != null);

    points.forEach(pin => {
        const marker = L.marker([pin.latitude, pin.longitude]).addTo(map);
        const label = pin.url
            ? `<a href="${pin.url}">${escapeHtml(pin.label)}</a>`
            : escapeHtml(pin.label);
        marker.bindPopup(`<strong>${label}</strong>${pin.detail ? `<br />${escapeHtml(pin.detail)}` : ''}`);
        state.markers.push(marker);
    });

    if (points.length === 1) {
        map.setView([points[0].latitude, points[0].longitude], 13);
    } else if (points.length > 1) {
        map.fitBounds(L.latLngBounds(points.map(p => [p.latitude, p.longitude])), { padding: [40, 40] });
    }

    // A map created inside a hidden or freshly-sized container measures 0×0; nudge it.
    setTimeout(() => map.invalidateSize(), 0);
}

export function dispose(element) {
    const state = instances.get(element);
    if (!state) return;
    state.map.remove();
    instances.delete(element);
}

function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, ch => ({
        '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
    })[ch]);
}
