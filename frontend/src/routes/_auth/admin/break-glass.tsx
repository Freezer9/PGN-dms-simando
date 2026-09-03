import { useForm } from "@tanstack/react-form";
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
import { PageHeader } from "@/components/common";
import { FormField } from "@/components/form/form-field";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Combobox } from "@/components/ui/combobox";
import {
	Dialog,
	DialogContent,
	DialogDescription,
	DialogFooter,
	DialogHeader,
	DialogTitle,
} from "@/components/ui/dialog";
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import {
	type BreakGlassRequestFormValues,
	breakGlassRequestSchema,
} from "@/lib/schemas";

export const Route = createFileRoute("/_auth/admin/break-glass")({
	component: BreakGlassPage,
});

function BreakGlassPage() {
	const [requestDialogOpen, setRequestDialogOpen] = React.useState(false);
	const [page, setPage] = React.useState(1);
	const [pageSize] = React.useState(20);
	const [error, setError] = React.useState<string | null>(null);

	// Fetch logs
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

	// Fetch company list for emergency access dropdown
	const { data: companiesData } = $api.useQuery(
		"get",
		"/api/companies",
		{
			params: {
				query: {
					pageSize: 100,
				},
			},
		},
		{
			enabled: requestDialogOpen,
		},
	);

	// Emergency request mutation
	const requestMutation = $api.useMutation(
		"post",
		"/api/admin/break-glass/request",
		{
			onSuccess: () => {
				setRequestDialogOpen(false);
				setError(null);
				refetch();
			},
			onError: (err) => {
				setError(
					err.detail ||
						err.title ||
						"Gagal mengajukan permintaan akses darurat.",
				);
			},
		},
	);

	const form = useForm({
		defaultValues: {
			companyId: "",
			reason: "",
		} as BreakGlassRequestFormValues,
		validators: {
			onSubmit: breakGlassRequestSchema,
		},
		onSubmit: async ({ value }) => {
			setError(null);
			await requestMutation.mutateAsync({
				body: {
					companyId: value.companyId,
					reason: value.reason.trim(),
				},
			});
		},
	});

	const items: BreakGlassAccessDto[] = logsData?.items || [];
	const totalCount = Number(logsData?.totalCount) || 0;
	const totalPages = Math.ceil(totalCount / pageSize) || 1;
	const companies = companiesData?.items || [];

	const handleOpenRequest = () => {
		form.reset({
			companyId: "",
			reason: "",
		});
		setError(null);
		setRequestDialogOpen(true);
	};

	return (
		<div className="space-y-6">
			{/* Page Header */}
			<PageHeader
				title="Akses Darurat (Break-Glass Protocol)"
				description="Audit log dan permohonan akses darurat bypass wewenang berkas untuk kondisi luar biasa. Seluruh aktivitas tercatat dan diaudit secara ketat."
				badge={
					<span className="p-1 rounded-md bg-destructive/10 text-destructive">
						<ShieldAlert className="h-4 w-4" />
					</span>
				}
				actions={
					<Button
						onClick={handleOpenRequest}
						variant="destructive"
						className="shadow-xs"
					>
						<KeyRound className="h-4 w-4 mr-1.5" />
						Buka Akses Darurat
					</Button>
				}
			/>

			{/* Security Notice Alert */}
			<Alert
				variant="destructive"
				className="bg-destructive/5 border-destructive/20 text-destructive"
			>
				<AlertTriangle className="h-4 w-4" />
				<AlertDescription className="text-xs">
					<strong>Pemberitahuan Keamanan:</strong> Fitur Break-Glass hanya boleh
					digunakan untuk investigasi mendesak, insiden operasional, atau audit
					kepatuhan resmi. Setiap pembukaan berkas melalui protokol ini akan
					menghasilkan log permanen yang dilaporkan kepada Dewan Pengawas
					Sistem.
				</AlertDescription>
			</Alert>

			{/* Audit Log Table Card */}
			<div className="space-y-4">
				<div className="flex items-center justify-between">
					<h3 className="text-base font-semibold text-foreground flex items-center gap-2">
						<History className="h-4 w-4 text-muted-foreground" />
						Log Aktivitas Akses Darurat
					</h3>
					<Badge variant="outline" className="font-mono text-xs">
						{totalCount} Riwayat
					</Badge>
				</div>

				<div className="rounded-xl border bg-card shadow-xs overflow-hidden">
					<Table>
						<TableHeader className="bg-muted/40">
							<TableRow>
								<TableHead className="font-semibold text-xs pl-4">
									Pengguna (Auditor)
								</TableHead>
								<TableHead className="font-semibold text-xs">
									Target Berkas
								</TableHead>
								<TableHead className="font-semibold text-xs">
									Alasan / Urgensi
								</TableHead>
								<TableHead className="font-semibold text-xs">
									Waktu Permintaan
								</TableHead>
								<TableHead className="font-semibold text-xs">
									Berakhir Pada
								</TableHead>
								<TableHead className="font-semibold text-xs text-right pr-4">
									Status Sesi
								</TableHead>
							</TableRow>
						</TableHeader>
						<TableBody>
							{isLoading ? (
								<TableRow>
									<TableCell colSpan={6} className="h-48 text-center">
										<div className="flex flex-col items-center justify-center gap-2 text-muted-foreground">
											<Loader2 className="h-6 w-6 animate-spin text-primary" />
											<span className="text-xs">
												Memuat audit log break-glass...
											</span>
										</div>
									</TableCell>
								</TableRow>
							) : items.length === 0 ? (
								<TableRow>
									<TableCell
										colSpan={6}
										className="h-48 text-center text-muted-foreground text-xs"
									>
										Belum ada riwayat pembukaan akses darurat.
									</TableCell>
								</TableRow>
							) : (
								items.map((log) => {
									const isExpired =
										!log.isActive || new Date(log.expiresAt) < new Date();

									return (
										<TableRow
											key={log.id}
											className="hover:bg-muted/30 transition-colors"
										>
											{/* User */}
											<TableCell className="py-3 pl-4">
												<div className="flex items-center gap-2">
													<div className="size-7 rounded-full bg-primary/10 text-primary flex items-center justify-center shrink-0">
														<User className="size-3.5" />
													</div>
													<div>
														<div className="font-medium text-xs text-foreground">
															{log.userName}
														</div>
														<span className="text-[10px] text-muted-foreground font-mono">
															ID: {log.userId.slice(0, 8)}
														</span>
													</div>
												</div>
											</TableCell>

											{/* Target Company */}
											<TableCell className="py-3">
												<div className="flex items-center gap-1.5 font-mono text-xs">
													<Building2 className="h-3.5 w-3.5 text-muted-foreground shrink-0" />
													<span>
														{log.companyNomor || log.companyId.slice(0, 8)} (
														{log.companyName})
													</span>
												</div>
											</TableCell>

											{/* Reason */}
											<TableCell className="py-3 text-xs max-w-xs">
												<p className="line-clamp-2 text-foreground">
													{log.reason}
												</p>
											</TableCell>

											{/* Requested At */}
											<TableCell className="py-3 text-xs text-muted-foreground">
												{new Date(log.requestedAt).toLocaleString("id-ID", {
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
							<div className="flex items-center gap-2 text-destructive">
								<ShieldAlert className="h-5 w-5" />
								<DialogTitle>
									Permohonan Akses Darurat (Break-Glass)
								</DialogTitle>
							</div>
							<DialogDescription className="text-xs">
								Akses darurat memberikan wewenang baca/tinjau sementara pada
								berkas pelanggan yang terkunci atau berada di luar wilayah Anda.
							</DialogDescription>
						</DialogHeader>

						<form
							onSubmit={(e) => {
								e.preventDefault();
								e.stopPropagation();
								form.handleSubmit();
							}}
							className="space-y-4 py-2"
						>
							{error && (
								<Alert variant="destructive" className="py-2 text-xs">
									<AlertTriangle className="h-4 w-4" />
									<AlertDescription>{error}</AlertDescription>
								</Alert>
							)}

							<form.Field name="companyId">
								{(field) => {
									const fieldError = field.state.meta.errors[0]?.message;
									const companyOptions = companies.map((c) => ({
										value: c.id,
										label: `${c.nomor} — ${c.namaPerusahaan} (${c.salesUserName || c.locationLabel || "Area"})`,
									}));

									return (
										<FormField
											label="Pilih Berkas Pelanggan"
											required
											error={fieldError}
										>
											<Combobox
												id={field.name}
												value={field.state.value}
												onValueChange={(val) => field.handleChange(val)}
												options={companyOptions}
												placeholder="-- Pilih Perusahaan --"
												searchPlaceholder="Cari nomor atau nama perusahaan..."
												emptyText="Perusahaan tidak ditemukan."
												className="h-9 text-xs"
											/>
										</FormField>
									);
								}}
							</form.Field>

							<form.Field name="reason">
								{(field) => {
									const fieldError = field.state.meta.errors[0]?.message;
									return (
										<FormField
											label="Alasan Akses Darurat"
											required
											error={fieldError}
										>
											<Textarea
												id={field.name}
												name={field.name}
												placeholder="Jelaskan alasan teknis mendesak perlunya membuka berkas ini..."
												value={field.state.value}
												onBlur={field.handleBlur}
												onChange={(e) => field.handleChange(e.target.value)}
												className="text-xs min-h-[80px]"
											/>
										</FormField>
									);
								}}
							</form.Field>

							<DialogFooter className="pt-2">
								<Button
									type="button"
									variant="outline"
									size="sm"
									onClick={() => setRequestDialogOpen(false)}
								>
									Batal
								</Button>
								<form.Subscribe
									selector={(state) => [state.canSubmit, state.isSubmitting]}
								>
									{([canSubmit, isSubmitting]) => (
										<Button
											type="submit"
											variant="destructive"
											size="sm"
											disabled={!canSubmit || isSubmitting}
										>
											{isSubmitting ? (
												<>
													<Loader2 className="h-3.5 w-3.5 animate-spin mr-1.5" />
													Memproses...
												</>
											) : (
												<>
													<KeyRound className="h-3.5 w-3.5 mr-1.5" />
													Konfirmasi Akses Darurat
												</>
											)}
										</Button>
									)}
								</form.Subscribe>
							</DialogFooter>
						</form>
					</DialogContent>
				</Dialog>
			</div>
		</div>
	);
}
