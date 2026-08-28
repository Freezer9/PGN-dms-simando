import { Link } from "@tanstack/react-router";
import { ExternalLink, MapPin as MapPinIcon } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import { Map, type MapPin } from "@/components/map";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

const STAGE_PIN_COLORS: Record<number, string> = {
	1: "#64748b",
	2: "#3b82f6",
	3: "#0284c7",
	4: "#10b981",
	5: "#f59e0b",
	6: "#f97316",
	7: "#a855f7",
	8: "#22c55e",
};

export function DashboardMapPreview() {
	const { data: pinsData } = $api.useQuery("get", "/api/companies/map-pins");

	const mapPins: MapPin[] = React.useMemo(() => {
		if (!pinsData) return [];
		return pinsData
			.filter(
				(p) =>
					typeof p.latitude === "number" && typeof p.longitude === "number",
			)
			.slice(0, 100)
			.map((p) => ({
				id: p.id,
				coordinates: {
					latitude: p.latitude as number,
					longitude: p.longitude as number,
				},
				title: p.namaPerusahaan,
				description: `Tahap ${p.currentStage}: ${p.locationLabel || "Area"}`,
				color: STAGE_PIN_COLORS[Number(p.currentStage)] || "#2563eb",
			}));
	}, [pinsData]);

	const defaultCenter: [number, number] = React.useMemo(() => {
		if (mapPins.length > 0) {
			return [
				mapPins[0].coordinates.longitude,
				mapPins[0].coordinates.latitude,
			];
		}
		return [112.7521, -7.2575]; // Surabaya default center
	}, [mapPins]);

	return (
		<Card className="shadow-sm overflow-hidden">
			<CardHeader className="flex flex-row items-center justify-between pb-3">
				<div>
					<CardTitle className="text-base font-semibold flex items-center gap-2">
						<MapPinIcon className="size-4 text-primary" />
						Peta Sebaran Pelanggan & Prospek
					</CardTitle>
					<p className="text-xs text-muted-foreground mt-0.5">
						Sebaran lokasi pelanggan dan calon pelanggan di wilayah kerja
					</p>
				</div>
				<Button variant="outline" size="sm" asChild className="gap-1.5 h-8">
					<Link to="/map">
						<ExternalLink className="size-3.5" />
						Buka Peta Penuh
					</Link>
				</Button>
			</CardHeader>
			<CardContent className="p-0">
				<div className="relative">
					<Map
						center={defaultCenter}
						zoom={10}
						interactive={true}
						pins={mapPins}
						className="h-[240px] w-full rounded-none border-t border-b-0"
					/>
					<div className="absolute bottom-2 left-2 z-10 flex flex-wrap items-center gap-2 bg-background/90 backdrop-blur-xs px-2.5 py-1 rounded-md border text-[11px] font-medium shadow-xs">
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#64748b]" /> Direktori
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#10b981]" /> Survei (KK0)
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#f59e0b]" /> Registrasi
							A1
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#22c55e]" /> Terbit NOL
						</span>
					</div>
				</div>
			</CardContent>
		</Card>
	);
}
