import {
	Map as MapLibreInstance,
	Marker,
	NavigationControl,
	Popup,
	type StyleSpecification,
} from "maplibre-gl";
import * as React from "react";
import "maplibre-gl/dist/maplibre-gl.css";
import { cn } from "@/lib/utils";

export interface MapCoordinates {
	longitude: number;
	latitude: number;
}

export interface MapPin {
	id: string;
	coordinates: MapCoordinates;
	title?: string;
	description?: string;
	color?: string;
}

export interface MapProps extends React.HTMLAttributes<HTMLDivElement> {
	center?: [number, number]; // [lng, lat]
	zoom?: number;
	interactive?: boolean;
	pins?: MapPin[];
	selectedCoordinates?: MapCoordinates | null;
	onCoordinateSelect?: (coords: MapCoordinates) => void;
	mapStyle?: string | StyleSpecification;
}

// Default to Indonesia center [lng, lat]
const DEFAULT_CENTER: [number, number] = [106.8456, -6.2088]; // Jakarta
const DEFAULT_ZOOM = 11;

const DEFAULT_MAP_STYLE: StyleSpecification = {
	version: 8,
	sources: {
		osm: {
			type: "raster",
			tiles: ["https://tile.openstreetmap.org/{z}/{x}/{y}.png"],
			tileSize: 256,
			attribution: "&copy; OpenStreetMap contributors",
		},
	},
	layers: [
		{
			id: "osm-layer",
			type: "raster",
			source: "osm",
			minzoom: 0,
			maxzoom: 19,
		},
	],
};

export const Map = React.forwardRef<HTMLDivElement, MapProps>(
	(
		{
			center = DEFAULT_CENTER,
			zoom = DEFAULT_ZOOM,
			interactive = true,
			pins = [],
			selectedCoordinates,
			onCoordinateSelect,
			mapStyle = DEFAULT_MAP_STYLE,
			className,
			...props
		},
		ref,
	) => {
		const containerRef = React.useRef<HTMLDivElement>(null);
		const mapInstanceRef = React.useRef<MapLibreInstance | null>(null);
		const activeMarkerRef = React.useRef<Marker | null>(null);
		const pinMarkersRef = React.useRef<Marker[]>([]);

		React.useImperativeHandle(
			ref,
			() => containerRef.current as HTMLDivElement,
		);

		// Initialize Map
		// biome-ignore lint/correctness/useExhaustiveDependencies: Initialize map instance once on mount
		React.useEffect(() => {
			if (!containerRef.current) return;

			try {
				const map = new MapLibreInstance({
					container: containerRef.current,
					style: mapStyle,
					center,
					zoom,
					interactive,
					attributionControl: false,
				});

				if (interactive) {
					map.addControl(
						new NavigationControl({
							showCompass: true,
							showZoom: true,
						}),
						"top-right",
					);
				}

				map.on("click", (e) => {
					if (!onCoordinateSelect) return;
					const coords: MapCoordinates = {
						longitude: e.lngLat.lng,
						latitude: e.lngLat.lat,
					};
					onCoordinateSelect(coords);
				});

				mapInstanceRef.current = map;

				return () => {
					try {
						map.remove();
					} catch {
						// safe cleanup
					}
					mapInstanceRef.current = null;
				};
			} catch (err) {
				console.warn("MapLibre GL initialization skipped:", err);
			}
		}, []);

		// Update center / zoom if changed
		React.useEffect(() => {
			try {
				const map = mapInstanceRef.current;
				if (!map) return;
				map.setCenter(center);
				map.setZoom(zoom);
			} catch (err) {
				console.warn("Map setCenter/setZoom skipped:", err);
			}
		}, [center, zoom]);

		// Update interactive selected coordinate pin
		React.useEffect(() => {
			try {
				const map = mapInstanceRef.current;
				if (!map) return;

				if (activeMarkerRef.current) {
					try {
						activeMarkerRef.current.remove();
					} catch {
						// safe cleanup
					}
					activeMarkerRef.current = null;
				}

				if (selectedCoordinates) {
					const marker = new Marker({
						color: "#00509E",
						draggable: true,
					})
						.setLngLat([
							selectedCoordinates.longitude,
							selectedCoordinates.latitude,
						])
						.addTo(map);

					marker.on("dragend", () => {
						const lngLat = marker.getLngLat();
						onCoordinateSelect?.({
							longitude: lngLat.lng,
							latitude: lngLat.lat,
						});
					});

					activeMarkerRef.current = marker;
				}
			} catch (err) {
				console.warn("Active marker update skipped:", err);
			}
		}, [selectedCoordinates, onCoordinateSelect]);

		// Update pins
		React.useEffect(() => {
			try {
				const map = mapInstanceRef.current;
				if (!map) return;

				// Clear existing pins
				for (const m of pinMarkersRef.current) {
					try {
						m.remove();
					} catch {
						// safe cleanup
					}
				}
				pinMarkersRef.current = [];

				for (const pin of pins) {
					const marker = new Marker({
						color: pin.color || "#2563eb",
					}).setLngLat([pin.coordinates.longitude, pin.coordinates.latitude]);

					if (pin.title || pin.description) {
						const popup = new Popup({ offset: 25 }).setHTML(`
	            <div class="p-2">
	              ${pin.title ? `<strong class="text-sm font-semibold">${pin.title}</strong>` : ""}
	              ${pin.description ? `<p class="text-xs text-gray-600 mt-1">${pin.description}</p>` : ""}
	            </div>
	          `);
						marker.setPopup(popup);
					}

					marker.addTo(map);
					pinMarkersRef.current.push(marker);
				}
			} catch (err) {
				console.warn("Pin markers update skipped:", err);
			}
		}, [pins]);

		return (
			<div
				ref={containerRef}
				className={cn(
					"relative h-[400px] w-full overflow-hidden rounded-lg border bg-muted",
					className,
				)}
				{...props}
			/>
		);
	},
);

Map.displayName = "Map";
