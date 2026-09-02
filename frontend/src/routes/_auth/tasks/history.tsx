import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	type ColumnDef,
	flexRender,
	getCoreRowModel,
	useReactTable,
} from "@tanstack/react-table";
import {
	AlertOctagon,
	Building2,
	CheckCircle2,
	ExternalLink,
	History,
	MessageSquare,
	RotateCcw,
	Send,
	Undo2,
	XCircle,
} from "lucide-react";
import * as React from "react";
import { z } from "zod";
import { $api } from "@/api/client";
import type { StatusEventAction, TaskHistoryItem } from "@/api/types";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { TableEmptyState } from "@/components/ui/table-empty-state";
import { TablePagination } from "@/components/ui/table-pagination";
import { TableSkeleton } from "@/components/ui/table-skeleton";

const historySearchSchema = z.object({
	page: z.number().default(1).optional(),
	pageSize: z.number().default(25).optional(),
});

export const Route = createFileRoute("/_auth/tasks/history")({
	validateSearch: historySearchSchema,
	component: TaskHistoryPage,
});

function TaskHistoryPage() {
	const search = Route.useSearch();
	const navigate = useNavigate({ from: Route.fullPath });

	const page = search.page || 1;
	const pageSize = search.pageSize || 25;

	// Query paged history
	const { data, isLoading } = $api.useQuery("get", "/api/tasks/history", {
		params: {
			query: {
				page,
				pageSize,
			},
		},
	});

	const items = React.useMemo(() => data?.items || [], [data?.items]);
	const totalCount = Number(data?.totalCount) || 0;
	const totalPages = Math.ceil(totalCount / pageSize) || 1;

	const handlePageChange = (newPage: number) => {
		navigate({
			search: (prev) => ({
				...prev,
				page: newPage,
			}),
		});
	};

	const handlePageSizeChange = (newPageSize: number) => {
		navigate({
			search: (prev) => ({
				...prev,
				pageSize: newPageSize,
				page: 1,
			}),
		});
	};

	// Column definitions using OpenAPI TaskHistoryItem DTO
	const columns = React.useMemo<ColumnDef<TaskHistoryItem>[]>(
		() => [
			{
				accessorKey: "namaPerusahaan",
				header: "Perusahaan",
				cell: ({ row }) => {
					const item = row.original;
					return (
						<div className="space-y-0.5">
							<div className="flex items-center gap-1.5 font-medium text-foreground text-xs">
								<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
								<span>{item.namaPerusahaan}</span>
							</div>
							<span className="text-[11px] font-mono text-muted-foreground block pl-5">
								{item.nomor}
							</span>
						</div>
					);
				},
			},
			{
				accessorKey: "action",
				header: "Tindakan",
				cell: ({ row }) => <ActionBadge action={row.original.action} />,
			},
			{
				accessorKey: "toStatus",
				header: "Menuju Status",
				cell: ({ row }) => (
					<Badge variant="outline" className="text-[11px]">
						{row.original.toStatus}
					</Badge>
				),
			},
			{
				accessorKey: "comment",
				header: "Catatan / Alasan",
				cell: ({ row }) => {
					const comment = row.original.comment;
					return comment ? (
						<div className="flex items-start gap-1.5 text-xs text-muted-foreground max-w-[280px]">
							<MessageSquare className="h-3.5 w-3.5 mt-0.5 shrink-0 text-muted-foreground/60" />
							<span className="line-clamp-2 italic">"{comment}"</span>
						</div>
					) : (
						<span className="text-xs text-muted-foreground/50">-</span>
					);
				},
			},
			{
				accessorKey: "actedAt",
				header: "Waktu Proses",
				cell: ({ row }) => (
					<span className="text-xs text-muted-foreground">
						{new Date(row.original.actedAt).toLocaleDateString("id-ID", {
							day: "numeric",
							month: "short",
							year: "numeric",
							hour: "2-digit",
							minute: "2-digit",
						})}
					</span>
				),
			},
			{
				id: "actions",
				header: () => <div className="text-right pr-2">Aksi</div>,
				cell: ({ row }) => (
					<div className="text-right">
						<Button asChild size="sm" variant="ghost" className="h-7 text-xs">
							<Link
								to="/directory/$companyId"
								params={{ companyId: row.original.companyId }}
							>
								<ExternalLink className="h-3.5 w-3.5 mr-1" />
								Buka Berkas
							</Link>
						</Button>
					</div>
				),
			},
		],
		[],
	);

	const table = useReactTable({
		data: items,
		columns,
		getCoreRowModel: getCoreRowModel(),
	});

	return (
		<div className="space-y-4">
			{/* Header info */}
			<div className="flex items-center justify-between">
				<div className="space-y-0.5">
					<h3 className="text-sm font-semibold text-foreground flex items-center gap-2">
						<History className="h-4 w-4 text-primary" />
						<span>Log Riwayat Keputusan Workflow</span>
					</h3>
					<p className="text-xs text-muted-foreground">
						Daftar seluruh tindakan verifikasi dan keputusan yang telah Anda
						proses pada sistem.
					</p>
				</div>
				<div className="text-xs text-muted-foreground">
					Total: <strong className="text-foreground">{totalCount}</strong>{" "}
					riwayat
				</div>
			</div>

			{/* History Table with TanStack Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						{table.getHeaderGroups().map((headerGroup) => (
							<TableRow key={headerGroup.id}>
								{headerGroup.headers.map((header) => (
									<TableHead key={header.id} className="font-semibold text-xs">
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
							<TableSkeleton columns={columns.length} rows={5} />
						) : table.getRowModel().rows?.length ? (
							table.getRowModel().rows.map((row) => (
								<TableRow
									key={row.id}
									className="hover:bg-muted/30 transition-colors"
								>
									{row.getVisibleCells().map((cell) => (
										<TableCell key={cell.id} className="py-3">
											{flexRender(
												cell.column.columnDef.cell,
												cell.getContext(),
											)}
										</TableCell>
									))}
								</TableRow>
							))
						) : (
							<TableEmptyState
								colSpan={columns.length}
								icon="empty"
								title="Belum Ada Riwayat Tindakan"
								description="Tindakan persetujuan atau revisi yang Anda lakukan akan tercatat di sini."
							/>
						)}
					</TableBody>
				</Table>

				{/* Standardized Table Pagination */}
				<TablePagination
					pageIndex={page - 1}
					page={page}
					pageSize={pageSize}
					totalCount={totalCount}
					totalPages={totalPages}
					onPageChange={handlePageChange}
					onPageSizeChange={handlePageSizeChange}
					pageSizeOptions={[10, 25, 50, 100]}
					className="border-t px-4"
				/>
			</div>
		</div>
	);
}

