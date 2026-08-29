import { createFileRoute, Link, useNavigate } from "@tanstack/react-router";
import {
	AlertOctagon,
	Building2,
	CheckCircle2,
	ChevronLeft,
	ChevronRight,
	ExternalLink,
	History,
	Loader2,
	MessageSquare,
	RotateCcw,
	Send,
	Undo2,
	XCircle,
} from "lucide-react";
import { z } from "zod";
import { $api } from "@/api/client";
import type { StatusEventAction } from "@/api/types";
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

	const items = data?.items || [];
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

			{/* History Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs">
								Perusahaan
							</TableHead>
							<TableHead className="font-semibold text-xs">Tindakan</TableHead>
							<TableHead className="font-semibold text-xs">
								Menuju Status
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Catatan / Alasan
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Waktu Proses
							</TableHead>
							<TableHead className="font-semibold text-xs text-right">
								Aksi
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell colSpan={6} className="h-48 text-center">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-6 w-6 animate-spin text-primary" />
										<span className="text-xs">Memuat riwayat tindakan...</span>
									</div>
								</TableCell>
							</TableRow>
						) : items.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="h-48 text-center">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<History className="h-8 w-8 text-muted-foreground/40" />
										<p className="text-sm font-medium">
											Belum ada riwayat tindakan
										</p>
										<p className="text-xs">
											Tindakan persetujuan atau revisi yang Anda lakukan akan
											tercatat di sini.
										</p>
									</div>
								</TableCell>
							</TableRow>
						) : (
							items.map((item) => (
								<TableRow
									key={`${item.companyId}-${item.actedAt}-${item.action}`}
									className="hover:bg-muted/30 transition-colors"
								>
									{/* Company Info */}
									<TableCell>
										<div className="space-y-0.5">
											<div className="flex items-center gap-1.5 font-medium text-foreground text-xs">
												<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
												<span>{item.namaPerusahaan}</span>
											</div>
											<span className="text-[11px] font-mono text-muted-foreground block pl-5">
												{item.nomor}
											</span>
										</div>
									</TableCell>

									{/* Action Taken */}
									<TableCell>
										<ActionBadge action={item.action} />
									</TableCell>

									{/* Target Status */}
									<TableCell>
										<Badge variant="outline" className="text-[11px]">
											{item.toStatus}
										</Badge>
									</TableCell>

									{/* Comment */}
									<TableCell className="max-w-[280px]">
										{item.comment ? (
											<div className="flex items-start gap-1.5 text-xs text-muted-foreground">
												<MessageSquare className="h-3.5 w-3.5 mt-0.5 shrink-0 text-muted-foreground/60" />
												<span className="line-clamp-2 italic">
													"{item.comment}"
												</span>
											</div>
										) : (
											<span className="text-xs text-muted-foreground/50">
												-
											</span>
										)}
									</TableCell>

									{/* Acted At */}
									<TableCell className="text-xs text-muted-foreground">
										{new Date(item.actedAt).toLocaleDateString("id-ID", {
											day: "numeric",
											month: "short",
											year: "numeric",
											hour: "2-digit",
											minute: "2-digit",
										})}
									</TableCell>

									{/* Action Button */}
									<TableCell className="text-right">
										<Button
											asChild
											size="sm"
											variant="ghost"
											className="h-7 text-xs"
										>
											<Link
												to="/directory/$companyId"
												params={{ companyId: item.companyId }}
											>
												<ExternalLink className="h-3.5 w-3.5 mr-1" />
												Buka Berkas
											</Link>
										</Button>
									</TableCell>
								</TableRow>
							))
						)}
					</TableBody>
				</Table>
			</div>

			{/* Pagination Controls */}
			{totalPages > 1 && (
				<div className="flex items-center justify-between text-xs text-muted-foreground px-2 py-1">
					<div>
						Halaman {page} dari {totalPages}
					</div>
					<div className="flex items-center gap-2">
						<Button
							size="sm"
							variant="outline"
							className="h-7 px-2"
							disabled={page <= 1}
							onClick={() => handlePageChange(page - 1)}
						>
							<ChevronLeft className="h-3.5 w-3.5 mr-1" />
							Sebelumnya
						</Button>
						<Button
							size="sm"
							variant="outline"
							className="h-7 px-2"
							disabled={page >= totalPages}
							onClick={() => handlePageChange(page + 1)}
						>
							Berikutnya
							<ChevronRight className="h-3.5 w-3.5 ml-1" />
						</Button>
					</div>
				</div>
			)}
		</div>
	);
}

function ActionBadge({ action }: { action: StatusEventAction }) {
	switch (action) {
		case "Setuju":
		case "Issue":
			return (
				<Badge
					variant="default"
					className="bg-emerald-600 hover:bg-emerald-700 text-[11px] gap-1"
				>
					<CheckCircle2 className="h-3 w-3" />
					Setuju
				</Badge>
			);
		case "Revisi":
			return (
				<Badge
					variant="outline"
					className="border-amber-400 text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/40 text-[11px] gap-1"
				>
					<RotateCcw className="h-3 w-3" />
					Revisi
				</Badge>
			);
		case "Tolak":
			return (
				<Badge variant="destructive" className="text-[11px] gap-1 shadow-none">
					<XCircle className="h-3 w-3" />
					Tolak
				</Badge>
			);
		case "Rework":
			return (
				<Badge
					variant="outline"
					className="border-blue-400 text-blue-600 dark:text-blue-400 bg-blue-50 dark:bg-blue-950/40 text-[11px] gap-1"
				>
					<Undo2 className="h-3 w-3" />
					Rework
				</Badge>
			);
		case "Discontinue":
			return (
				<Badge
					variant="outline"
					className="border-rose-400 text-rose-600 bg-rose-50 dark:bg-rose-950/40 text-[11px] gap-1"
				>
					<AlertOctagon className="h-3 w-3" />
					Discontinue
				</Badge>
			);
		case "Submit":
			return (
				<Badge variant="secondary" className="text-[11px] gap-1">
					<Send className="h-3 w-3" />
					Submit
				</Badge>
			);
		default:
			return (
				<Badge variant="outline" className="text-[11px]">
					{action}
				</Badge>
			);
	}
}
