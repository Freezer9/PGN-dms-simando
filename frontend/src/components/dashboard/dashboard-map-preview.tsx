import { Link } from "@tanstack/react-router";
import { ExternalLink } from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
	Map,
	MapControls,
	MapMarker,
	MarkerContent,
	MarkerPopup,
} from "@/components/ui/map";

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

	const validPins = React.useMemo(() => {
		if (!pinsData) return [];
		return pinsData.filter(
			(p) => typeof p.latitude === "number" && typeof p.longitude === "number",
		);
	}, [pinsData]);

	const defaultCenter: [number, number] = React.useMemo(() => {
		if (validPins.length > 0) {
			const avgLng =
				validPins.reduce((sum, p) => sum + (p.longitude as number), 0) /
				validPins.length;
			const avgLat =
				validPins.reduce((sum, p) => sum + (p.latitude as number), 0) /
				validPins.length;
			return [avgLng, avgLat];
		}
		return [108.5, -6.8];
	}, [validPins]);

	return (
		<Card className="shadow-xs overflow-hidden">
			<CardHeader className="flex flex-row items-center justify-between pb-3">
				<div>
					<CardTitle className="text-base font-semibold">
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
						zoom={6}
						className="h-[260px] w-full rounded-none border-t border-b-0"
					>
						<MapControls />
						{validPins.map((p) => {
							const stageColor =
								STAGE_PIN_COLORS[Number(p.currentStage)] || "#3b82f6";
							return (
								<MapMarker
									key={p.id}
									longitude={p.longitude as number}
									latitude={p.latitude as number}
								>
									<MarkerContent>
										<div
											className="size-4 rounded-full border-2 border-white shadow-xs cursor-pointer transition-transform hover:scale-125"
											style={{ backgroundColor: stageColor }}
											title={p.namaPerusahaan}
										/>
									</MarkerContent>
									<MarkerPopup>
										<div className="text-xs space-y-0.5">
											<div className="font-semibold">{p.namaPerusahaan}</div>
											<div className="text-[11px] text-muted-foreground">
												Tahap {p.currentStage}: {p.locationLabel || "Area"}
											</div>
										</div>
									</MarkerPopup>
								</MapMarker>
							);
						})}
					</Map>
					<div className="absolute bottom-2 left-2 z-10 flex flex-wrap items-center gap-2 bg-background/90 backdrop-blur-xs px-2.5 py-1 rounded-md border text-[11px] font-medium shadow-xs">
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#64748b]" /> T1:
							Direktori
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#3b82f6]" /> T2: Plotting
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#0284c7]" /> T3: Prospek
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#10b981]" /> T4: Survei
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#f59e0b]" /> T5:
							Registrasi
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#f97316]" /> T6:
							Permohonan
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#a855f7]" /> T7: Evaluasi
						</span>
						<span className="flex items-center gap-1">
							<span className="size-2 rounded-full bg-[#22c55e]" /> T8: Terbit
							NOL
						</span>
					</div>
				</div>
			</CardContent>
		</Card>
	);
}
