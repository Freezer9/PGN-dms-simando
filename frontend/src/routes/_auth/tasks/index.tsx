import { createFileRoute, Link } from "@tanstack/react-router";
import {
	type ColumnDef,
	getCoreRowModel,
	getPaginationRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	Building2,
	CheckCircle2,
	Eye,
	RotateCcw,
	Search,
	User,
	XCircle,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
import { DataTable, DataTableToolbar, IconButton } from "@/components/common";
import { SlaClockBadge } from "@/components/tasks/sla-clock-badge";
import {
	TaskActionModal,
	type TaskActionModalType,
} from "@/components/tasks/task-action-modal";
import { TaskQuickPreviewDrawer } from "@/components/tasks/task-quick-preview-drawer";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { formatRole } from "@/lib/roles";

export const Route = createFileRoute("/_auth/tasks/")({
	component: TasksInboxPage,
});

type TaskScope = "my" | "region";
type SortOption = "sla_desc" | "sla_asc" | "name_asc";

function TasksInboxPage() {
	const [scope, setScope] = React.useState<TaskScope>("my");
	const [searchTerm, setSearchTerm] = React.useState("");
	const [stepFilter, setStepFilter] = React.useState<string>("all");
	const [areaFilter, setAreaFilter] = React.useState<string>("all");
	const [sortBy, setSortBy] = React.useState<SortOption>("sla_desc");

	// Active action modal state
	const [modalTask, setModalTask] = React.useState<TaskListItem | null>(null);
	const [modalAction, setModalAction] =
		React.useState<TaskActionModalType>(null);

	// Quick preview drawer state
	const [drawerTask, setDrawerTask] = React.useState<TaskListItem | null>(null);
	const [isDrawerOpen, setIsDrawerOpen] = React.useState(false);

	// Queries
	const {
		data: myTasks = [],
		isLoading: isLoadingMy,
		refetch: refetchMy,
	} = $api.useQuery("get", "/api/tasks/inbox");

	const {
		data: regionTasks = [],
		isLoading: isLoadingRegion,
		refetch: refetchRegion,
	} = $api.useQuery("get", "/api/tasks/region", undefined, {
		enabled: scope === "region",
	});

	const tasks = scope === "my" ? myTasks : regionTasks;
	const isLoading = scope === "my" ? isLoadingMy : isLoadingRegion;

	// Extract unique areas from tasks
	const availableAreas = React.useMemo(() => {
		const areas = new Set<string>();
		for (const t of tasks) {
			if (t.areaName) areas.add(t.areaName);
		}
		return Array.from(areas).sort();
	}, [tasks]);

	// Filter & sort logic
	const filteredTasks = React.useMemo(() => {
		let list = [...tasks];

		// Text search
		if (searchTerm.trim()) {
			const q = searchTerm.toLowerCase();
			list = list.filter(
				(t) =>
					t.namaPerusahaan?.toLowerCase().includes(q) ||
					t.nomor?.toLowerCase().includes(q) ||
					t.submittedByName?.toLowerCase().includes(q) ||
					t.areaName?.toLowerCase().includes(q),
			);
		}

		// Step filter
		if (stepFilter !== "all") {
			list = list.filter((t) => t.stepKind === stepFilter);
		}

		// Area filter
		if (areaFilter !== "all") {
			list = list.filter((t) => t.areaName === areaFilter);
		}

		// Sorting
		list.sort((a, b) => {
			if (sortBy === "sla_desc") {
				return (
					new Date(a.waitingSince).getTime() -
					new Date(b.waitingSince).getTime()
				);
			}
			if (sortBy === "sla_asc") {
				return (
					new Date(b.waitingSince).getTime() -
					new Date(a.waitingSince).getTime()
				);
			}
			if (sortBy === "name_asc") {
				return a.namaPerusahaan.localeCompare(b.namaPerusahaan);
			}
			return 0;
		});

		return list;
	}, [tasks, searchTerm, stepFilter, areaFilter, sortBy]);

	const handleOpenActionModal = React.useCallback(
		(task: TaskListItem, action: NonNullable<TaskActionModalType>) => {
			setModalTask(task);
			setModalAction(action);
		},
		[],
	);

	const handleOpenDrawer = React.useCallback((task: TaskListItem) => {
		setDrawerTask(task);
		setIsDrawerOpen(true);
	}, []);

	// Column definitions using OpenAPI TaskListItem DTO
	const columns = React.useMemo<ColumnDef<TaskListItem>[]>(
		() => [
			{
				accessorKey: "namaPerusahaan",
				header: "Perusahaan",
				meta: {
					headerClassName: "min-w-[280px]",
					cellClassName: "min-w-[280px]",
				},
				cell: ({ row }) => {
					const task = row.original;
					return (
						<div className="flex flex-col gap-0.5">
							<Link
								to="/directory/$companyId"
								params={{ companyId: task.companyId }}
								className="font-medium text-foreground hover:text-primary transition-colors text-sm flex items-center gap-1.5"
							>
								<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
								<span>{task.namaPerusahaan}</span>
							</Link>
							<div className="flex items-center gap-2 text-xs text-muted-foreground">
								<span className="font-mono text-[11px]">{task.nomor}</span>
								<span>•</span>
								<span>{task.industryTypeName || "Industri"}</span>
							</div>
						</div>
					);
				},
			},
			{
				accessorKey: "stepKind",
				header: "Tahap Verifikasi",
				meta: {
					headerClassName: "min-w-[150px]",
					cellClassName: "min-w-[150px]",
				},
				cell: ({ row }) => (
					<Badge variant="secondary" className="text-xs font-medium">
						{formatRole(row.original.stepKind) || "Persetujuan"}
					</Badge>
				),
			},
			{
				id: "location",
				header: "Wilayah & Area",
				meta: {
					headerClassName: "min-w-[150px]",
					cellClassName: "min-w-[150px]",
				},
				cell: ({ row }) => (
					<div className="flex flex-col gap-0.5 text-xs">
						<span className="font-medium text-foreground">
							{row.original.areaName}
						</span>
						<span className="text-muted-foreground text-[11px]">
							{row.original.regionName}
						</span>
					</div>
				),
			},
			{
				accessorKey: "submittedByName",
				header: "Diajukan Oleh",
				meta: {
					headerClassName: "min-w-[140px]",
					cellClassName: "min-w-[140px]",
				},
				cell: ({ row }) => (
					<div className="flex items-center gap-1.5 text-xs text-foreground">
						<User className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
						<span>{row.original.submittedByName || "-"}</span>
					</div>
				),
			},
			{
				accessorKey: "waitingSince",
				header: "Status SLA",
				meta: {
					headerClassName: "min-w-[170px]",
					cellClassName: "min-w-[170px]",
				},
				cell: ({ row }) => (
					<SlaClockBadge waitingSince={row.original.waitingSince} />
				),
			},
			{
				id: "actions",
				header: () => <div className="text-right pr-4">Aksi</div>,
				meta: {
					headerClassName: "min-w-[160px] text-right",
					cellClassName: "min-w-[160px]",
				},
				cell: ({ row }) => {
					const task = row.original;
					return (
						<div className="flex items-center justify-end gap-1.5 pr-2">
							<IconButton
								tooltip="Tinjau Cepat"
								onClick={() => handleOpenDrawer(task)}
								aria-label="Tinjau Cepat"
							>
								<Eye className="size-4" />
							</IconButton>
							<IconButton
								tooltip="Kembalikan untuk Revisi"
								className="text-amber-600 border-amber-200 hover:bg-amber-50 hover:text-amber-700 dark:border-amber-800/60 dark:hover:bg-amber-950/50"
								onClick={() => handleOpenActionModal(task, "Revisi")}
								aria-label="Kembalikan untuk revisi"
							>
								<RotateCcw className="size-4" />
							</IconButton>
							<IconButton
								tooltip="Tolak Berkas"
								danger
								onClick={() => handleOpenActionModal(task, "Tolak")}
								aria-label="Tolak berkas"
							>
								<XCircle className="size-4" />
							</IconButton>
							<IconButton
								tooltip="Setujui dan Lanjutkan"
								className="text-emerald-600 border-emerald-200 hover:bg-emerald-50 hover:text-emerald-700 dark:border-emerald-800/60 dark:hover:bg-emerald-950/50"
								onClick={() => handleOpenActionModal(task, "Setuju")}
								aria-label="Setujui dan lanjutkan"
							>
								<CheckCircle2 className="size-4" />
							</IconButton>
						</div>
					);
				},
			},
		],
		[handleOpenDrawer, handleOpenActionModal],
	);

	const [pagination, setPagination] = React.useState({
		pageIndex: 0,
		pageSize: 10,
	});

	const table = useReactTable({
		data: filteredTasks,
		columns,
		getCoreRowModel: getCoreRowModel(),
		getPaginationRowModel: getPaginationRowModel(),
		onPaginationChange: setPagination,
		state: {
			pagination,
		},
	});

	return (
		<div className="space-y-4">
			{/* Top Scope Selector & Controls */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
				<Tabs
					value={scope}
					onValueChange={(val) => {
						setScope(val as TaskScope);
						setPagination((prev) => ({ ...prev, pageIndex: 0 }));
					}}
					className="w-full sm:w-auto"
				>
					<TabsList className="grid w-full sm:w-auto grid-cols-2">
						<TabsTrigger value="my" className="px-4 text-xs">
							Tugas Saya ({myTasks.length})
						</TabsTrigger>
						<TabsTrigger value="region" className="px-4 text-xs">
							Semua Tugas Wilayah ({regionTasks.length})
						</TabsTrigger>
					</TabsList>
				</Tabs>
			</div>

			{/* Filter & Search Toolbar */}
			<DataTableToolbar
				actions={
					<div className="text-xs text-muted-foreground">
						Menampilkan{" "}
						<strong className="text-foreground font-semibold">
							{filteredTasks.length}
						</strong>{" "}
						tugas aktif
					</div>
				}
			>
				{/* Search Input */}
				<div className="relative min-w-[220px] flex-1 max-w-sm">
					<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
					<Input
						placeholder="Cari perusahaan, nomor, pengaju..."
						value={searchTerm}
						onChange={(e) => {
							setSearchTerm(e.target.value);
							setPagination((prev) => ({ ...prev, pageIndex: 0 }));
						}}
						className="pl-9 h-9 text-xs"
					/>
				</div>

				{/* Step Kind Filter */}
				<div className="w-[180px]">
					<Select
						value={stepFilter}
						onValueChange={(val) => {
							setStepFilter(val);
							setPagination((prev) => ({ ...prev, pageIndex: 0 }));
						}}
					>
						<SelectTrigger className="h-9 text-xs w-full">
							<SelectValue placeholder="Semua Tahap Verifikasi" />
						</SelectTrigger>
						<SelectContent>
							<SelectItem value="all">Semua Tahap Verifikasi</SelectItem>
							<SelectItem value="AreaHead">Area Head</SelectItem>
							<SelectItem value="RegionalAdmin">Regional Admin</SelectItem>
							<SelectItem value="Reviewer1">Reviewer 1 (Sales)</SelectItem>
							<SelectItem value="Reviewer2">Reviewer 2 (Teknik)</SelectItem>
							<SelectItem value="Reviewer3">Reviewer 3 (Keuangan)</SelectItem>
							<SelectItem value="DivisionHead">Division Head</SelectItem>
						</SelectContent>
					</Select>
				</div>

				{/* Area Filter */}
				<div className="w-[150px]">
					<Select
						value={areaFilter}
						onValueChange={(val) => {
							setAreaFilter(val);
							setPagination((prev) => ({ ...prev, pageIndex: 0 }));
						}}
					>
						<SelectTrigger className="h-9 text-xs w-full">
							<SelectValue placeholder="Semua Area" />
						</SelectTrigger>
						<SelectContent>
							<SelectItem value="all">Semua Area</SelectItem>
							{availableAreas.map((area) => (
								<SelectItem key={area} value={area}>
									{area}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
				</div>

				{/* Sort Selector */}
				<div className="w-[180px]">
					<Select
						value={sortBy}
						onValueChange={(val) => setSortBy(val as SortOption)}
					>
						<SelectTrigger className="h-9 text-xs w-full">
							<SelectValue placeholder="Urutkan Berdasarkan" />
						</SelectTrigger>
						<SelectContent>
							<SelectItem value="sla_desc">Waktu Tunggu Terlama</SelectItem>
							<SelectItem value="sla_asc">Terbaru Diajukan</SelectItem>
							<SelectItem value="name_asc">Nama Perusahaan (A-Z)</SelectItem>
						</SelectContent>
					</Select>
				</div>
			</DataTableToolbar>

			{/* Task List Table with DataTable */}
			<DataTable
				table={table}
				columnsCount={columns.length}
				isLoading={isLoading}
				skeletonRows={5}
				emptyTitle={
					searchTerm || stepFilter !== "all" || areaFilter !== "all"
						? "Tidak Ada Tugas Ditemukan"
						: "Tidak Ada Tugas Menunggu"
				}
				emptyDescription={
					searchTerm || stepFilter !== "all" || areaFilter !== "all"
						? "Tidak ada tugas yang sesuai dengan filter pencarian yang diterapkan."
						: scope === "my"
							? "Semua berkas pada antrean Anda telah selesai ditindaklanjuti."
							: "Tidak ada berkas aktif yang sedang berproses pada wilayah kerja Anda."
				}
				emptyIcon={
					searchTerm || stepFilter !== "all" || areaFilter !== "all"
						? "search"
						: "empty"
				}
				onResetFilters={
					searchTerm || stepFilter !== "all" || areaFilter !== "all"
						? () => {
								setSearchTerm("");
								setStepFilter("all");
								setAreaFilter("all");
							}
						: undefined
				}
				resetLabel="Reset Filter"
				pagination={
					filteredTasks.length > 0
						? {
								page: pagination.pageIndex + 1,
								pageSize: pagination.pageSize,
								totalCount: filteredTasks.length,
								totalPages: table.getPageCount(),
								onPageChange: (p) => table.setPageIndex(p - 1),
								onPageSizeChange: (size) => table.setPageSize(size),
								pageSizeOptions: [10, 25, 50],
							}
						: undefined
				}
			/>

			{/* Action Modal (Setuju / Revisi / Tolak / Reassign) */}
			<TaskActionModal
				task={modalTask}
				actionType={modalAction}
				isOpen={!!modalAction}
				onClose={() => {
					setModalAction(null);
					setModalTask(null);
				}}
				onSuccess={() => {
					refetchMy();
					if (scope === "region") refetchRegion();
				}}
			/>

			{/* Quick Preview Drawer */}
			<TaskQuickPreviewDrawer
				task={drawerTask}
				isOpen={isDrawerOpen}
				onClose={() => {
					setIsDrawerOpen(false);
					setDrawerTask(null);
				}}
				onTakeAction={(task, actionType) => {
					setIsDrawerOpen(false);
					if (actionType) {
						handleOpenActionModal(task, actionType);
					}
				}}
			/>
		</div>
	);
}
