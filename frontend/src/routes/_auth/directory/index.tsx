import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	type ColumnDef,
	getCoreRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	Building2,
	ExternalLink,
	Filter,
	MapPin,
	Plus,
	RotateCcw,
	Search,
	SlidersHorizontal,
	X,
} from "lucide-react";
import * as React from "react";
import { z } from "zod";
import { $api } from "@/api/client";
import type { CompanyListItem } from "@/api/types";
import {
	DataTable,
	DataTableToolbar,
	IconButton,
	PageHeader,
} from "@/components/common";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import { Input } from "@/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import {
	Sheet,
	SheetContent,
	SheetDescription,
	SheetHeader,
	SheetTitle,
	SheetTrigger,
} from "@/components/ui/sheet";
import { useAuth } from "@/lib/auth";
import {
	getKawasanLabel,
	getPosisiPelangganLabel,
	getStageInfo,
	getStatusLabel,
} from "@/lib/directory-utils";

const directorySearchSchema = z.object({
	page: z.number().default(1).optional(),
	pageSize: z.number().default(25).optional(),
	stage: z.number().optional(),
	industryTypeId: z.string().optional(),
	searchTerm: z.string().optional(),
	posisiPelanggan: z.enum(["Pengembangan", "JalurExisting"]).optional(),
	kawasan: z.enum(["KawasanIndustri", "NonKawasanIndustri"]).optional(),
	provinceId: z.string().optional(),
	regencyId: z.string().optional(),
	districtId: z.string().optional(),
	villageId: z.string().optional(),
});

export const Route = createFileRoute("/_auth/directory/")({
	validateSearch: directorySearchSchema,
	component: CompanyDirectoryPage,
});

