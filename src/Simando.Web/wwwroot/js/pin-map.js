// JS interop backing PinDropMap.razor. Single draggable marker, click to
// place/move — not the multi-pin, layered map that /map (a00f) will need,
// which is a separate, much larger component.
const maps = {};

const defaultCenter = [-2.5, 118.0]; // Indonesia-wide — no village carries
const defaultZoom = 5;               // coordinates in this domain model to auto-centre on instead.

export function initMap(elementId, dotNetRef, initialLat, initialLng) {
    const hasInitial = initialLat != null && initialLng != null;
    const center = hasInitial ? [initialLat, initialLng] : defaultCenter;

    const map = L.map(elementId).setView(center, hasInitial ? 15 : defaultZoom);
    L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
        attribution: "&copy; OpenStreetMap contributors",
        maxZoom: 19,
    }).addTo(map);

    let marker = hasInitial ? placeMarker(map, center, dotNetRef) : null;

    map.on("click", (e) => {
        if (marker) {
            marker.setLatLng(e.latlng);
            notify(marker, dotNetRef);
        } else {
            marker = placeMarker(map, e.latlng, dotNetRef);
            notify(marker, dotNetRef);
        }
    });

    maps[elementId] = map;
}

function placeMarker(map, latlng, dotNetRef) {
    const marker = L.marker(latlng, { draggable: true }).addTo(map);
    marker.on("dragend", () => notify(marker, dotNetRef));
    return marker;
}

function notify(marker, dotNetRef) {
    const pos = marker.getLatLng();
    dotNetRef.invokeMethodAsync("OnPinMoved", pos.lat, pos.lng);
}

export function dispose(elementId) {
    const map = maps[elementId];
    if (map) {
        map.remove();
        delete maps[elementId];
    }
}
