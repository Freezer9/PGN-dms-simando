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
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Combobox } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Map,
	MapControls,
	MapMarker,
	MarkerContent,
	MarkerPopup,
	useMap,
} from "@/components/ui/map";
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

function MapViewController({
	center,
	zoom,
}: {
	center: [number, number];
	zoom: number;
}) {
	const { map } = useMap();
	React.useEffect(() => {
		if (!map) return;
		map.flyTo({
			center,
			zoom,
			duration: 800,
		});
	}, [map, center, zoom]);
	return null;
}

function GeospatialMapPage() {
	// Filter states
	const [searchTerm, setSearchTerm] = React.useState("");
	const [selectedProvince, setSelectedProvince] = React.useState<string>("ALL");
	const [selectedRegency, setSelectedRegency] = React.useState<string>("ALL");
	const [selectedDistrict, setSelectedDistrict] = React.useState<string>("ALL");
	const [selectedVillage, setSelectedVillage] = React.useState<string>("ALL");
	const [selectedStages, setSelectedStages] = React.useState<number[]>([]);
	const [selectedIndustry, setSelectedIndustry] = React.useState<string>("ALL");
	const [selectedPosisi, setSelectedPosisi] = React.useState<string>("ALL");
	const [selectedKawasan, setSelectedKawasan] = React.useState<string>("ALL");

	// Active Selected Pin for detail drawer
	const [selectedPin, setSelectedPin] = React.useState<CompanyMapPinDto | null>(
		null,
	);
	const [isFilterOpen, setIsFilterOpen] = React.useState(false);

	// Responsive initial state: open filter panel on desktop (>= 768px), keep closed on mobile
	React.useEffect(() => {
		if (typeof window !== "undefined" && window.innerWidth >= 768) {
			setIsFilterOpen(true);
		}
	}, []);

	// Fetch Pins with 4-level cascading geography filters
	const { data: pinsData, isLoading } = $api.useQuery(
		"get",
		"/api/companies/map-pins",
		{
			params: {
				query: {
					provinceId: selectedProvince !== "ALL" ? selectedProvince : undefined,
					regencyId: selectedRegency !== "ALL" ? selectedRegency : undefined,
					districtId: selectedDistrict !== "ALL" ? selectedDistrict : undefined,
					villageId: selectedVillage !== "ALL" ? selectedVillage : undefined,
				},
			},
		},
	);

	// Cascading Geography Queries (4 Levels)
	const { data: provinces } = $api.useQuery("get", "/api/geography/provinces");
	const { data: regencies } = $api.useQuery(
		"get",
		"/api/geography/regencies",
		{
			params: {
				query: {
					provinceId: selectedProvince === "ALL" ? "" : selectedProvince,
				},
			},
		},
		{
			enabled: selectedProvince !== "ALL" && Boolean(selectedProvince),
		},
	);
	const { data: districts } = $api.useQuery(
		"get",
		"/api/geography/districts",
		{
			params: {
				query: {
					regencyId: selectedRegency === "ALL" ? "" : selectedRegency,
				},
			},
		},
		{
			enabled: selectedRegency !== "ALL" && Boolean(selectedRegency),
		},
	);
	const { data: villages } = $api.useQuery(
		"get",
		"/api/geography/villages",
		{
			params: {
				query: {
					districtId: selectedDistrict === "ALL" ? "" : selectedDistrict,
				},
			},
		},
		{
			enabled: selectedDistrict !== "ALL" && Boolean(selectedDistrict),
		},
	);

	// Fetch Master Data
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);

	// Filter Pins in memory
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

			// Province filter
			if (selectedProvince !== "ALL") {
				if (item.provinceId && item.provinceId !== selectedProvince) {
					return false;
				}
			}

			// Regency filter
			if (selectedRegency !== "ALL") {
				if (item.regencyId && item.regencyId !== selectedRegency) return false;
			}

			// District filter (Kecamatan)
			if (selectedDistrict !== "ALL") {
				if (item.districtId && item.districtId !== selectedDistrict) {
					return false;
				}
			}

			// Village filter (Kelurahan / Desa)
			if (selectedVillage !== "ALL") {
				if (item.villageId && item.villageId !== selectedVillage) return false;
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
		selectedProvince,
		selectedRegency,
		selectedDistrict,
		selectedVillage,
		selectedStages,
		selectedIndustry,
		selectedPosisi,
		selectedKawasan,
	]);

	const toggleStage = (stage: number) => {
		setSelectedStages((prev) =>
			prev.includes(stage) ? prev.filter((s) => s !== stage) : [...prev, stage],
		);
	};

	const resetFilters = () => {
		setSearchTerm("");
		setSelectedProvince("ALL");
		setSelectedRegency("ALL");
		setSelectedDistrict("ALL");
		setSelectedVillage("ALL");
		setSelectedStages([]);
		setSelectedIndustry("ALL");
		setSelectedPosisi("ALL");
		setSelectedKawasan("ALL");
	};

	const activeFilterCount = [
		selectedProvince !== "ALL",
		selectedRegency !== "ALL",
		selectedDistrict !== "ALL",
		selectedVillage !== "ALL",
		selectedIndustry !== "ALL",
		selectedPosisi !== "ALL",
		selectedKawasan !== "ALL",
		selectedStages.length > 0,
		Boolean(searchTerm),
	].filter(Boolean).length;

	const mapCenter: [number, number] = React.useMemo(() => {
		if (filteredPins.length > 0) {
			const first = filteredPins[0];
			if (
				typeof first.longitude === "number" &&
				typeof first.latitude === "number"
			) {
				return [first.longitude, first.latitude];
			}
		}
		return [106.8456, -6.2088]; // Jakarta center default
	}, [filteredPins]);

	const mapZoom = React.useMemo(() => {
		if (selectedVillage !== "ALL") return 15;
		if (selectedDistrict !== "ALL") return 13;
		if (selectedRegency !== "ALL") return 11;
		if (selectedProvince !== "ALL") return 8;
		return 10;
	}, [selectedProvince, selectedRegency, selectedDistrict, selectedVillage]);

	return (
		<div className="relative h-[calc(100vh-8rem)] w-full flex flex-col overflow-hidden rounded-lg border shadow-xs">
			{/* Top Header Bar */}
			<div className="h-12 bg-background border-b px-3 sm:px-4 flex items-center justify-between shrink-0 z-10 gap-2">
				<div className="flex items-center gap-2 sm:gap-3 min-w-0">
					<div className="flex items-center gap-1.5 font-semibold text-xs sm:text-sm truncate">
						<MapPinIcon className="size-4 text-primary shrink-0" />
						<span className="hidden sm:inline truncate">
							Peta Sebaran Pelanggan & Jaringan Pipa
						</span>
						<span className="sm:hidden font-medium truncate">Peta Spasial</span>
					</div>
					<Badge
						variant="outline"
						className="text-[10px] sm:text-xs font-normal shrink-0"
					>
						{filteredPins.length} dari {pinsData?.length || 0} Titik
					</Badge>
				</div>

				<div className="flex items-center gap-1.5 sm:gap-2 shrink-0">
					<Button
						variant={isFilterOpen ? "secondary" : "outline"}
						size="sm"
						onClick={() => setIsFilterOpen(!isFilterOpen)}
						className="h-8 text-xs flex items-center gap-1 px-2.5"
						aria-label="Toggle Filter Spasial"
					>
						<Filter className="size-3.5" />
						<span className="hidden sm:inline">Filter Spasial</span>
						{activeFilterCount > 0 && (
							<Badge
								variant="secondary"
								className="h-4.5 min-w-4 px-1 text-[10px] font-semibold"
							>
								{activeFilterCount}
							</Badge>
						)}
					</Button>
					<Button
						variant="default"
						size="sm"
						asChild
						className="h-8 text-xs px-2.5"
					>
						<Link to="/directory" className="flex items-center gap-1">
							<Building2 className="size-3.5" />
							<span className="hidden sm:inline">Tabel Direktori</span>
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
					center={mapCenter}
					zoom={10}
					className="h-full w-full rounded-none border-0"
				>
					<MapControls />
					<MapViewController center={mapCenter} zoom={mapZoom} />
					{filteredPins.map((item) => {
						const stageNum = Number(item.currentStage);
						const stageColor = STAGE_PIN_COLORS[stageNum] || "#3b82f6";
						const stage = getStageInfo(stageNum);

						const fullLocation =
							[
								item.villageName ? `Kel. ${item.villageName}` : null,
								item.districtName ? `Kec. ${item.districtName}` : null,
								item.regencyName,
								item.provinceName,
							]
								.filter(Boolean)
								.join(", ") ||
							item.locationLabel ||
							"-";

						return (
							<MapMarker
								key={item.id}
								longitude={Number(item.longitude)}
								latitude={Number(item.latitude)}
								onClick={() => setSelectedPin(item)}
							>
								<MarkerContent>
									<div
										className="size-5 rounded-full border-2 border-white shadow-md flex items-center justify-center cursor-pointer transition-transform hover:scale-125"
										style={{ backgroundColor: stageColor }}
										title={`${item.namaPerusahaan} (${stage.shortName})`}
									>
										<div className="size-1.5 rounded-full bg-white" />
									</div>
								</MarkerContent>
								<MarkerPopup>
									<div className="space-y-1.5 text-xs min-w-[220px]">
										<div className="font-semibold text-foreground">
											{item.namaPerusahaan}
										</div>
										<div className="text-[11px] text-muted-foreground font-mono">
											{item.nomor}
										</div>
										<div className="flex items-center gap-1.5 text-[11px] text-muted-foreground">
											<span
												className="size-2 rounded-full shrink-0"
												style={{ backgroundColor: stageColor }}
											/>
											<span>{stage.name}</span>
										</div>
										<div className="text-[11px] text-muted-foreground flex items-center gap-1 truncate pt-0.5">
											<MapPinIcon className="size-3 shrink-0 text-muted-foreground" />
											<span className="truncate">{fullLocation}</span>
										</div>
										<div className="pt-1 border-t">
											<Link
												to="/directory/$companyId"
												params={{ companyId: item.id }}
												className="text-primary hover:underline font-medium text-[11px] flex items-center gap-1"
											>
												<span>Buka Detail Perusahaan</span>
												<ChevronRight className="size-3" />
											</Link>
										</div>
									</div>
								</MarkerPopup>
							</MapMarker>
						);
					})}
				</Map>

				{/* Floating / Responsive Filter Overlay Panel */}
				{isFilterOpen && (
					<Card className="absolute left-3 right-3 bottom-3 max-h-[75vh] md:left-4 md:right-auto md:top-4 md:bottom-auto md:w-80 md:max-h-[calc(100%-2rem)] overflow-y-auto z-20 shadow-xl border-border/80 bg-background/95 backdrop-blur-xs rounded-xl md:rounded-lg">
						<CardHeader className="p-3 pb-2 flex flex-row items-center justify-between">
							<CardTitle className="text-xs font-bold uppercase tracking-wider text-muted-foreground">
								Filter Peta
							</CardTitle>
							<Button
								variant="ghost"
								size="icon"
								className="size-6"
								onClick={() => setIsFilterOpen(false)}
								aria-label="Tutup filter peta"
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

							{/* Wilayah Administratif 4 Tingkat */}
							<div className="space-y-2 pt-1 border-t">
								<Label className="text-[11px] font-semibold text-muted-foreground block">
									Wilayah Administratif
								</Label>
								<div className="space-y-1">
									<Label className="text-[10px] text-muted-foreground">
										Provinsi
									</Label>
									<Combobox
										value={selectedProvince === "ALL" ? "" : selectedProvince}
										onValueChange={(val) => {
											setSelectedProvince(val || "ALL");
											setSelectedRegency("ALL");
											setSelectedDistrict("ALL");
											setSelectedVillage("ALL");
										}}
										options={[
											{ value: "", label: "Semua Provinsi" },
											...(provinces?.map((p) => ({
												value: p.id,
												label: p.name,
											})) || []),
										]}
										placeholder="Semua Provinsi"
										searchPlaceholder="Cari provinsi..."
										emptyText="Provinsi tidak ditemukan."
										aria-label="Filter Provinsi"
										className="h-8 text-xs"
									/>
								</div>
								<div className="space-y-1">
									<Label className="text-[10px] text-muted-foreground">
										Kota / Kabupaten
									</Label>
									<Combobox
										value={selectedRegency === "ALL" ? "" : selectedRegency}
										disabled={selectedProvince === "ALL" || !selectedProvince}
										onValueChange={(val) => {
											setSelectedRegency(val || "ALL");
											setSelectedDistrict("ALL");
											setSelectedVillage("ALL");
										}}
										options={[
											{ value: "", label: "Semua Kota / Kab" },
											...(regencies?.map((r) => ({
												value: r.id,
												label: r.name,
											})) || []),
										]}
										placeholder={
											selectedProvince !== "ALL"
												? "Semua Kota / Kab"
												: "Pilih Provinsi Dulu"
										}
										searchPlaceholder="Cari kota / kab..."
										emptyText="Kota / Kabupaten tidak ditemukan."
										aria-label="Filter Kota / Kabupaten"
										className="h-8 text-xs"
									/>
								</div>
								<div className="space-y-1">
									<Label className="text-[10px] text-muted-foreground">
										Kecamatan
									</Label>
									<Combobox
										value={selectedDistrict === "ALL" ? "" : selectedDistrict}
										disabled={selectedRegency === "ALL" || !selectedRegency}
										onValueChange={(val) => {
											setSelectedDistrict(val || "ALL");
											setSelectedVillage("ALL");
										}}
										options={[
											{ value: "", label: "Semua Kecamatan" },
											...(districts?.map((d) => ({
												value: d.id,
												label: d.name,
											})) || []),
										]}
										placeholder={
											selectedRegency !== "ALL"
												? "Semua Kecamatan"
												: "Pilih Kota/Kab Dulu"
										}
										searchPlaceholder="Cari kecamatan..."
										emptyText="Kecamatan tidak ditemukan."
										aria-label="Filter Kecamatan"
										className="h-8 text-xs"
									/>
								</div>
								<div className="space-y-1">
									<Label className="text-[10px] text-muted-foreground">
										Kelurahan / Desa
									</Label>
									<Combobox
										value={selectedVillage === "ALL" ? "" : selectedVillage}
										disabled={selectedDistrict === "ALL" || !selectedDistrict}
										onValueChange={(val) => setSelectedVillage(val || "ALL")}
										options={[
											{ value: "", label: "Semua Kelurahan / Desa" },
											...(villages?.map((v) => ({
												value: v.id,
												label: v.name,
											})) || []),
										]}
										placeholder={
											selectedDistrict !== "ALL"
												? "Semua Kelurahan"
												: "Pilih Kecamatan Dulu"
										}
										searchPlaceholder="Cari kelurahan..."
										emptyText="Kelurahan tidak ditemukan."
										aria-label="Filter Kelurahan / Desa"
										className="h-8 text-xs"
									/>
								</div>
							</div>

							{/* Sektor Industri */}
							<div className="space-y-1">
								<Label className="text-[11px] font-medium text-muted-foreground">
									Sektor Industri
								</Label>
								<Combobox
									value={selectedIndustry === "ALL" ? "" : selectedIndustry}
									onValueChange={(val) => setSelectedIndustry(val || "ALL")}
									options={[
										{ value: "", label: "Semua Sektor Industri" },
										...(industryTypes?.map((it) => ({
											value: it.name,
											label: it.name,
										})) || []),
									]}
									placeholder="Semua Sektor"
									searchPlaceholder="Cari sektor..."
									emptyText="Sektor tidak ditemukan."
									aria-label="Sektor Industri"
									className="h-8 text-xs"
								/>
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
							<div className="space-y-1.5 pt-2.5 border-t">
								<Label className="text-[11px] font-semibold text-muted-foreground block">
									Filter Berdasarkan Tahapan
								</Label>
								<div className="space-y-1.5">
									{[1, 2, 3, 4, 5, 6, 7, 8].map((s) => {
										const info = STAGE_CONFIG[s];
										const isChecked = selectedStages.includes(s);
										return (
											<label
												key={s}
												htmlFor={`stage-filter-${s}`}
												className="flex items-center gap-2 py-0.5 cursor-pointer hover:bg-muted/40 px-1 rounded transition-colors"
											>
												<Checkbox
													id={`stage-filter-${s}`}
													checked={isChecked}
													onCheckedChange={() => toggleStage(s)}
													className="size-3.5"
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
							<div className="space-y-1 pt-2.5 border-t">
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
								className="w-full h-8 text-xs flex items-center justify-center gap-1.5 text-muted-foreground hover:text-foreground"
							>
								<RotateCcw className="size-3" />
								<span>Reset Filter</span>
							</Button>
						</CardContent>
					</Card>
				)}

				{/* Floating Results Quick List (Bottom Right) - Responsive on Desktop */}
				<div className="hidden md:flex absolute bottom-4 right-4 sm:w-80 max-h-48 overflow-y-auto z-10 flex-col gap-1.5 p-1 bg-background/90 backdrop-blur-xs rounded-lg border shadow-md">
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
									<span>{p.regencyName || stage.shortName}</span>
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

				{/* Floating / Responsive Detail Drawer for Selected Pin */}
				{selectedPin && (
					<Card className="absolute left-3 right-3 bottom-3 max-h-[75vh] md:left-auto md:right-4 md:top-4 md:bottom-auto md:w-88 md:max-h-[calc(100%-2rem)] overflow-y-auto z-30 shadow-2xl border-border/80 bg-background/95 backdrop-blur-xs rounded-xl md:rounded-lg">
						<CardHeader className="p-4 pb-2 flex flex-row items-center justify-between">
							<div className="min-w-0 pr-2">
								<Badge variant="outline" className="font-mono text-xs mb-1">
									{selectedPin.nomor}
								</Badge>
								<CardTitle className="text-sm font-bold text-foreground truncate">
									{selectedPin.namaPerusahaan}
								</CardTitle>
							</div>
							<Button
								variant="ghost"
								size="icon"
								className="size-6 shrink-0"
								onClick={() => setSelectedPin(null)}
								aria-label="Tutup detail titik"
							>
								<X className="size-4" />
							</Button>
						</CardHeader>
						<CardContent className="p-4 pt-1 space-y-3 text-xs">
							<div className="flex items-center gap-2 flex-wrap">
								<Badge
									variant="outline"
									className={`text-[10px] font-normal border ${getStatusLabel(selectedPin.status).badgeClass}`}
								>
									{getStatusLabel(selectedPin.status).label}
								</Badge>
								<Badge variant="secondary" className="text-[10px]">
									Tahap {selectedPin.currentStage}:{" "}
									{getStageInfo(selectedPin.currentStage).shortName}
								</Badge>
							</div>

							<div className="space-y-1 border-t pt-2">
								<div className="text-muted-foreground text-[11px]">
									Lokasi Administratif (4 Tingkat)
								</div>
								<div className="font-medium text-foreground text-xs leading-relaxed">
									{[
										selectedPin.villageName
											? `Kel. ${selectedPin.villageName}`
											: null,
										selectedPin.districtName
											? `Kec. ${selectedPin.districtName}`
											: null,
										selectedPin.regencyName,
										selectedPin.provinceName,
									]
										.filter(Boolean)
										.join(", ") ||
										selectedPin.locationLabel ||
										"-"}
								</div>
							</div>

							<div className="grid grid-cols-2 gap-2 border-t pt-2">
								<div>
									<div className="text-muted-foreground text-[11px]">
										Sektor Industri
									</div>
									<div className="font-medium">
										{selectedPin.industryTypeName || "-"}
									</div>
								</div>
								<div>
									<div className="text-muted-foreground text-[11px]">
										PIC Sales
									</div>
									<div className="font-medium">
										{selectedPin.salesUserName || "Belum Ditugaskan"}
									</div>
								</div>
							</div>

							<div className="grid grid-cols-2 gap-2 border-t pt-2">
								<div>
									<div className="text-muted-foreground text-[11px]">
										Posisi Jalur
									</div>
									<div className="font-medium">
										{getPosisiPelangganLabel(selectedPin.posisiPelanggan)}
									</div>
								</div>
								<div>
									<div className="text-muted-foreground text-[11px]">
										Kawasan
									</div>
									<div className="font-medium">
										{getKawasanLabel(selectedPin.kawasan)}
									</div>
								</div>
							</div>

							<div className="space-y-1 border-t pt-2">
								<div className="text-muted-foreground text-[11px]">
									Koordinat Presisi
								</div>
								<div className="font-mono text-[11px]">
									Lat: {Number(selectedPin.latitude).toFixed(6)}, Lng:{" "}
									{Number(selectedPin.longitude).toFixed(6)}
								</div>
							</div>

							<div className="pt-2 border-t">
								<Button
									asChild
									className="w-full h-8 text-xs flex items-center gap-1.5"
								>
									<Link
										to="/directory/$companyId"
										params={{ companyId: selectedPin.id }}
									>
										<span>Buka Berkas Lengkap</span>
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
