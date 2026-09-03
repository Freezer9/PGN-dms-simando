import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	type ColumnDef,
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
import { DataTable, PageHeader } from "@/components/common";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";

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
				meta: {
					headerClassName: "min-w-[260px]",
					cellClassName: "min-w-[260px]",
				},
				cell: ({ row }) => {
					const item = row.original;
					return (
						<div className="space-y-0.5">
							<div className="flex items-center gap-1.5 font-medium text-foreground text-xs">
								<Building2 className="size-3.5 text-primary shrink-0" />
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
				meta: {
					headerClassName: "min-w-[130px]",
					cellClassName: "min-w-[130px]",
				},
				cell: ({ row }) => <ActionBadge action={row.original.action} />,
			},
			{
				accessorKey: "toStatus",
				header: "Menuju Status",
				meta: {
					headerClassName: "min-w-[140px]",
					cellClassName: "min-w-[140px]",
				},
				cell: ({ row }) => (
					<Badge variant="outline" className="text-[11px]">
						{row.original.toStatus}
					</Badge>
				),
			},
			{
				accessorKey: "comment",
				header: "Catatan / Alasan",
				meta: {
					headerClassName: "min-w-[310px]",
					cellClassName: "min-w-[310px]",
				},
				cell: ({ row }) => {
					const comment = row.original.comment;
					return comment ? (
						<div className="flex items-start gap-1.5 text-xs text-muted-foreground max-w-[280px]">
							<MessageSquare className="size-3.5 mt-0.5 shrink-0 text-muted-foreground/60" />
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
				meta: {
					headerClassName: "min-w-[170px]",
					cellClassName: "min-w-[170px]",
				},
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
				meta: {
					headerClassName: "min-w-[124px] text-right",
					cellClassName: "min-w-[124px]",
				},
				cell: ({ row }) => (
					<div className="text-right">
						<Button asChild size="sm" variant="ghost" className="h-7 text-xs">
							<Link
								to="/directory/$companyId"
								params={{ companyId: row.original.companyId }}
							>
								<ExternalLink className="size-3.5" />
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
			{/* Page Header */}
			<PageHeader
				title="Log Riwayat Keputusan Workflow"
				description="Daftar seluruh tindakan verifikasi dan keputusan yang telah Anda proses pada sistem."
				badge={
					<span className="p-1 rounded-md bg-primary/10 text-primary">
						<History className="h-4 w-4" />
					</span>
				}
				actions={
					<div className="text-xs text-muted-foreground">
						Total: <strong className="text-foreground">{totalCount}</strong>{" "}
						riwayat
					</div>
				}
			/>

			{/* History Table with DataTable */}
			<DataTable
				table={table}
				columnsCount={columns.length}
				isLoading={isLoading}
				skeletonRows={5}
				emptyTitle="Belum Ada Riwayat Tindakan"
				emptyDescription="Tindakan persetujuan atau revisi yang Anda lakukan akan tercatat di sini."
				emptyIcon="empty"
				pagination={
					totalCount > 0
						? {
								page,
								pageSize,
								totalCount,
								totalPages,
								onPageChange: handlePageChange,
								onPageSizeChange: handlePageSizeChange,
								pageSizeOptions: [10, 25, 50, 100],
							}
						: undefined
				}
			/>
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
