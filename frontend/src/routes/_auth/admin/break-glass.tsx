import { createFileRoute } from "@tanstack/react-router";
import {
	AlertTriangle,
	Building2,
	ChevronLeft,
	ChevronRight,
	Clock,
	History,
	KeyRound,
	Loader2,
	ShieldAlert,
	User,
} from "lucide-react";
import * as React from "react";
import { $api } from "@/api/client";
import type { BreakGlassAccessDto } from "@/api/types";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";

export const Route = createFileRoute("/_auth/admin/break-glass")({
	component: BreakGlassPage,
});

function BreakGlassPage() {
	const [requestDialogOpen, setRequestDialogOpen] = React.useState(false);
	const [companySearch, setCompanySearch] = React.useState("");
	const [selectedCompanyId, setSelectedCompanyId] = React.useState<string>("");
	const [reason, setReason] = React.useState("");
	const [page, setPage] = React.useState(1);
	const [pageSize] = React.useState(20);
	const [error, setError] = React.useState<string | null>(null);

	// Fetch audit logs
	const {
		data: logsData,
		isLoading,
		refetch,
	} = $api.useQuery("get", "/api/admin/break-glass/logs", {
		params: {
			query: {
				page,
				pageSize,
			},
		},
	});

	// Fetch companies for selection
	const { data: companiesData } = $api.useQuery("get", "/api/companies", {
		params: {
			query: {
				page: 1,
				pageSize: 50,
				search: companySearch || undefined,
			},
		},
	});

	const requestMutation = $api.useMutation(
		"post",
		"/api/admin/break-glass/request",
		{
			onSuccess: () => {
				setRequestDialogOpen(false);
				setSelectedCompanyId("");
				setReason("");
				setError(null);
				refetch();
			},
			onError: (err: unknown) => {
				const errorObj = err as { error?: string; errors?: string[] };
				setError(
					errorObj?.error ||
						errorObj?.errors?.[0] ||
						"Gagal mengajukan permintaan akses darurat.",
				);
			},
		},
	);

	const items: BreakGlassAccessDto[] = logsData?.items || [];
	const totalCount = logsData?.totalCount || 0;
	const totalPages = logsData?.totalPages || 1;
	const companies = companiesData?.items || [];

	const handleOpenRequest = () => {
		setSelectedCompanyId("");
		setReason("");
		setError(null);
		setRequestDialogOpen(true);
	};

	const handleSubmitRequest = (e: React.FormEvent) => {
		e.preventDefault();
		if (!selectedCompanyId) {
			setError("Silakan pilih perusahaan terlebih dahulu.");
			return;
		}
		if (!reason.trim()) {
			setError("Alasan akses darurat wajib diisi secara spesifik.");
			return;
		}

		requestMutation.mutate({
			body: {
				companyId: selectedCompanyId,
				reason: reason.trim(),
			},
		});
	};

	const now = Date.now();

	return (
		<div className="space-y-4">
			{/* Top Header */}
			<div className="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
				<div className="space-y-0.5">
					<h2 className="text-lg font-bold tracking-tight text-foreground flex items-center gap-2">
						<ShieldAlert className="h-5 w-5 text-destructive" />
						<span>Akses Darurat (Break-Glass) & Log Audit</span>
					</h2>
					<p className="text-xs text-muted-foreground">
						Mekanisme akses darurat berbatas waktu (60 menit) dengan pencatatan
						audit penuh untuk keperluan investigasi teknis dan dukungan sistem.
					</p>
				</div>

				<div className="flex items-center gap-2">
					<Button
						variant="destructive"
						size="sm"
						onClick={handleOpenRequest}
						className="h-9 gap-1.5 text-xs"
					>
						<KeyRound className="h-4 w-4" />
						<span>Minta Akses Darurat</span>
					</Button>
				</div>
			</div>

			{/* Info Box */}
			<Alert className="border-amber-200 bg-amber-50 dark:border-amber-800/60 dark:bg-amber-950/40 text-amber-900 dark:text-amber-200 py-2.5">
				<AlertTriangle className="h-4 w-4 text-amber-600 dark:text-amber-400 shrink-0" />
				<AlertDescription className="text-xs leading-relaxed">
					<strong>Ketentuan Keamanan:</strong> Akses break-glass hanya
					diperbolehkan saat pemecahan masalah teknis mendesak. Setiap pembacaan
					berkas dicatat permanen dalam jejak audit kepatuhan.
				</AlertDescription>
			</Alert>

			{/* Audit Log Table */}
			<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
				<Table>
					<TableHeader className="bg-muted/40">
						<TableRow>
							<TableHead className="font-semibold text-xs py-3">
								Pengguna (Admin)
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								ID / Target Perusahaan
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Alasan Permintaan
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Waktu Akses
							</TableHead>
							<TableHead className="font-semibold text-xs py-3">
								Kadaluarsa
							</TableHead>
							<TableHead className="text-right font-semibold text-xs py-3 pr-4">
								Status Sesi
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
											Memuat log audit akses darurat...
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : items.length === 0 ? (
							<TableRow>
								<TableCell colSpan={6} className="text-center py-12">
									<div className="flex flex-col items-center justify-center gap-1.5 text-muted-foreground">
										<History className="h-8 w-8 text-muted-foreground/60" />
										<span className="text-sm font-medium text-foreground">
											Belum Ada Riwayat Break-Glass
										</span>
										<span className="text-xs">
											Tidak ada aktivitas akses darurat yang tercatat.
										</span>
									</div>
								</TableCell>
							</TableRow>
						) : (
							items.map((log) => {
								const expiresTime = new Date(log.expiresAt).getTime();
								const isExpired = now >= expiresTime;
								return (
									<TableRow
										key={log.id}
										className="hover:bg-muted/30 transition-colors"
									>
										{/* User */}
										<TableCell className="py-3">
											<div className="flex items-center gap-2">
												<User className="h-3.5 w-3.5 text-primary shrink-0" />
												<div className="flex flex-col">
													<span className="font-semibold text-foreground text-xs">
														{log.userName}
													</span>
													<span className="font-mono text-[10px] text-muted-foreground">
														{log.userId.slice(0, 8)}...
													</span>
												</div>
											</div>
										</TableCell>

										{/* Target Company */}
										<TableCell className="py-3">
											<div className="flex items-center gap-1.5 font-mono text-xs">
												<Building2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
												<span>{log.companyId.slice(0, 8)}...</span>
											</div>
										</TableCell>

										{/* Reason */}
										<TableCell className="py-3 text-xs max-w-xs">
											<p className="line-clamp-2 text-foreground">
												{log.reason}
											</p>
										</TableCell>

										{/* Granted At */}
										<TableCell className="py-3 text-xs text-muted-foreground">
											{new Date(log.grantedAt).toLocaleString("id-ID", {
												dateStyle: "medium",
												timeStyle: "short",
											})}
										</TableCell>

										{/* Expires At */}
										<TableCell className="py-3 text-xs text-muted-foreground">
											{new Date(log.expiresAt).toLocaleString("id-ID", {
												dateStyle: "medium",
												timeStyle: "short",
											})}
										</TableCell>

										{/* Status */}
										<TableCell className="py-3 text-right pr-4">
											{isExpired ? (
												<Badge
													variant="outline"
													className="text-[10px] bg-muted/40 text-muted-foreground"
												>
													Kadaluarsa
												</Badge>
											) : (
												<Badge
													variant="outline"
													className="text-[10px] bg-emerald-50 text-emerald-700 border-emerald-300 dark:bg-emerald-950/40 dark:text-emerald-300 animate-pulse"
												>
													<Clock className="h-3 w-3 mr-1" />
													Sesi Aktif
												</Badge>
											)}
										</TableCell>
									</TableRow>
								);
							})
						)}
					</TableBody>
				</Table>
			</div>

			{/* Pagination */}
			{totalPages > 1 && (
				<div className="flex items-center justify-between pt-2">
					<div className="text-xs text-muted-foreground">
						Halaman <strong className="text-foreground">{page}</strong> dari{" "}
						<strong className="text-foreground">{totalPages}</strong> (Total{" "}
						{totalCount} entri)
					</div>
					<div className="flex items-center gap-2">
						<Button
							size="sm"
							variant="outline"
							className="h-8 px-3 text-xs"
							onClick={() => setPage((p) => Math.max(1, p - 1))}
							disabled={page <= 1}
						>
							<ChevronLeft className="h-3.5 w-3.5 mr-1" />
							Sebelumnya
						</Button>
						<Button
							size="sm"
							variant="outline"
							className="h-8 px-3 text-xs"
							onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
							disabled={page >= totalPages}
						>
							Berikutnya
							<ChevronRight className="h-3.5 w-3.5 ml-1" />
						</Button>
					</div>
				</div>
			)}

			{/* Request Dialog */}
			<Dialog open={requestDialogOpen} onOpenChange={setRequestDialogOpen}>
				<DialogContent className="sm:max-w-[500px]">
					<DialogHeader>
						<DialogTitle className="flex items-center gap-2 text-sm font-semibold text-destructive">
							<ShieldAlert className="h-4 w-4" />
							<span>Permintaan Akses Darurat (Break-Glass)</span>
						</DialogTitle>
						<DialogDescription className="text-xs">
							Akses darurat diberikan selama 60 menit dan seluruh aksi Anda akan
							diaudit.
						</DialogDescription>
					</DialogHeader>

					{error && (
						<Alert variant="destructive">
							<AlertDescription className="text-xs">{error}</AlertDescription>
						</Alert>
					)}

					<form onSubmit={handleSubmitRequest} className="space-y-3 py-2">
						<div className="space-y-1">
							<Label htmlFor="company-search" className="text-xs font-medium">
								Pilih Perusahaan Target{" "}
								<span className="text-destructive">*</span>
							</Label>
							<div className="space-y-1.5">
								<Input
									id="company-search"
									placeholder="Ketik untuk memfilter nama perusahaan..."
									value={companySearch}
									onChange={(e) => setCompanySearch(e.target.value)}
									className="h-8 text-xs"
								/>
								<select
									value={selectedCompanyId}
									onChange={(e) => setSelectedCompanyId(e.target.value)}
									className="w-full h-8 px-2.5 rounded-md border bg-background text-xs"
									required
								>
									<option value="">-- Pilih Perusahaan --</option>
									{companies.map((c) => (
										<option key={c.id} value={c.id}>
											{c.nomor} — {c.namaPerusahaan} (
											{c.salesAreaName || "Area"})
										</option>
									))}
								</select>
							</div>
						</div>

						<div className="space-y-1">
							<Label htmlFor="reason" className="text-xs font-medium">
								Alasan Akses Darurat <span className="text-destructive">*</span>
							</Label>
							<Textarea
								id="reason"
								placeholder="Jelaskan alasan teknis mendesak perlunya membuka berkas ini..."
								value={reason}
								onChange={(e) => setReason(e.target.value)}
								className="text-xs min-h-[80px]"
								required
							/>
						</div>

						<DialogFooter className="pt-2">
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={() => setRequestDialogOpen(false)}
								disabled={requestMutation.isPending}
							>
								Batal
							</Button>
							<Button
								type="submit"
								variant="destructive"
								size="sm"
								disabled={requestMutation.isPending}
							>
								{requestMutation.isPending ? (
									<>
										<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
										<span>Mengajukan...</span>
									</>
								) : (
									"Konfirmasi Akses Darurat"
								)}
							</Button>
						</DialogFooter>
					</form>
				</DialogContent>
			</Dialog>
		</div>
	);
}