function CompanyDirectoryPage() {
	const search = Route.useSearch();
	const navigate = useNavigate({ from: Route.fullPath });
	const { hasCapability } = useAuth();

	const page = search.page || 1;
	const pageSize = search.pageSize || 25;
	const [searchInput, setSearchInput] = React.useState(search.searchTerm || "");
	const [isMobileFilterOpen, setIsMobileFilterOpen] = React.useState(false);

	// Fetch industry types for filter
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);

	// Cascading Geography Queries (4 Levels)
	const { data: provinces } = $api.useQuery("get", "/api/geography/provinces");
	const { data: regencies } = $api.useQuery(
		"get",
		"/api/geography/regencies",
		{
			params: {
				query: { provinceId: search.provinceId },
			},
		},
		{
			enabled: Boolean(search.provinceId),
		},
	);
	const { data: districts } = $api.useQuery(
		"get",
		"/api/geography/districts",
		{
			params: {
				query: { regencyId: search.regencyId },
			},
		},
		{
			enabled: Boolean(search.regencyId),
		},
	);
	const { data: villages } = $api.useQuery(
		"get",
		"/api/geography/villages",
		{
			params: {
				query: { districtId: search.districtId },
			},
		},
		{
			enabled: Boolean(search.districtId),
		},
	);

	// Fetch paged companies list with full 4-tier geography filters
	const { data, isLoading } = $api.useQuery("get", "/api/companies", {
		params: {
			query: {
				page,
				pageSize,
				stage: search.stage,
				industryTypeId: search.industryTypeId,
				searchTerm: search.searchTerm,
				posisiPelanggan: search.posisiPelanggan || undefined,
				kawasan: search.kawasan || undefined,
				provinceId: search.provinceId || undefined,
				regencyId: search.regencyId || undefined,
				districtId: search.districtId || undefined,
				villageId: search.villageId || undefined,
			},
		},
	});

	const items = React.useMemo(() => data?.items || [], [data?.items]);
	const totalCount = Number(data?.totalCount || 0);
	const totalPages = Math.ceil(totalCount / pageSize) || 1;

	const activeFilterCount = [
		search.provinceId,
		search.regencyId,
		search.districtId,
		search.villageId,
		search.industryTypeId,
		search.posisiPelanggan,
		search.kawasan,
	].filter(Boolean).length;

	const handleResetFilters = React.useCallback(() => {
		setSearchInput("");
		navigate({ search: {} });
	}, [navigate]);

	// Handle filter changes
	const updateFilters = React.useCallback(
		(newParams: Partial<typeof search>) => {
			navigate({
				search: (prev) => ({
					...prev,
					...newParams,
					page: newParams.page !== undefined ? newParams.page : 1,
				}),
			});
		},
		[navigate],
	);

	// Debounce search input
	React.useEffect(() => {
		const timer = setTimeout(() => {
			if (searchInput !== (search.searchTerm || "")) {
				updateFilters({ searchTerm: searchInput || undefined });
			}
		}, 400);
		return () => clearTimeout(timer);
	}, [searchInput, search.searchTerm, updateFilters]);

	const columns = React.useMemo<ColumnDef<CompanyListItem>[]>(
		() => [
			{
				accessorKey: "nomor",
				header: "Nomor Registrasi",
				meta: {
					headerClassName: "min-w-[130px]",
					cellClassName: "min-w-[130px]",
				},
				cell: ({ row }) => {
					const item = row.original;
					return (
						<Link
							to="/directory/$companyId"
							params={{ companyId: item.id }}
							className="font-mono text-xs font-semibold text-primary hover:underline flex items-center gap-1.5"
						>
							<Building2 className="size-3.5 text-muted-foreground" />
							{item.nomor}
						</Link>
					);
				},
			},
			{
				accessorKey: "namaPerusahaan",
				header: "Nama Perusahaan",
				meta: {
					headerClassName: "min-w-[230px]",
					cellClassName: "min-w-[230px]",
				},
				cell: ({ row }) => {
					const item = row.original;
					return (
						<div>
							<Link
								to="/directory/$companyId"
								params={{ companyId: item.id }}
								className="font-medium text-sm text-foreground hover:text-primary transition-colors block"
							>
								{item.namaPerusahaan}
							</Link>
							<span className="text-xs text-muted-foreground">
								{item.industryTypeName || "Sektor Industri"}
							</span>
						</div>
					);
				},
			},
			{
				accessorKey: "locationLabel",
				header: "Lokasi",
				meta: {
					headerClassName: "min-w-[140px]",
					cellClassName: "min-w-[140px]",
				},
				cell: ({ row }) => (
					<div className="flex items-center gap-1 text-xs text-muted-foreground">
						<MapPin className="size-3 shrink-0 text-muted-foreground" />
						<span>{row.original.locationLabel}</span>
					</div>
				),
			},
			{
				accessorKey: "currentStage",
				header: "Tahapan",
				meta: {
					headerClassName: "min-w-[120px]",
					cellClassName: "min-w-[120px]",
				},
				cell: ({ row }) => {
					const stageInfo = getStageInfo(row.original.currentStage);
					return (
						<Badge
							variant="outline"
							className={`text-[11px] font-medium border ${stageInfo.badgeClass}`}
						>
							{stageInfo.shortName}
						</Badge>
					);
				},
			},
			{
				accessorKey: "status",
				header: "Status",
				meta: {
					headerClassName: "min-w-[85px]",
					cellClassName: "min-w-[85px]",
				},
				cell: ({ row }) => {
					const status = getStatusLabel(row.original.status);
					return (
						<Badge
							variant="outline"
							className={`text-[11px] font-normal border ${status.badgeClass}`}
						>
							{status.label}
						</Badge>
					);
				},
			},
			{
				accessorKey: "salesUserName",
				header: "PIC Sales",
				meta: {
					headerClassName: "min-w-[100px]",
					cellClassName: "min-w-[100px]",
				},
				cell: ({ row }) => (
					<span className="text-xs text-muted-foreground">
						{row.original.salesUserName || "-"}
					</span>
				),
			},
			{
				accessorKey: "posisiPelanggan",
				header: "Jalur Pipa",
				meta: {
					headerClassName: "min-w-[100px]",
					cellClassName: "min-w-[100px]",
				},
				cell: ({ row }) => (
					<span className="text-xs">
						{getPosisiPelangganLabel(row.original.posisiPelanggan)}
					</span>
				),
			},
			{
				accessorKey: "kawasan",
				header: "Kawasan",
				meta: {
					headerClassName: "min-w-[100px]",
					cellClassName: "min-w-[100px]",
				},
				cell: ({ row }) => (
					<span className="text-xs">
						{getKawasanLabel(row.original.kawasan)}
					</span>
				),
			},
			{
				id: "actions",
				header: () => <div className="text-right pr-2">Aksi</div>,
				meta: {
					headerClassName: "min-w-[70px] text-right",
					cellClassName: "min-w-[70px]",
				},
				cell: ({ row }) => {
					const item = row.original;
					return (
						<div className="flex items-center justify-end pr-1">
							<IconButton tooltip="Lihat Detail" asChild>
								<Link
									to="/directory/$companyId"
									params={{ companyId: item.id }}
									aria-label="Lihat detail perusahaan"
								>
									<ExternalLink className="size-4" />
								</Link>
							</IconButton>
						</div>
					);
				},
			},
		],
		[],
	);

	const table = useReactTable({
		data: items,
		columns,
		getCoreRowModel: getCoreRowModel(),
		manualPagination: true,
		pageCount: totalPages,
	});

	const stagePills = [
		{ label: "Semua", value: undefined },
		{ label: "Tahap 1: Calon Pelanggan", value: 1 },
		{ label: "Tahap 2: Plotting", value: 2 },
		{ label: "Tahap 3: Prospek", value: 3 },
		{ label: "Tahap 4: Survei KK0", value: 4 },
		{ label: "Tahap 5: Registrasi A1", value: 5 },
		{ label: "Tahap 6: Permohonan NOL", value: 6 },
		{ label: "Tahap 7: Evaluasi NOL", value: 7 },
		{ label: "Tahap 8: NOL Terbit", value: 8 },
	];

	return (
		<div className="space-y-6">
			{/* Standard Page Header */}
			<PageHeader
				title="Direktori Industri"
				description="Manajemen calon pelanggan, plotting jaringan pipa gas, dan tracking dokumen berlangganan"
				actions={
					<>
						<Button variant="outline" asChild size="sm" className="h-9">
							<Link to="/map" className="flex items-center gap-1.5 text-xs">
								<MapPin className="size-3.5" /> Peta Spasial
							</Link>
						</Button>
						{hasCapability("CreateCompany") && (
							<Button asChild size="sm" className="h-9">
								<Link
									to="/directory/new"
									className="flex items-center gap-1.5 text-xs"
								>
									<Plus className="size-4" /> Tambah Calon Pelanggan
								</Link>
							</Button>
						)}
					</>
				}
			/>

			{/* Stage Filter Pills */}
			<div className="flex items-center gap-1.5 overflow-x-auto pb-1 scrollbar-none">
				{stagePills.map((pill) => {
					const isActive = search.stage === pill.value;
					return (
						<Button
							key={pill.label}
							variant={isActive ? "default" : "outline"}
							size="sm"
							onClick={() => updateFilters({ stage: pill.value })}
							className={`text-xs h-8 whitespace-nowrap rounded-full px-3.5 transition-all ${
								isActive ? "shadow-xs font-semibold" : "text-muted-foreground"
							}`}
						>
							{pill.label}
						</Button>
					);
				})}
			</div>

			{/* Search & Filter Toolbar */}
			<DataTableToolbar
				actions={
					<div className="flex items-center gap-3 text-xs text-muted-foreground">
						<div className="flex items-center gap-1.5">
							<Filter className="size-3.5 text-muted-foreground" />
							<span>
								<strong className="text-foreground font-semibold">
									{items.length}
								</strong>{" "}
								dari{" "}
								<strong className="text-foreground font-semibold">
									{totalCount}
								</strong>
							</span>
						</div>
						{(search.searchTerm ||
							search.stage ||
							search.industryTypeId ||
							search.posisiPelanggan ||
							search.kawasan ||
							search.provinceId ||
							search.regencyId ||
							search.districtId ||
							search.villageId) && (
							<Button
								variant="ghost"
								size="sm"
								onClick={handleResetFilters}
								className="h-8 px-2 text-xs text-muted-foreground hover:text-foreground flex items-center gap-1"
							>
								<RotateCcw className="size-3" /> Reset
							</Button>
						)}
					</div>
				}
			>
				<div className="flex items-center gap-2 flex-1 min-w-[200px] max-w-full sm:max-w-sm">
					<div className="relative flex-1">
						<Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
						<Input
							placeholder="Cari nama perusahaan atau nomor registrasi..."
							value={searchInput}
							onChange={(e) => setSearchInput(e.target.value)}
							className="pl-9 h-9 text-xs w-full"
						/>
					</div>

					{/* Mobile Filter Sheet Trigger */}
					<Sheet open={isMobileFilterOpen} onOpenChange={setIsMobileFilterOpen}>
						<SheetTrigger asChild>
							<Button
								variant="outline"
								size="sm"
								className="md:hidden h-9 text-xs flex items-center gap-1.5 shrink-0"
								aria-label="Filter Lengkap"
							>
								<SlidersHorizontal className="size-3.5 text-muted-foreground" />
								<span>Filter</span>
								{activeFilterCount > 0 && (
									<Badge
										variant="secondary"
										className="h-4.5 min-w-4 px-1 text-[10px] font-semibold ml-0.5"
									>
										{activeFilterCount}
									</Badge>
								)}
							</Button>
						</SheetTrigger>
						<SheetContent
							side="bottom"
							className="max-h-[85vh] overflow-y-auto rounded-t-2xl p-4 sm:p-6 space-y-4"
						>
							<SheetHeader className="p-0 text-left">
								<div className="flex items-center justify-between">
									<SheetTitle className="text-sm font-bold">
										Filter Direktori Pelanggan
									</SheetTitle>
									{activeFilterCount > 0 && (
										<Button
											variant="ghost"
											size="sm"
											onClick={() => {
												handleResetFilters();
												setIsMobileFilterOpen(false);
											}}
											className="h-7 px-2 text-xs text-muted-foreground hover:text-foreground"
										>
											Reset Semua
										</Button>
									)}
								</div>
								<SheetDescription className="text-xs text-muted-foreground">
									Filter hierarki wilayah 4 tingkat (Provinsi, Kota/Kab,
									Kecamatan, Kelurahan) dan klasifikasi operasional
								</SheetDescription>
							</SheetHeader>

							<div className="space-y-3 pt-2">
								<div className="space-y-1.5">
									<span className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider block">
										Wilayah Administratif
									</span>
									<div className="space-y-2">
										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Provinsi
											</span>
											<Combobox
												value={search.provinceId || ""}
												onValueChange={(val) =>
													updateFilters({
														provinceId: val ? val : undefined,
														regencyId: undefined,
														districtId: undefined,
														villageId: undefined,
													})
												}
												options={[
													{ value: "", label: "Semua Provinsi" },
													...(provinces?.map((p) => ({
														value: p.id,
														label: p.name,
													})) || []),
												]}
												placeholder="Pilih Provinsi"
												searchPlaceholder="Cari provinsi..."
												emptyText="Provinsi tidak ditemukan."
											/>
										</div>

										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Kota / Kabupaten
											</span>
											<Combobox
												value={search.regencyId || ""}
												disabled={!search.provinceId}
												onValueChange={(val) =>
													updateFilters({
														regencyId: val ? val : undefined,
														districtId: undefined,
														villageId: undefined,
													})
												}
												options={[
													{ value: "", label: "Semua Kota / Kab" },
													...(regencies?.map((r) => ({
														value: r.id,
														label: r.name,
													})) || []),
												]}
												placeholder={
													search.provinceId
														? "Pilih Kota / Kab"
														: "Pilih Provinsi Dulu"
												}
												searchPlaceholder="Cari kota / kabupaten..."
												emptyText="Kota / Kabupaten tidak ditemukan."
											/>
										</div>

										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Kecamatan
											</span>
											<Combobox
												value={search.districtId || ""}
												disabled={!search.regencyId}
												onValueChange={(val) =>
													updateFilters({
														districtId: val ? val : undefined,
														villageId: undefined,
													})
												}
												options={[
													{ value: "", label: "Semua Kecamatan" },
													...(districts?.map((d) => ({
														value: d.id,
														label: d.name,
													})) || []),
												]}
												placeholder={
													search.regencyId
														? "Pilih Kecamatan"
														: "Pilih Kota/Kab Dulu"
												}
												searchPlaceholder="Cari kecamatan..."
												emptyText="Kecamatan tidak ditemukan."
											/>
										</div>

										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Kelurahan / Desa
											</span>
											<Combobox
												value={search.villageId || ""}
												disabled={!search.districtId}
												onValueChange={(val) =>
													updateFilters({
														villageId: val ? val : undefined,
													})
												}
												options={[
													{ value: "", label: "Semua Kelurahan / Desa" },
													...(villages?.map((v) => ({
														value: v.id,
														label: v.name,
													})) || []),
												]}
												placeholder={
													search.districtId
														? "Pilih Kelurahan"
														: "Pilih Kecamatan Dulu"
												}
												searchPlaceholder="Cari kelurahan/desa..."
												emptyText="Kelurahan/Desa tidak ditemukan."
											/>
										</div>
									</div>
								</div>

								<div className="space-y-1.5 pt-2 border-t">
									<span className="text-[11px] font-semibold text-muted-foreground uppercase tracking-wider block">
										Klasifikasi Industri & Jalur
									</span>
									<div className="space-y-2">
										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Sektor Industri
											</span>
											<Combobox
												value={search.industryTypeId || ""}
												onValueChange={(val) =>
													updateFilters({
														industryTypeId: val ? val : undefined,
													})
												}
												options={[
													{ value: "", label: "Semua Sektor Industri" },
													...(industryTypes?.map((it) => ({
														value: it.id,
														label: it.name,
													})) || []),
												]}
												placeholder="Pilih Sektor Industri"
												searchPlaceholder="Cari sektor industri..."
												emptyText="Sektor industri tidak ditemukan."
											/>
										</div>

										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Jalur Pipa
											</span>
											<Select
												value={search.posisiPelanggan || "ALL"}
												onValueChange={(val) =>
													updateFilters({
														posisiPelanggan:
															val === "ALL"
																? undefined
																: (val as "Pengembangan" | "JalurExisting"),
													})
												}
											>
												<SelectTrigger className="h-9 text-xs w-full">
													<SelectValue placeholder="Jalur Pipa" />
												</SelectTrigger>
												<SelectContent>
													<SelectItem value="ALL">Semua Jalur Pipa</SelectItem>
													<SelectItem value="JalurExisting">
														Jalur Existing
													</SelectItem>
													<SelectItem value="Pengembangan">
														Pengembangan
													</SelectItem>
												</SelectContent>
											</Select>
										</div>

										<div>
											<span className="text-xs text-muted-foreground mb-1 block font-medium">
												Kawasan
											</span>
											<Select
												value={search.kawasan || "ALL"}
												onValueChange={(val) =>
													updateFilters({
														kawasan:
															val === "ALL"
																? undefined
																: (val as
																		| "KawasanIndustri"
																		| "NonKawasanIndustri"),
													})
												}
											>
												<SelectTrigger className="h-9 text-xs w-full">
													<SelectValue placeholder="Kawasan" />
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
									</div>
								</div>

								<Button
									className="w-full mt-4 h-9 text-xs"
									onClick={() => setIsMobileFilterOpen(false)}
								>
									Tutup & Terapkan Filter
								</Button>
							</div>
						</SheetContent>
					</Sheet>
				</div>

				{/* Desktop & Tablet Inline Cascading 4-Level Geography Filters */}
				<div className="hidden md:flex flex-wrap items-center gap-2">
					<div className="w-[155px]">
						<Combobox
							value={search.provinceId || ""}
							onValueChange={(val) =>
								updateFilters({
									provinceId: val ? val : undefined,
									regencyId: undefined,
									districtId: undefined,
									villageId: undefined,
								})
							}
							options={[
								{ value: "", label: "Semua Provinsi" },
								...(provinces?.map((p) => ({
									value: p.id,
									label: p.name,
								})) || []),
							]}
							placeholder="Pilih Provinsi"
							searchPlaceholder="Cari provinsi..."
							emptyText="Provinsi tidak ditemukan."
							aria-label="Filter Provinsi"
						/>
					</div>

					<div className="w-[155px]">
						<Combobox
							value={search.regencyId || ""}
							disabled={!search.provinceId}
							onValueChange={(val) =>
								updateFilters({
									regencyId: val ? val : undefined,
									districtId: undefined,
									villageId: undefined,
								})
							}
							options={[
								{ value: "", label: "Semua Kota / Kab" },
								...(regencies?.map((r) => ({
									value: r.id,
									label: r.name,
								})) || []),
							]}
							placeholder={
								search.provinceId ? "Pilih Kota / Kab" : "Pilih Provinsi Dulu"
							}
							searchPlaceholder="Cari kota / kab..."
							emptyText="Kota / Kab tidak ditemukan."
							aria-label="Filter Kota / Kabupaten"
						/>
					</div>

					<div className="w-[155px]">
						<Combobox
							value={search.districtId || ""}
							disabled={!search.regencyId}
							onValueChange={(val) =>
								updateFilters({
									districtId: val ? val : undefined,
									villageId: undefined,
								})
							}
							options={[
								{ value: "", label: "Semua Kecamatan" },
								...(districts?.map((d) => ({
									value: d.id,
									label: d.name,
								})) || []),
							]}
							placeholder={
								search.regencyId ? "Pilih Kecamatan" : "Pilih Kota/Kab Dulu"
							}
							searchPlaceholder="Cari kecamatan..."
							emptyText="Kecamatan tidak ditemukan."
							aria-label="Filter Kecamatan"
						/>
					</div>

					<div className="w-[155px]">
						<Combobox
							value={search.villageId || ""}
							disabled={!search.districtId}
							onValueChange={(val) =>
								updateFilters({
									villageId: val ? val : undefined,
								})
							}
							options={[
								{ value: "", label: "Semua Kelurahan / Desa" },
								...(villages?.map((v) => ({
									value: v.id,
									label: v.name,
								})) || []),
							]}
							placeholder={
								search.districtId ? "Pilih Kelurahan" : "Pilih Kecamatan Dulu"
							}
							searchPlaceholder="Cari kelurahan/desa..."
							emptyText="Kelurahan/Desa tidak ditemukan."
							aria-label="Filter Kelurahan / Desa"
						/>
					</div>

					<div className="w-[155px]">
						<Combobox
							value={search.industryTypeId || ""}
							onValueChange={(val) =>
								updateFilters({
									industryTypeId: val ? val : undefined,
								})
							}
							options={[
								{ value: "", label: "Semua Sektor Industri" },
								...(industryTypes?.map((it) => ({
									value: it.id,
									label: it.name,
								})) || []),
							]}
							placeholder="Sektor Industri"
							searchPlaceholder="Cari sektor..."
							emptyText="Sektor tidak ditemukan."
							aria-label="Filter Sektor Industri"
						/>
					</div>

					<div className="w-[140px]">
						<Select
							value={search.posisiPelanggan || "ALL"}
							onValueChange={(val) =>
								updateFilters({
									posisiPelanggan:
										val === "ALL"
											? undefined
											: (val as "Pengembangan" | "JalurExisting"),
								})
							}
						>
							<SelectTrigger className="h-9 text-xs w-full">
								<SelectValue placeholder="Jalur Pipa" />
							</SelectTrigger>
							<SelectContent>
								<SelectItem value="ALL">Semua Jalur</SelectItem>
								<SelectItem value="JalurExisting">Jalur Existing</SelectItem>
								<SelectItem value="Pengembangan">Pengembangan</SelectItem>
							</SelectContent>
						</Select>
					</div>
				</div>
			</DataTableToolbar>

			{/* Mobile Active Filter Chips */}
			{activeFilterCount > 0 && (
				<div className="md:hidden flex items-center gap-1.5 overflow-x-auto pb-1 text-xs">
					<span className="text-[11px] text-muted-foreground shrink-0 font-medium">
						Filter Aktif:
					</span>
					{search.provinceId && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>
								{provinces?.find((p) => p.id === search.provinceId)?.name ||
									"Provinsi"}
							</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() =>
									updateFilters({
										provinceId: undefined,
										regencyId: undefined,
										districtId: undefined,
										villageId: undefined,
									})
								}
							/>
						</Badge>
					)}
					{search.regencyId && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>
								{regencies?.find((r) => r.id === search.regencyId)?.name ||
									"Kota/Kab"}
							</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() =>
									updateFilters({
										regencyId: undefined,
										districtId: undefined,
										villageId: undefined,
									})
								}
							/>
						</Badge>
					)}
					{search.districtId && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>
								{districts?.find((d) => d.id === search.districtId)?.name ||
									"Kecamatan"}
							</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() =>
									updateFilters({
										districtId: undefined,
										villageId: undefined,
									})
								}
							/>
						</Badge>
					)}
					{search.villageId && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>
								{villages?.find((v) => v.id === search.villageId)?.name ||
									"Kelurahan"}
							</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() => updateFilters({ villageId: undefined })}
							/>
						</Badge>
					)}
					{search.industryTypeId && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>
								{industryTypes?.find((it) => it.id === search.industryTypeId)
									?.name || "Industri"}
							</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() => updateFilters({ industryTypeId: undefined })}
							/>
						</Badge>
					)}
					{search.posisiPelanggan && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>{getPosisiPelangganLabel(search.posisiPelanggan)}</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() => updateFilters({ posisiPelanggan: undefined })}
							/>
						</Badge>
					)}
					{search.kawasan && (
						<Badge
							variant="secondary"
							className="h-6 gap-1 px-2 text-[11px] shrink-0 font-normal"
						>
							<span>{getKawasanLabel(search.kawasan)}</span>
							<X
								className="size-3 cursor-pointer hover:text-foreground"
								onClick={() => updateFilters({ kawasan: undefined })}
							/>
						</Badge>
					)}
				</div>
			)}

			{/* Clean Data Table */}
			<DataTable
				table={table}
				columnsCount={columns.length}
				isLoading={isLoading}
				skeletonRows={pageSize > 10 ? 10 : pageSize}
				emptyTitle="Tidak Ada Data Perusahaan"
				emptyDescription="Tidak ada data perusahaan yang sesuai dengan kriteria filter Anda."
				onResetFilters={handleResetFilters}
				resetLabel="Reset Filter"
				pagination={{
					page,
					pageSize,
					totalCount,
					totalPages,
					onPageChange: (newPage) => updateFilters({ page: newPage }),
					onPageSizeChange: (newPageSize) =>
						updateFilters({ pageSize: newPageSize, page: 1 }),
					pageSizeOptions: [10, 25, 50, 100],
				}}
			/>
		</div>
	);
}
