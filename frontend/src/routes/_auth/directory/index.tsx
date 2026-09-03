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
import { Input } from "@/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
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
			},
		},
	});

	const items = React.useMemo(() => data?.items || [], [data?.items]);
	const totalCount = Number(data?.totalCount || 0);
	const totalPages = Math.ceil(totalCount / pageSize) || 1;

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
				header: "Sales PIC",
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
							search.kawasan) && (
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
				<div className="relative flex-1 min-w-[240px] max-w-md">
					<Search className="absolute left-2.5 top-2.5 size-4 text-muted-foreground" />
					<Input
						placeholder="Cari nama perusahaan atau nomor registrasi..."
						value={searchInput}
						onChange={(e) => setSearchInput(e.target.value)}
						className="pl-9 h-9 text-xs"
					/>
				</div>

				<div className="w-[180px]">
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

				<div className="w-[160px]">
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
							<SelectItem value="JalurExisting">Jalur Existing</SelectItem>
							<SelectItem value="Pengembangan">Pengembangan</SelectItem>
						</SelectContent>
					</Select>
				</div>
			</DataTableToolbar>

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