function ActionBadge({ action }: { action: StatusEventAction }) {
	switch (action) {
		case "Setuju":
			return (
				<Badge className="bg-emerald-500/15 text-emerald-700 dark:text-emerald-300 border-emerald-500/30 gap-1 text-[11px] font-normal hover:bg-emerald-500/20">
					<CheckCircle2 className="h-3 w-3" />
					Setuju
				</Badge>
			);
		case "Tolak":
			return (
				<Badge className="bg-rose-500/15 text-rose-700 dark:text-rose-300 border-rose-500/30 gap-1 text-[11px] font-normal hover:bg-rose-500/20">
					<XCircle className="h-3 w-3" />
					Tolak
				</Badge>
			);
		case "Revisi":
			return (
				<Badge className="bg-amber-500/15 text-amber-700 dark:text-amber-300 border-amber-500/30 gap-1 text-[11px] font-normal hover:bg-amber-500/20">
					<RotateCcw className="h-3 w-3" />
					Revisi
				</Badge>
			);
		case "Submit":
			return (
				<Badge className="bg-blue-500/15 text-blue-700 dark:text-blue-300 border-blue-500/30 gap-1 text-[11px] font-normal hover:bg-blue-500/20">
					<Send className="h-3 w-3" />
					Submit
				</Badge>
			);
		case "Reassign":
			return (
				<Badge className="bg-purple-500/15 text-purple-700 dark:text-purple-300 border-purple-500/30 gap-1 text-[11px] font-normal hover:bg-purple-500/20">
					<Undo2 className="h-3 w-3" />
					Alihkan
				</Badge>
			);
		case "Discontinue":
			return (
				<Badge className="bg-slate-500/15 text-slate-700 dark:text-slate-300 border-slate-500/30 gap-1 text-[11px] font-normal hover:bg-slate-500/20">
					<AlertOctagon className="h-3 w-3" />
					Hentikan
				</Badge>
			);
		case "BreakGlass":
			return (
				<Badge className="bg-destructive/15 text-destructive border-destructive/30 gap-1 text-[11px] font-normal hover:bg-destructive/20">
					<AlertOctagon className="h-3 w-3" />
					Break Glass
				</Badge>
			);
		case "Rework":
			return (
				<Badge className="bg-amber-500/15 text-amber-700 dark:text-amber-300 border-amber-500/30 gap-1 text-[11px] font-normal hover:bg-amber-500/20">
					<RotateCcw className="h-3 w-3" />
					Rework
				</Badge>
			);
		default:
			return (
				<Badge variant="secondary" className="text-[11px] font-normal">
					{action}
				</Badge>
			);
	}
}
