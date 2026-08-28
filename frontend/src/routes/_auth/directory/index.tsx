import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	type ColumnDef,
	flexRender,
	getCoreRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	Building2,
	ChevronLeft,
	ChevronRight,
	ExternalLink,
	Filter,
	MapPin,
	Plus,
	RotateCcw,
	Search,
} from "lucide-react";
import * as React from "react";
import { z } from "zod";
import { $api } from "@/api/client";
import type { CompanyListItem, PosisiPelanggan } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
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

	// Fetch industry types for filter
	const { data: industryTypes } = $api.useQuery(
		"get",
		"/api/master/industry-types",
	);

	// Fetch paged companies list
	const { data, isLoading, isPlaceholderData } = $api.useQuery(
		"get",
		"/api/companies",
		{
			params: {
				query: {
					page,
					pageSize,
					stage: search.stage,
					industryTypeId: search.industryTypeId,
					searchTerm: search.searchTerm,
					posisiPelanggan: search.posisiPelanggan,
					kawasan: search.kawasan,
				},
			},
		},
	);

	const items = React.useMemo(() => data?.items || [], [data?.items]);
	const totalCount = Number(data?.totalCount || 0);
	const totalPages = Math.ceil(totalCount / pageSize) || 1;

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
				header: "Sales PIC",
				cell: ({ row }) => (
					<span className="text-xs text-muted-foreground">
						{row.original.salesUserName || "-"}
					</span>
				),
			},
			{
				accessorKey: "posisiPelanggan",
				header: "Jalur Pipa",
				cell: ({ row }) => (
					<span className="text-xs">
						{getPosisiPelangganLabel(row.original.posisiPelanggan)}
					</span>
				),
			},
			{
				accessorKey: "kawasan",
				header: "Kawasan",
				cell: ({ row }) => (
					<span className="text-xs">
						{getKawasanLabel(row.original.kawasan)}
					</span>
				),
			},
			{
				id: "actions",
				header: "Aksi",
				cell: ({ row }) => {
					const item = row.original;
					return (
						<div className="flex items-center justify-end gap-1">
							<Button variant="ghost" size="sm" asChild className="h-8 px-2.5">
								<Link
									to="/directory/$companyId"
									params={{ companyId: item.id }}
									className="text-xs flex items-center gap-1"
								>
									<span>Detail</span>
									<ExternalLink className="size-3 text-muted-foreground" />
								</Link>
							</Button>
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
			{/* Page Header */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
				<div>
					<h1 className="text-2xl font-bold tracking-tight text-foreground">
						Direktori Industri
					</h1>
					<p className="text-sm text-muted-foreground">
						Manajemen calon pelanggan, plotting jaringan pipa gas, dan tracking
						dokumen berlangganan
					</p>
				</div>
				<div className="flex items-center gap-2">
					<Button variant="outline" asChild size="sm" className="h-9">
						<Link to="/map" className="flex items-center gap-1.5 text-xs">
							<MapPin className="size-3.5" /> Peta Spatial
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
				</div>
			</div>

			{/* Stage Filters Bar */}
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
								isActive ? "shadow-sm font-semibold" : "text-muted-foreground"
							}`}
						>
							{pill.label}
						</Button>
					);
				})}
			</div>

			{/* Filter & Search Bar */}
			<Card className="border-border/60 shadow-xs">
				<CardContent className="p-4 space-y-3">
					<div className="grid grid-cols-1 md:grid-cols-4 gap-3">
						{/* Search Input */}
						<div className="relative md:col-span-2">
							<Search className="absolute left-3 top-2.5 size-4 text-muted-foreground" />
							<Input
								placeholder="Cari nama perusahaan atau nomor registrasi..."
								value={searchInput}
								onChange={(e) => setSearchInput(e.target.value)}
								className="pl-9 h-9 text-xs"
							/>
						</div>

						{/* Industry Type Filter */}
						<div>
							<Select
								value={search.industryTypeId || "ALL"}
								onValueChange={(val) =>
									updateFilters({
										industryTypeId: val === "ALL" ? undefined : val,
									})
								}
							>
								<SelectTrigger className="h-9 text-xs w-full">
									<SelectValue placeholder="Sektor Industri" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="ALL">Semua Sektor Industri</SelectItem>
									{industryTypes?.map((it) => (
										<SelectItem key={it.id} value={it.id}>
											{it.name}
										</SelectItem>
									))}
								</SelectContent>
							</Select>
						</div>

						{/* Posisi Pelanggan Filter */}
						<div>
							<Select
								value={search.posisiPelanggan || "ALL"}
								onValueChange={(val) =>
									updateFilters({
										posisiPelanggan:
											val === "ALL" ? undefined : (val as PosisiPelanggan),
									})
								}
							>
								<SelectTrigger className="h-9 text-xs w-full">
									<SelectValue placeholder="Jalur Pipa" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="ALL">Semua Jalur Pipa</SelectItem>
									<SelectItem value="JalurExisting">Jalur Existing</SelectItem>
									<SelectItem value="Pengembangan">Pengembangan</SelectItem>
								</SelectContent>
							</Select>
						</div>
					</div>

					{/* Active Filter Indicators & Reset */}
					<div className="flex items-center justify-between pt-1 border-t text-xs text-muted-foreground">
						<div className="flex items-center gap-2">
							<Filter className="size-3.5" />
							<span>
								Menampilkan{" "}
								<strong className="text-foreground">{items.length}</strong> dari{" "}
								<strong className="text-foreground">{totalCount}</strong>{" "}
								perusahaan
							</span>
						</div>
						{(search.searchTerm ||
							search.stage ||
							search.industryTypeId ||
							search.posisiPelanggan ||
							search.kawasan) && (
							<Button
								variant="ghost"
								size="sm"
								onClick={() => {
									setSearchInput("");
									navigate({ search: {} });
								}}
								className="h-7 text-xs text-muted-foreground hover:text-foreground flex items-center gap-1"
							>
								<RotateCcw className="size-3" /> Reset Filter
							</Button>
						)}
					</div>
				</CardContent>
			</Card>

			{/* Data Table */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="p-4 pb-2">
					<CardTitle className="text-sm font-semibold">
						Daftar Pelanggan & Prospek
					</CardTitle>
					<CardDescription className="text-xs">
						Daftar seluruh calon pelanggan yang terdaftar dalam lingkup area
						kerja Anda
					</CardDescription>
				</CardHeader>
				<CardContent className="p-0">
					<Table>
						<TableHeader>
							{table.getHeaderGroups().map((headerGroup) => (
								<TableRow key={headerGroup.id} className="bg-muted/30">
									{headerGroup.headers.map((header) => (
										<TableHead
											key={header.id}
											className="h-10 text-xs font-semibold"
										>
											{header.isPlaceholder
												? null
												: flexRender(
														header.column.columnDef.header,
														header.getContext(),
													)}
										</TableHead>
									))}
								</TableRow>
							))}
						</TableHeader>
						<TableBody>
							{isLoading ? (
								<TableRow>
									<TableCell
										colSpan={columns.length}
										className="h-32 text-center text-xs text-muted-foreground"
									>
										Memuat data direktori industri...
									</TableCell>
								</TableRow>
							) : table.getRowModel().rows?.length ? (
								table.getRowModel().rows.map((row) => (
									<TableRow
										key={row.id}
										data-state={row.getIsSelected() && "selected"}
										className="hover:bg-muted/40 transition-colors"
									>
										{row.getVisibleCells().map((cell) => (
											<TableCell key={cell.id} className="py-2.5">
												{flexRender(
													cell.column.columnDef.cell,
													cell.getContext(),
												)}
											</TableCell>
										))}
									</TableRow>
								))
							) : (
								<TableRow>
									<TableCell
										colSpan={columns.length}
										className="h-32 text-center text-xs text-muted-foreground"
									>
										Tidak ada data perusahaan yang sesuai dengan kriteria filter
										Anda.
									</TableCell>
								</TableRow>
							)}
						</TableBody>
					</Table>

					{/* Pagination Controls */}
					<div className="flex flex-col sm:flex-row items-center justify-between p-4 border-t gap-3 text-xs">
						<div className="flex items-center gap-2 text-muted-foreground">
							<span>Baris per halaman:</span>
							<Select
								value={pageSize.toString()}
								onValueChange={(val) =>
									updateFilters({ pageSize: Number.parseInt(val, 10), page: 1 })
								}
							>
								<SelectTrigger className="h-8 w-18 text-xs">
									<SelectValue placeholder={pageSize.toString()} />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="10">10</SelectItem>
									<SelectItem value="25">25</SelectItem>
									<SelectItem value="50">50</SelectItem>
									<SelectItem value="100">100</SelectItem>
								</SelectContent>
							</Select>
						</div>

						<div className="flex items-center gap-1.5">
							<span className="text-muted-foreground mr-2">
								Halaman {page} dari {totalPages}
							</span>
							<Button
								variant="outline"
								size="sm"
								className="h-8 w-8 p-0"
								disabled={page <= 1 || isLoading}
								onClick={() => updateFilters({ page: page - 1 })}
							>
								<ChevronLeft className="size-4" />
							</Button>
							<Button
								variant="outline"
								size="sm"
								className="h-8 w-8 p-0"
								disabled={page >= totalPages || isPlaceholderData || isLoading}
								onClick={() => updateFilters({ page: page + 1 })}
							>
								<ChevronRight className="size-4" />
							</Button>
						</div>
					</div>
				</CardContent>
			</Card>
		</div>
	);
}
