import { createFileRoute, Link } from "@tanstack/react-router";
import {
	type ColumnDef,
	getCoreRowModel,
	getPaginationRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	AlertOctagon,
	Building2,
	CheckCircle2,
	Eye,
	Search,
	User,
	UserCheck,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { TaskListItem } from "@/api/types";
import { DataTable, DataTableToolbar } from "@/components/common";
import { SlaClockBadge } from "@/components/tasks/sla-clock-badge";
import {
	TaskActionModal,
	type TaskActionModalType,
} from "@/components/tasks/task-action-modal";
import { TaskQuickPreviewDrawer } from "@/components/tasks/task-quick-preview-drawer";
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

export const Route = createFileRoute("/_auth/tasks/blocked")({
	component: BlockedTasksPage,
});

function BlockedTasksPage() {
	const [searchTerm, setSearchTerm] = React.useState("");
	const [areaFilter, setAreaFilter] = React.useState<string>("all");

	// Active modal and drawer states
	const [modalTask, setModalTask] = React.useState<TaskListItem | null>(null);
	const [modalAction, setModalAction] =
		React.useState<TaskActionModalType>(null);

	const [drawerTask, setDrawerTask] = React.useState<TaskListItem | null>(null);
	const [isDrawerOpen, setIsDrawerOpen] = React.useState(false);

	// Query blocked tasks
	const {
		data: blockedTasks = [],
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/tasks/blocked");

	// Extract unique areas
	const availableAreas = React.useMemo(() => {
		const areas = new Set<string>();
		for (const t of blockedTasks) {
			if (t.areaName) areas.add(t.areaName);
		}
		return Array.from(areas).sort();
	}, [blockedTasks]);

	// Filter & sort (prioritize longest waiting)
	const filteredTasks = React.useMemo(() => {
		let list = [...blockedTasks];

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

		if (areaFilter !== "all") {
			list = list.filter((t) => t.areaName === areaFilter);
		}

		// Sort by longest waiting duration first
		list.sort(
			(a, b) =>
				new Date(a.waitingSince).getTime() - new Date(b.waitingSince).getTime(),
		);

		return list;
	}, [blockedTasks, searchTerm, areaFilter]);

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
								<Building2 className="size-3.5 text-primary shrink-0" />
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
				header: "Tahap Tertahan",
				meta: {
					headerClassName: "min-w-[160px]",
					cellClassName: "min-w-[160px]",
				},
				cell: ({ row }) => (
					<Badge
						variant="secondary"
						className="text-xs font-medium bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-300"
					>
						{row.original.stepKind ?? "Persetujuan"}
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
					headerClassName: "min-w-[150px]",
					cellClassName: "min-w-[150px]",
				},
				cell: ({ row }) => (
					<div className="flex items-center gap-1.5 text-xs text-foreground">
						<User className="size-3.5 text-muted-foreground shrink-0" />
						<span>{row.original.submittedByName || "-"}</span>
					</div>
				),
			},
			{
				accessorKey: "waitingSince",
				header: "Durasi Keterlambatan",
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
				header: () => <div className="text-right pr-4">Tindakan Eskalasi</div>,
				meta: {
					headerClassName: "min-w-[224px] text-right",
					cellClassName: "min-w-[224px]",
				},
				cell: ({ row }) => {
					const task = row.original;
					return (
						<div className="flex items-center justify-end gap-1.5 pr-2">
							<Button
								size="sm"
								variant="ghost"
								className="h-8 text-xs"
								onClick={() => handleOpenDrawer(task)}
								title="Tinjau Cepat"
							>
								<Eye className="size-3.5" />
								Tinjau
							</Button>
							<Button
								size="sm"
								variant="outline"
								className="h-8 text-xs text-blue-600 border-blue-200 hover:bg-blue-50 dark:hover:bg-blue-950/50"
								onClick={() => handleOpenActionModal(task, "Reassign")}
								title="Tugaskan ulang reviewer"
							>
								<UserCheck className="size-3.5" />
								Tugaskan Ulang
							</Button>
							<Button
								size="sm"
								className="h-8 text-xs bg-emerald-600 hover:bg-emerald-700 text-white font-medium"
								onClick={() => handleOpenActionModal(task, "Setuju")}
								title="Setujui"
							>
								<CheckCircle2 className="size-3.5" />
								Setuju
							</Button>
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
			{/* SLA Escalation Notice Banner */}
			<div className="flex items-start gap-3 p-4 rounded-xl bg-rose-50/80 dark:bg-rose-950/30 border border-rose-200 dark:border-rose-900/60 shadow-xs">
				<div className="p-2 rounded-lg bg-rose-100 dark:bg-rose-900/50 text-rose-600 dark:text-rose-400 shrink-0">
					<AlertOctagon className="h-5 w-5" />
				</div>
				<div className="space-y-1 text-xs">
					<h3 className="font-semibold text-rose-900 dark:text-rose-200 text-sm">
						Pemantauan Berkas Tertahan & Kemacetan (Bottleneck SLA)
					</h3>
					<p className="text-rose-700 dark:text-rose-300/90 leading-relaxed">
						Halaman ini menampilkan seluruh berkas pengajuan pelanggan yang
						telah menunggu lebih dari <strong>7 hari kalender</strong> pada
						suatu tahap atau belum memiliki reviewer yang ditugaskan. Regional
						Admin atau System Admin dapat melakukan penugasan ulang (reassign)
						untuk mempercepat kelancaran proses.
					</p>
				</div>
			</div>

			{/* Filter Toolbar */}
			<DataTableToolbar
				actions={
					<div className="text-xs text-muted-foreground">
						Total Tertahan:{" "}
						<strong className="text-rose-600 dark:text-rose-400 ml-1 font-bold">
							{filteredTasks.length} berkas
						</strong>
					</div>
				}
			>
				<div className="relative min-w-[220px] flex-1 max-w-sm">
					<Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
					<Input
						placeholder="Cari berkas tertahan..."
						value={searchTerm}
						onChange={(e) => {
							setSearchTerm(e.target.value);
							setPagination((prev) => ({ ...prev, pageIndex: 0 }));
						}}
						className="pl-9 h-9 text-xs"
					/>
				</div>

				<div className="w-[180px]">
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
			</DataTableToolbar>

			{/* Blocked Tasks Table with DataTable */}
			<DataTable
				table={table}
				columnsCount={columns.length}
				isLoading={isLoading}
				skeletonRows={5}
				emptyTitle={
					searchTerm || areaFilter !== "all"
						? "Tidak Ada Berkas Ditemukan"
						: "Tidak Ada Berkas Tertahan"
				}
				emptyDescription={
					searchTerm || areaFilter !== "all"
						? "Tidak ada berkas tertahan yang sesuai dengan kata kunci pencarian Anda."
						: "Semua berkas permohonan berjalan sesuai dengan target SLA proses bisnis (< 7 hari)."
				}
				emptyIcon={searchTerm || areaFilter !== "all" ? "search" : "empty"}
				onResetFilters={
					searchTerm || areaFilter !== "all"
						? () => {
								setSearchTerm("");
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

			{/* Action Modal */}
			<TaskActionModal
				task={modalTask}
				actionType={modalAction}
				isOpen={!!modalAction}
				onClose={() => {
					setModalAction(null);
					setModalTask(null);
				}}
				onSuccess={() => refetch()}
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
