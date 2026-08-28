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
	const totalCount = data?.totalCount || 0;
	const totalPages = data?.totalPages || 1;

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
								Status Akhir
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Catatan / Alasan
							</TableHead>
							<TableHead className="font-semibold text-xs">
								Waktu Keputusan
							</TableHead>
							<TableHead className="text-right font-semibold text-xs pr-4">
								Tautan
							</TableHead>
						</TableRow>
					</TableHeader>
					<TableBody>
						{isLoading ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-12">
									<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
										<Loader2 className="h-6 w-6 animate-spin text-primary" />
										<span className="text-sm font-medium">
											Memuat riwayat persetujuan...
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : items.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-14">
									<div className="flex flex-col items-center justify-center gap-2 max-w-sm mx-auto text-muted-foreground">
										<div className="p-3 rounded-full bg-muted text-muted-foreground">
											<History className="h-6 w-6" />
										</div>
										<h4 className="font-semibold text-foreground text-base">
											Belum Ada Riwayat Keputusan
										</h4>
										<p className="text-xs text-muted-foreground text-center leading-relaxed">
											Anda belum pernah mengambil tindakan atau keputusan
											persetujuan pada berkas manapun.
										</p>
									</div>
								</TableCell>
							</TableRow>
						) : (
							items.map((item, index) => (
								<TableRow
									// biome-ignore lint/suspicious/noArrayIndexKey: unique row key with index fallback
									key={`${item.companyId}-${item.actedAt}-${index}`}
									className="hover:bg-muted/30 transition-colors"
								>
									{/* Company Info */}
									<TableCell className="py-3">
										<div className="flex flex-col gap-0.5">
											<span className="font-medium text-foreground text-sm flex items-center gap-1.5">
												<Building2 className="h-3.5 w-3.5 text-primary shrink-0" />
												<span>{item.namaPerusahaan}</span>
											</span>
											<span className="font-mono text-[11px] text-muted-foreground">
												{item.nomor}
											</span>
										</div>
									</TableCell>

									{/* Action Badge */}
									<TableCell className="py-3">
										<ActionBadge action={item.action} />
									</TableCell>

									{/* Target Status */}
									<TableCell className="py-3">
										<Badge variant="outline" className="text-xs font-mono">
											{item.toStatus}
										</Badge>
									</TableCell>

									{/* Comments */}
									<TableCell className="py-3 max-w-xs">
										{item.comment ? (
											<div className="flex items-start gap-1.5 text-xs text-foreground bg-muted/40 p-2 rounded-md border">
												<MessageSquare className="h-3 w-3 text-muted-foreground shrink-0 mt-0.5" />
												<span className="line-clamp-2">{item.comment}</span>
											</div>
										) : (
											<span className="text-xs text-muted-foreground italic">
												-
											</span>
										)}
									</TableCell>

									{/* Acted At */}
									<TableCell className="py-3 text-xs text-muted-foreground">
										{new Date(item.actedAt).toLocaleString("id-ID", {
											dateStyle: "medium",
											timeStyle: "short",
										})}
									</TableCell>

									{/* Link */}
									<TableCell className="py-3 text-right pr-4">
										<Button
											asChild
											size="sm"
											variant="ghost"
											className="h-8 px-2 text-xs"
										>
											<Link
												to="/directory/$companyId"
												params={{ companyId: item.companyId }}
											>
												Detail
												<ExternalLink className="h-3 w-3 ml-1" />
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
				<div className="flex items-center justify-between pt-2">
					<div className="text-xs text-muted-foreground">
						Halaman <strong className="text-foreground">{page}</strong> dari{" "}
						<strong className="text-foreground">{totalPages}</strong> (Total{" "}
						{totalCount} riwayat)
					</div>
					<div className="flex items-center gap-2">
						<Button
							size="sm"
							variant="outline"
							className="h-8 px-3 text-xs"
							onClick={() => handlePageChange(page - 1)}
							disabled={page <= 1}
						>
							<ChevronLeft className="h-3.5 w-3.5 mr-1" />
							Sebelumnya
						</Button>
						<Button
							size="sm"
							variant="outline"
							className="h-8 px-3 text-xs"
							onClick={() => handlePageChange(page + 1)}
							disabled={page >= totalPages}
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
			return (
				<Badge
					variant="outline"
					className="bg-emerald-50 text-emerald-700 border-emerald-200 dark:bg-emerald-950/40 dark:text-emerald-300 dark:border-emerald-800/60 inline-flex items-center gap-1 text-xs"
				>
					<CheckCircle2 className="h-3 w-3 text-emerald-600 dark:text-emerald-400" />
					<span>Setuju</span>
				</Badge>
			);
		case "Revisi":
			return (
				<Badge
					variant="outline"
					className="bg-amber-50 text-amber-700 border-amber-200 dark:bg-amber-950/40 dark:text-amber-300 dark:border-amber-800/60 inline-flex items-center gap-1 text-xs"
				>
					<RotateCcw className="h-3 w-3 text-amber-600 dark:text-amber-400" />
					<span>Revisi</span>
				</Badge>
			);
		case "Tolak":
			return (
				<Badge
					variant="outline"
					className="bg-rose-50 text-rose-700 border-rose-200 dark:bg-rose-950/40 dark:text-rose-300 dark:border-rose-800/60 inline-flex items-center gap-1 text-xs"
				>
					<XCircle className="h-3 w-3 text-rose-600 dark:text-rose-400" />
					<span>Tolak</span>
				</Badge>
			);
		case "Submit":
			return (
				<Badge
					variant="outline"
					className="bg-blue-50 text-blue-700 border-blue-200 dark:bg-blue-950/40 dark:text-blue-300 dark:border-blue-800/60 inline-flex items-center gap-1 text-xs"
				>
					<Send className="h-3 w-3 text-blue-600 dark:text-blue-400" />
					<span>Submit</span>
				</Badge>
			);
		case "Rework":
			return (
				<Badge
					variant="outline"
					className="bg-orange-50 text-orange-700 border-orange-200 dark:bg-orange-950/40 dark:text-orange-300 dark:border-orange-800/60 inline-flex items-center gap-1 text-xs"
				>
					<Undo2 className="h-3 w-3 text-orange-600 dark:text-orange-400" />
					<span>Rework</span>
				</Badge>
			);
		case "Discontinue":
			return (
				<Badge
					variant="outline"
					className="bg-slate-100 text-slate-700 border-slate-300 dark:bg-slate-800 dark:text-slate-300 dark:border-slate-700 inline-flex items-center gap-1 text-xs"
				>
					<AlertOctagon className="h-3 w-3 text-slate-600 dark:text-slate-400" />
					<span>Discontinue</span>
				</Badge>
			);
		default:
			return (
				<Badge variant="secondary" className="text-xs">
					{action}
				</Badge>
			);
	}
}
