import { createFileRoute, Link } from "@tanstack/react-router";
import {
	Building2,
	ChevronRight,
	ExternalLink,
	Filter,
	Loader2,
	MapPin as MapPinIcon,
	RotateCcw,
	Search,
	X,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { CompanyMapPinDto } from "@/api/types";
import { Map, type MapPin } from "@/components/map";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import {
	getKawasanLabel,
	getPosisiPelangganLabel,
	getStageInfo,
	getStatusLabel,
	STAGE_CONFIG,
} from "@/lib/directory-utils";

export const Route = createFileRoute("/_auth/map")({
	component: GeospatialMapPage,
});

const STAGE_PIN_COLORS: Record<number, string> = {
	1: "#64748b", // Slate
	2: "#3b82f6", // Blue
	3: "#0284c7", // Sky
	4: "#10b981", // Emerald
	5: "#f59e0b", // Amber
	6: "#f97316", // Orange
	7: "#a855f7", // Purple
	8: "#22c55e", // Green
};

function GeospatialMapPage() {
	// Filter states
	const [searchTerm, setSearchTerm] = React.useState("");
	const [selectedStages, setSelectedStages] = React.useState<number[]>([]);
	const [selectedIndustry, setSelectedIndustry] = React.useState<string>("ALL");
	const [selectedPosisi, setSelectedPosisi] = React.useState<string>("ALL");
	const [selectedKawasan, setSelectedKawasan] = React.useState<string>("ALL");

	// Active Selected Pin for detail drawer
	const [selectedPin, setSelectedPin] = React.useState<CompanyMapPinDto | null>(
		null,
	);
	const [isFilterOpen, setIsFilterOpen] = React.useState(true);

	// Fetch Pins
	const { data: pinsData, isLoading } = $api.useQuery(
		"get",
		"/api/companies/map-pins",
	);

	// Fetch Master Data
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);

	// Filter Pins
	const filteredPins = React.useMemo(() => {
		if (!pinsData) return [];

		return pinsData.filter((item) => {
			// Search filter
			if (searchTerm) {
				const term = searchTerm.toLowerCase();
				const matchName = item.namaPerusahaan.toLowerCase().includes(term);
				const matchNomor = item.nomor.toLowerCase().includes(term);
				if (!matchName && !matchNomor) return false;
			}

			// Stage filter
			if (selectedStages.length > 0) {
				if (!selectedStages.includes(Number(item.currentStage))) return false;
			}

			// Industry filter
			if (selectedIndustry !== "ALL") {
				if (item.industryTypeName !== selectedIndustry) return false;
			}

			// Posisi filter
			if (selectedPosisi !== "ALL") {
				if (item.posisiPelanggan !== selectedPosisi) return false;
			}

			// Kawasan filter
			if (selectedKawasan !== "ALL") {
				if (item.kawasan !== selectedKawasan) return false;
			}

			return true;
		});
	}, [
		pinsData,
		searchTerm,
		selectedStages,
		selectedIndustry,
		selectedPosisi,
		selectedKawasan,
	]);

	// Convert to Map Component Pin Format
	const mapPins: MapPin[] = React.useMemo(() => {
		return filteredPins.map((item) => ({
			id: item.id,
			coordinates: {
				latitude: Number(item.latitude),
				longitude: Number(item.longitude),
			},
			title: item.namaPerusahaan,
			description: `${item.nomor} — ${item.industryTypeName || "Industri"} (${getStageInfo(item.currentStage).shortName})`,
			color: STAGE_PIN_COLORS[Number(item.currentStage)] || "#3b82f6",
		}));
	}, [filteredPins]);

	const toggleStage = (stage: number) => {
		setSelectedStages((prev) =>
			prev.includes(stage) ? prev.filter((s) => s !== stage) : [...prev, stage],
		);
	};

	const resetFilters = () => {
		setSearchTerm("");
		setSelectedStages([]);
		setSelectedIndustry("ALL");
		setSelectedPosisi("ALL");
		setSelectedKawasan("ALL");
	};

	return (
		<div className="relative h-[calc(100vh-8rem)] w-full flex flex-col overflow-hidden rounded-lg border shadow-xs">
			{/* Top Header Bar */}
			<div className="h-12 bg-background border-b px-4 flex items-center justify-between shrink-0 z-10">
				<div className="flex items-center gap-3">
					<div className="flex items-center gap-1.5 font-semibold text-sm">
						<MapPinIcon className="size-4 text-primary" />
						<span>Peta Sebaran Pelanggan & Jaringan Pipa</span>
					</div>
					<Badge variant="outline" className="text-xs font-normal">
						{filteredPins.length} dari {pinsData?.length || 0} Titik
					</Badge>
				</div>

				<div className="flex items-center gap-2">
					<Button
						variant="outline"
						size="sm"
						onClick={() => setIsFilterOpen(!isFilterOpen)}
						className="h-8 text-xs flex items-center gap-1.5"
					>
						<Filter className="size-3.5" />
						<span>Filter Spasial</span>
					</Button>
					<Button variant="default" size="sm" asChild className="h-8 text-xs">
						<Link to="/directory" className="flex items-center gap-1">
							<Building2 className="size-3.5" />
							<span>Tabel Direktori</span>
						</Link>
					</Button>
				</div>
			</div>

			{/* Map Container Area */}
			<div className="relative flex-1 w-full h-full overflow-hidden">
				{isLoading ? (
					<div className="absolute inset-0 flex flex-col items-center justify-center bg-muted/40 z-20 space-y-2">
						<Loader2 className="size-8 animate-spin text-primary" />
						<span className="text-xs text-muted-foreground">
							Memuat data spasial GIS...
						</span>
					</div>
				) : null}

				<Map
					pins={mapPins}
					interactive={true}
					zoom={11}
					className="h-full w-full rounded-none border-0"
				/>

				{/* Floating Filter Overlay Panel */}
				{isFilterOpen && (
					<Card className="absolute top-4 left-4 w-80 max-h-[calc(100%-2rem)] overflow-y-auto z-20 shadow-lg border-border/80 bg-background/95 backdrop-blur-xs">
						<CardHeader className="p-3 pb-2 flex flex-row items-center justify-between">
							<CardTitle className="text-xs font-bold uppercase tracking-wider flex items-center gap-1.5 text-muted-foreground">
								<Filter className="size-3.5 text-primary" />
								Filter Peta
							</CardTitle>
							<Button
								variant="ghost"
								size="icon"
								className="size-6"
								onClick={() => setIsFilterOpen(false)}
							>
								<X className="size-3.5" />
							</Button>
						</CardHeader>
						<CardContent className="p-3 pt-0 space-y-3 text-xs">
							{/* Search */}
							<div className="relative">
								<Search className="absolute left-2.5 top-2 size-3.5 text-muted-foreground" />
								<Input
									placeholder="Cari nama atau nomor..."
									value={searchTerm}
									onChange={(e) => setSearchTerm(e.target.value)}
									className="pl-8 h-8 text-xs"
								/>
							</div>

							{/* Sektor Industri */}
							<div className="space-y-1">
								<Label className="text-[11px] font-medium text-muted-foreground">
									Sektor Industri
								</Label>
								<Select
									value={selectedIndustry}
									onValueChange={setSelectedIndustry}
								>
									<SelectTrigger className="h-8 text-xs">
										<SelectValue placeholder="Semua Sektor" />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="ALL">Semua Sektor Industri</SelectItem>
										{industryTypes?.map((it) => (
											<SelectItem key={it.id} value={it.name}>
												{it.name}
											</SelectItem>
										))}
									</SelectContent>
								</Select>
							</div>

							{/* Posisi Pelanggan */}
							<div className="space-y-1">
								<Label className="text-[11px] font-medium text-muted-foreground">
									Jalur Pipa
								</Label>
								<Select
									value={selectedPosisi}
									onValueChange={setSelectedPosisi}
								>
									<SelectTrigger className="h-8 text-xs">
										<SelectValue placeholder="Semua Jalur" />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="ALL">Semua Jalur</SelectItem>
										<SelectItem value="JalurExisting">
											Jalur Existing
										</SelectItem>
										<SelectItem value="Pengembangan">Pengembangan</SelectItem>
									</SelectContent>
								</Select>
							</div>

							{/* Stage Checkboxes with Legend Colors */}
							<div className="space-y-1.5 pt-1 border-t">
								<Label className="text-[11px] font-semibold text-muted-foreground block">
									Filter Berdasarkan Tahapan
								</Label>
								<div className="space-y-1">
									{[1, 2, 3, 4, 5, 6, 7, 8].map((s) => {
										const info = STAGE_CONFIG[s];
										const isChecked = selectedStages.includes(s);
										return (
											<label
												key={s}
												className="flex items-center gap-2 py-0.5 cursor-pointer hover:bg-muted/40 px-1 rounded transition-colors"
											>
												<input
													type="checkbox"
													checked={isChecked}
													onChange={() => toggleStage(s)}
													className="rounded border-gray-300 size-3.5 text-primary focus:ring-primary"
												/>
												<span
													className="size-2.5 rounded-full shrink-0"
													style={{ backgroundColor: STAGE_PIN_COLORS[s] }}
												/>
												<span className="text-[11px] truncate leading-none">
													{info.name}
												</span>
											</label>
										);
									})}
								</div>
							</div>

							{/* Posisi Kawasan */}
							<div className="space-y-1 pt-1 border-t">
								<Label className="text-[11px] font-medium text-muted-foreground">
									Kawasan
								</Label>
								<Select
									value={selectedKawasan}
									onValueChange={setSelectedKawasan}
								>
									<SelectTrigger className="h-8 text-xs">
										<SelectValue placeholder="Semua Kawasan" />
									</SelectTrigger>
									<SelectContent>
										<SelectItem value="ALL">Semua Kawasan</SelectItem>
										<SelectItem value="KawasanIndustri">
											Kawasan Industri
										</SelectItem>
										<SelectItem value="NonKawasanIndustri">
											Non Kawasan Industri
										</SelectItem>
									</SelectContent>
								</Select>
							</div>

							{/* Reset Button */}
							<Button
								variant="outline"
								size="sm"
								onClick={resetFilters}
								className="w-full h-7 text-xs flex items-center justify-center gap-1 mt-2 text-muted-foreground hover:text-foreground"
							>
								<RotateCcw className="size-3" />
								<span>Reset Filter</span>
							</Button>
						</CardContent>
					</Card>
				)}

				{/* Floating Results Quick List (Bottom Left) */}
				<div className="absolute bottom-4 left-4 right-4 sm:right-auto sm:w-96 max-h-48 overflow-y-auto z-10 flex flex-col gap-1.5 p-1 bg-background/80 backdrop-blur-xs rounded-lg border shadow-md">
					<div className="px-2 py-1 text-[11px] font-semibold text-muted-foreground flex justify-between items-center border-b">
						<span>Daftar Titik ({filteredPins.length})</span>
						<span className="text-[10px] text-primary">
							Klik item untuk detail
						</span>
					</div>
					{filteredPins.slice(0, 10).map((p) => {
						const stage = getStageInfo(p.currentStage);
						return (
							<Card
								key={p.id}
								className="p-2 cursor-pointer hover:bg-muted/60 transition-colors shadow-none border-border/60"
								onClick={() => setSelectedPin(p)}
							>
								<div className="flex items-center justify-between">
									<Badge
										variant="outline"
										className="font-mono text-[10px] px-1.5 py-0"
									>
										{p.nomor}
									</Badge>
									<span
										className="size-2 rounded-full"
										style={{
											backgroundColor: STAGE_PIN_COLORS[Number(p.currentStage)],
										}}
									/>
								</div>
								<div className="font-semibold text-xs text-foreground truncate">
									{p.namaPerusahaan}
								</div>
								<div className="flex items-center justify-between text-[11px] text-muted-foreground">
									<span>{stage.shortName}</span>
									<Link
										to="/directory/$companyId"
										params={{ companyId: p.id }}
										className="text-primary hover:underline flex items-center gap-0.5"
										onClick={(e) => e.stopPropagation()}
									>
										<span>Buka</span>
										<ChevronRight className="size-3" />
									</Link>
								</div>
							</Card>
						);
					})}
				</div>

				{/* Floating Detail Drawer for Selected Pin */}
				{selectedPin && (
					<Card className="absolute top-4 right-4 w-88 max-h-[calc(100%-2rem)] overflow-y-auto z-20 shadow-xl border-border/80 bg-background/95 backdrop-blur-xs">
						<CardHeader className="p-4 pb-2 flex flex-row items-center justify-between">
							<div>
								<Badge variant="outline" className="font-mono text-[10px]">
									{selectedPin.nomor}
								</Badge>
								<CardTitle className="text-sm font-bold mt-1 text-foreground">
									{selectedPin.namaPerusahaan}
								</CardTitle>
							</div>
							<Button
								variant="ghost"
								size="icon"
								className="size-6"
								onClick={() => setSelectedPin(null)}
							>
								<X className="size-4" />
							</Button>
						</CardHeader>
						<CardContent className="p-4 space-y-3 text-xs">
							<div className="flex flex-wrap items-center gap-1.5">
								<Badge
									variant="outline"
									className={`text-[10px] ${getStageInfo(selectedPin.currentStage).badgeClass}`}
								>
									{getStageInfo(selectedPin.currentStage).name}
								</Badge>
								<Badge
									variant="outline"
									className={`text-[10px] ${getStatusLabel(selectedPin.status).badgeClass}`}
								>
									{getStatusLabel(selectedPin.status).label}
								</Badge>
							</div>

							<div className="space-y-1.5 pt-2 border-t">
								<div className="flex justify-between">
									<span className="text-muted-foreground">
										Sektor Industri:
									</span>
									<span className="font-medium text-foreground">
										{selectedPin.industryTypeName || "-"}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-muted-foreground">Wilayah BPS:</span>
									<span className="font-medium text-foreground">
										{selectedPin.locationLabel || "-"}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-muted-foreground">Jalur Pipa:</span>
									<span className="font-medium text-foreground">
										{getPosisiPelangganLabel(selectedPin.posisiPelanggan)}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-muted-foreground">Kawasan:</span>
									<span className="font-medium text-foreground">
										{getKawasanLabel(selectedPin.kawasan)}
									</span>
								</div>
								<div className="flex justify-between">
									<span className="text-muted-foreground">Sales PIC:</span>
									<span className="font-medium text-foreground">
										{selectedPin.salesUserName || "Belum Ditugaskan"}
									</span>
								</div>
								<div className="flex justify-between font-mono text-[11px]">
									<span className="text-muted-foreground">Koordinat:</span>
									<span>
										{Number(selectedPin.latitude).toFixed(5)},{" "}
										{Number(selectedPin.longitude).toFixed(5)}
									</span>
								</div>
							</div>

							<div className="pt-3">
								<Button asChild size="sm" className="w-full text-xs h-9">
									<Link
										to="/directory/$companyId"
										params={{ companyId: selectedPin.id }}
										className="flex items-center justify-center gap-1.5"
									>
										<span>Buka Company Record Hub</span>
										<ExternalLink className="size-3.5" />
									</Link>
								</Button>
							</div>
						</CardContent>
					</Card>
				)}
			</div>
		</div>
	);
}
