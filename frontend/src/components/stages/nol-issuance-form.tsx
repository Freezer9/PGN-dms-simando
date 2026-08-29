import { useQueryClient } from "@tanstack/react-query";
import {
	AlertTriangle,
	Award,
	CheckCircle,
	FileBadge,
	Loader2,
	Plus,
	Save,
	ScrollText,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	NolIssuanceApprovedTermDetail,
	NolIssuanceDetail,
	NolOutcome,
	SaveNolIssuanceRequest,
} from "@/api/types";
import { DocumentDownloadButton } from "@/components/documents/document-download-buttons";
import { Button } from "@/components/ui/button";
import {
	Card,
	CardContent,
	CardDescription,
	CardHeader,
	CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
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

interface NolIssuanceFormProps {
	companyId: string;
	initialData?: NolIssuanceDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
}

export function NolIssuanceForm({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
}: NolIssuanceFormProps) {
	const queryClient = useQueryClient();

	// Form State
	const [outcome, setOutcome] = React.useState<NolOutcome>(
		initialData?.outcome || "Nol",
	);
	const [nomorNotaDinas, setNomorNotaDinas] = React.useState<string>(
		initialData?.nomorNotaDinas || "",
	);
	const [berlakuSejak, setBerlakuSejak] = React.useState<string>(
		initialData?.berlakuSejak || "",
	);
	const [berlakuSampai, setBerlakuSampai] = React.useState<string>(
		initialData?.berlakuSampai || "",
	);

	// Conditional Terms (List of strings)
	const [kontrakBersyarat, setKontrakBersyarat] = React.useState<string[]>(
		initialData?.kontrakBersyarat || [],
	);

	// Repeating Approved Terms
	const [approvedTerms, setApprovedTerms] = React.useState<
		NolIssuanceApprovedTermDetail[]
	>(
		initialData?.approvedTerms?.map((t, idx) => ({
			id: t.id || crypto.randomUUID(),
			periodeMulai: t.periodeMulai,
			periodeSelesai: t.periodeSelesai,
			rataRata: Number(t.rataRata) || 0,
			kontrakMinimum: Number(t.kontrakMinimum) || 0,
			kontrakMaksimum: Number(t.kontrakMaksimum) || 0,
			sortOrder: idx + 1,
		})) || [],
	);

	// Sync initialData
	React.useEffect(() => {
		if (initialData) {
			setOutcome(initialData.outcome || "Nol");
			setNomorNotaDinas(initialData.nomorNotaDinas || "");
			setBerlakuSejak(initialData.berlakuSejak || "");
			setBerlakuSampai(initialData.berlakuSampai || "");
			setKontrakBersyarat(initialData.kontrakBersyarat || []);

			if (initialData.approvedTerms) {
				setApprovedTerms(
					initialData.approvedTerms.map((t, idx) => ({
						id: t.id || crypto.randomUUID(),
						periodeMulai: t.periodeMulai,
						periodeSelesai: t.periodeSelesai,
						rataRata: Number(t.rataRata) || 0,
						kontrakMinimum: Number(t.kontrakMinimum) || 0,
						kontrakMaksimum: Number(t.kontrakMaksimum) || 0,
						sortOrder: idx + 1,
					})),
				);
			}
		}
	}, [initialData]);

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-issuance",
		{
			onSuccess: () => {
				toast.success("Penerbitan Surat NOL/RL berhasil disimpan!");
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}",
						{ params: { path: { id: companyId } } },
					],
				});
				queryClient.invalidateQueries({
					queryKey: [
						"get",
						"/api/companies/{id}/nol-issuance",
						{ params: { path: { id: companyId } } },
					],
				});
				onSaved?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menyimpan Penerbitan NOL",
				);
			},
		},
	);

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveNolIssuanceRequest = {
			outcome,
			nomorNotaDinas: nomorNotaDinas || null,
			kontrakBersyarat,
			berlakuSejak: berlakuSejak || null,
			berlakuSampai: berlakuSampai || null,
			documentId: initialData?.documentId || null,
			approvedTerms: approvedTerms.map((t, idx) => ({
				id: t.id || crypto.randomUUID(),
				periodeMulai: t.periodeMulai,
				periodeSelesai: t.periodeSelesai,
				rataRata: Number(t.rataRata) || 0,
				kontrakMinimum: Number(t.kontrakMinimum) || 0,
				kontrakMaksimum: Number(t.kontrakMaksimum) || 0,
				sortOrder: idx + 1,
			})),
		};

		saveMutation.mutate({
			params: { path: { id: companyId } },
			body: request,
		});
	};

	const addTermRow = () => {
		setApprovedTerms([
			...approvedTerms,
			{
				id: crypto.randomUUID(),
				periodeMulai: "",
				periodeSelesai: "",
				rataRata: 0,
				kontrakMinimum: 0,
				kontrakMaksimum: 0,
				sortOrder: approvedTerms.length + 1,
			},
		]);
	};

	const removeTermRow = (index: number) => {
		setApprovedTerms(approvedTerms.filter((_, i) => i !== index));
	};

	const addConditionRow = () => {
		setKontrakBersyarat([...kontrakBersyarat, ""]);
	};

	const removeConditionRow = (index: number) => {
		setKontrakBersyarat(kontrakBersyarat.filter((_, i) => i !== index));
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
			{/* Top Bar Summary / Save */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
				<div className="flex items-center gap-3">
					<div className="size-10 rounded-full bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 flex items-center justify-center">
						<Award className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Penerbitan Surat Kesiapan Pasokan Gas (Surat NOL / RL)
						</h3>
						<p className="text-xs text-muted-foreground">
							Surat resmi persetujuan pasokan gas bumi (NOL) atau surat
							tanggapan (RL) beserta ketentuan akhir
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="nol-issuance"
						label="Unduh Surat Penerbitan (.docx)"
					/>
					{canEdit && (
						<Button
							type="submit"
							size="sm"
							disabled={saveMutation.isPending}
							className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
						>
							{saveMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Save className="size-3.5" />
							)}
							Simpan Penerbitan
						</Button>
					)}
				</div>
			</div>

			{/* SECTION 1: KEPUTUSAN & NOMOR NOTA DINAS */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<FileBadge className="size-4 text-emerald-500" />
						1. Keputusan Akhir & Administrasi Surat Resmi
					</CardTitle>
					<CardDescription className="text-xs">
						Status penerbitan surat NOL (Notice of Letter) / RL (Response
						Letter) dan nota dinas divisi
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Hasil / Outcome */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Keputusan Akhir (Outcome)
							</Label>
							<Select
								value={outcome}
								onValueChange={(val) => setOutcome(val as NolOutcome)}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9 font-semibold">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="Nol">
										<div className="flex items-center gap-2 text-emerald-600">
											<CheckCircle className="size-3.5" /> Diterbitkan Surat NOL
											(Approved)
										</div>
									</SelectItem>
									<SelectItem value="Rl">
										<div className="flex items-center gap-2 text-amber-600">
											<AlertTriangle className="size-3.5" /> Diterbitkan Surat
											RL (Response Letter)
										</div>
									</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nomor Nota Dinas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nomor Nota Dinas Divisi
							</Label>
							<Input
								value={nomorNotaDinas}
								onChange={(e) => setNomorNotaDinas(e.target.value)}
								placeholder="contoh: ND-108/PGN/COM/2026"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>
					</div>

					{/* Masa Berlaku Surat */}
					<div className="pt-2 border-t">
						<Label className="text-xs font-semibold mb-2 block text-muted-foreground">
							Masa Berlaku Surat Kesiapan Gas (NOL Validity)
						</Label>
						<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
							<div className="space-y-1">
								<Label className="text-[11px]">Berlaku Sejak</Label>
								<Input
									type="date"
									value={berlakuSejak}
									onChange={(e) => setBerlakuSejak(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label className="text-[11px]">
									Berlaku Sampai (Maks 6 Bulan)
								</Label>
								<Input
									type="date"
									value={berlakuSampai}
									onChange={(e) => setBerlakuSampai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: APPROVED TERMS TABLE */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold">
							2. Ketentuan Volume Gas yang Disetujui (Approved Terms)
						</CardTitle>
						<CardDescription className="text-xs">
							Volume pasokan terjamin per periode kontrak (MMBTUD)
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={addTermRow}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" /> Tambah Ketentuan
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<div className="border rounded-lg overflow-x-auto">
						<Table>
							<TableHeader className="bg-muted/50">
								<TableRow>
									<TableHead className="text-xs font-semibold min-w-[130px]">
										Periode Mulai
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[130px]">
										Periode Selesai
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[130px]">
										Vol Rata-rata
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[120px]">
										Vol Minimum
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[120px]">
										Vol Maksimum
									</TableHead>
									{canEdit && (
										<TableHead className="text-xs font-semibold w-12 text-center">
											Aksi
										</TableHead>
									)}
								</TableRow>
							</TableHeader>
							<TableBody>
								{approvedTerms.length === 0 ? (
									<TableRow>
										<TableCell
											colSpan={canEdit ? 6 : 5}
											className="h-20 text-center text-xs text-muted-foreground"
										>
											Belum ada ketentuan volume. Klik "+ Tambah Ketentuan"
											untuk menambahkan.
										</TableCell>
									</TableRow>
								) : (
									approvedTerms.map((row, idx) => (
										// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
										<TableRow key={idx}>
											<TableCell>
												<Input
													type="date"
													value={row.periodeMulai ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].periodeMulai = e.target.value;
														setApprovedTerms(next);
													}}
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="date"
													value={row.periodeSelesai ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].periodeSelesai = e.target.value;
														setApprovedTerms(next);
													}}
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.rataRata ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].rataRata = e.target.value
															? Number(e.target.value)
															: 0;
														setApprovedTerms(next);
													}}
													placeholder="1000"
													disabled={!canEdit}
													className="text-xs h-8 font-mono font-medium"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.kontrakMinimum ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].kontrakMinimum = e.target.value
															? Number(e.target.value)
															: 0;
														setApprovedTerms(next);
													}}
													placeholder="800"
													disabled={!canEdit}
													className="text-xs h-8 font-mono"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.kontrakMaksimum ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].kontrakMaksimum = e.target.value
															? Number(e.target.value)
															: 0;
														setApprovedTerms(next);
													}}
													placeholder="1200"
													disabled={!canEdit}
													className="text-xs h-8 font-mono"
												/>
											</TableCell>
											{canEdit && (
												<TableCell className="text-center">
													<Button
														type="button"
														variant="ghost"
														size="icon"
														onClick={() => removeTermRow(idx)}
														className="size-7 text-destructive hover:text-destructive"
													>
														<Trash2 className="size-3.5" />
													</Button>
												</TableCell>
											)}
										</TableRow>
									))
								)}
							</TableBody>
						</Table>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: SYARAT & KETENTUAN TAMBAHAN (KONTRAK BERSYARAT) */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<ScrollText className="size-4 text-amber-500" />
							3. Syarat & Ketentuan Tambahan (Kontrak Bersyarat)
						</CardTitle>
						<CardDescription className="text-xs">
							Klausul khusus yang wajib dipenuhi pelanggan sebelum gas in /
							pengaliran
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={addConditionRow}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" /> Tambah Klausul Syarat
						</Button>
					)}
				</CardHeader>
				<CardContent className="space-y-3">
					{kontrakBersyarat.length === 0 ? (
						<p className="text-xs text-muted-foreground text-center py-4">
							Tidak ada klausul syarat tambahan khusus.
						</p>
					) : (
						kontrakBersyarat.map((item, idx) => (
							// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
							<div key={idx} className="flex items-center gap-2">
								<span className="text-xs font-mono text-muted-foreground w-6 text-right">
									{idx + 1}.
								</span>
								<Input
									value={item}
									onChange={(e) => {
										const next = [...kontrakBersyarat];
										next[idx] = e.target.value;
										setKontrakBersyarat(next);
									}}
									placeholder="contoh: Pelanggan wajib menyerahkan Jaminan Pembayaran 14 hari sebelum gas in"
									disabled={!canEdit}
									className="text-xs h-8 flex-1"
								/>
								{canEdit && (
									<Button
										type="button"
										variant="ghost"
										size="icon"
										onClick={() => removeConditionRow(idx)}
										className="size-7 text-destructive"
									>
										<Trash2 className="size-3.5" />
									</Button>
								)}
							</div>
						))
					)}
				</CardContent>
			</Card>

			{canEdit && (
				<div className="flex justify-end pt-2">
					<Button
						type="submit"
						disabled={saveMutation.isPending}
						className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
					>
						{saveMutation.isPending ? (
							<Loader2 className="size-3.5 animate-spin" />
						) : (
							<Save className="size-3.5" />
						)}
						Simpan Penerbitan
					</Button>
				</div>
			)}
		</form>
	);
}
