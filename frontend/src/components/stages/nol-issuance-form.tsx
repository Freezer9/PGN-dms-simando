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
	XCircle,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	NolIssuanceDetail,
	NolOutcome,
	SaveNolIssuanceApprovedTermRequest,
	SaveNolIssuanceRequest,
	SkemaHarga,
} from "@/api/types";
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
import { Textarea } from "@/components/ui/textarea";

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
	const { data: units } = $api.useQuery("get", "/api/master/units");

	// Form State
	const [outcome, setOutcome] = React.useState<NolOutcome | "">(
		initialData?.outcome || "Diterbitkan",
	);
	const [nomorSuratNol, setNomorSuratNol] = React.useState<string>(
		initialData?.nomorSuratNol || "",
	);
	const [tanggalSuratNol, setTanggalSuratNol] = React.useState<string>(
		initialData?.tanggalSuratNol || "",
	);
	const [nomorNotaDinas, setNomorNotaDinas] = React.useState<string>(
		initialData?.nomorNotaDinas || "",
	);
	const [tanggalNotaDinas, setTanggalNotaDinas] = React.useState<string>(
		initialData?.tanggalNotaDinas || "",
	);
	const [jangkaWaktuTahun, setJangkaWaktuTahun] = React.useState<string>(
		initialData?.jangkaWaktuTahun ? String(initialData.jangkaWaktuTahun) : "",
	);
	const [skemaHarga, setSkemaHarga] = React.useState<SkemaHarga | "">(
		initialData?.skemaHarga || "Reguler",
	);
	const [hargaJualNilai, setHargaJualNilai] = React.useState<string>(
		initialData?.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
	);
	const [berlakuSejak, setBerlakuSejak] = React.useState<string>(
		initialData?.berlakuSejak || "",
	);
	const [berlakuSampai, setBerlakuSampai] = React.useState<string>(
		initialData?.berlakuSampai || "",
	);
	const [keterangan, setKeterangan] = React.useState<string>(
		initialData?.keterangan || "",
	);

	// Conditional Terms (List of strings)
	const [syaratKetentuan, setSyaratKetentuan] = React.useState<string[]>(
		initialData?.syaratKetentuanTambahan || [],
	);

	// Repeating Approved Terms
	const [approvedTerms, setApprovedTerms] = React.useState<
		SaveNolIssuanceApprovedTermRequest[]
	>(
		initialData?.approvedTerms?.map((t) => ({
			periodeMulai: t.periodeMulai || undefined,
			periodeSelesai: t.periodeSelesai || undefined,
			volumeRataRata:
				t.volumeRataRata != null ? Number(t.volumeRataRata) : undefined,
			volumeMin: t.volumeMin != null ? Number(t.volumeMin) : undefined,
			volumeMax: t.volumeMax != null ? Number(t.volumeMax) : undefined,
			unitId: t.unitId || undefined,
		})) || [],
	);

	// Sync initialData
	React.useEffect(() => {
		if (initialData) {
			setOutcome(initialData.outcome || "Diterbitkan");
			setNomorSuratNol(initialData.nomorSuratNol || "");
			setTanggalSuratNol(initialData.tanggalSuratNol || "");
			setNomorNotaDinas(initialData.nomorNotaDinas || "");
			setTanggalNotaDinas(initialData.tanggalNotaDinas || "");
			setJangkaWaktuTahun(
				initialData.jangkaWaktuTahun
					? String(initialData.jangkaWaktuTahun)
					: "",
			);
			setSkemaHarga(initialData.skemaHarga || "Reguler");
			setHargaJualNilai(
				initialData.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
			);
			setBerlakuSejak(initialData.berlakuSejak || "");
			setBerlakuSampai(initialData.berlakuSampai || "");
			setKeterangan(initialData.keterangan || "");
			setSyaratKetentuan(initialData.syaratKetentuanTambahan || []);

			if (initialData.approvedTerms) {
				setApprovedTerms(
					initialData.approvedTerms.map((t) => ({
						periodeMulai: t.periodeMulai || undefined,
						periodeSelesai: t.periodeSelesai || undefined,
						volumeRataRata:
							t.volumeRataRata != null ? Number(t.volumeRataRata) : undefined,
						volumeMin: t.volumeMin != null ? Number(t.volumeMin) : undefined,
						volumeMax: t.volumeMax != null ? Number(t.volumeMax) : undefined,
						unitId: t.unitId || undefined,
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
				toast.success("Penerbitan Surat NOL berhasil disimpan!");
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
			outcome: outcome ? (outcome as NolOutcome) : null,
			nomorSuratNol: nomorSuratNol || null,
			tanggalSuratNol: tanggalSuratNol || null,
			nomorNotaDinas: nomorNotaDinas || null,
			tanggalNotaDinas: tanggalNotaDinas || null,
			jangkaWaktuTahun: jangkaWaktuTahun ? Number(jangkaWaktuTahun) : null,
			skemaHarga: skemaHarga ? (skemaHarga as SkemaHarga) : null,
			hargaJualNilai: hargaJualNilai ? Number(hargaJualNilai) : null,
			berlakuSejak: berlakuSejak || null,
			berlakuSampai: berlakuSampai || null,
			syaratKetentuanTambahan: syaratKetentuan,
			keterangan: keterangan || null,
			approvedTerms,
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
				periodeMulai: undefined,
				periodeSelesai: undefined,
				volumeRataRata: undefined,
				volumeMin: undefined,
				volumeMax: undefined,
				unitId: undefined,
			},
		]);
	};

	const removeTermRow = (index: number) => {
		setApprovedTerms(approvedTerms.filter((_, i) => i !== index));
	};

	const addConditionRow = () => {
		setSyaratKetentuan([...syaratKetentuan, ""]);
	};

	const removeConditionRow = (index: number) => {
		setSyaratKetentuan(syaratKetentuan.filter((_, i) => i !== index));
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
							Penerbitan Surat Kesiapan Pasokan Gas (Surat NOL)
						</h3>
						<p className="text-xs text-muted-foreground">
							Surat resmi persetujuan pasokan gas bumi, nomor nota dinas, dan
							ketentuan komitmen akhir
						</p>
					</div>
				</div>

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
						Simpan Penerbitan NOL
					</Button>
				)}
			</div>

			{/* SECTION 1: KEPUTUSAN & NOMOR SURAT RESMI */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<FileBadge className="size-4 text-emerald-500" />
						1. Keputusan Akhir & Administrasi Surat Resmi
					</CardTitle>
					<CardDescription className="text-xs">
						Status penerbitan surat NOL, nomor surat resmi PGN, dan nota dinas
						divisi
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
								value={outcome || "Diterbitkan"}
								onValueChange={(val) => setOutcome(val as NolOutcome)}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9 font-semibold">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="Diterbitkan">
										<div className="flex items-center gap-2 text-emerald-600">
											<CheckCircle className="size-3.5" /> Diterbitkan
											(Approved)
										</div>
									</SelectItem>
									<SelectItem value="Bersyarat">
										<div className="flex items-center gap-2 text-amber-600">
											<AlertTriangle className="size-3.5" /> Diterbitkan
											Bersyarat (Conditional)
										</div>
									</SelectItem>
									<SelectItem value="Ditolak">
										<div className="flex items-center gap-2 text-destructive">
											<XCircle className="size-3.5" /> Ditolak (Rejected)
										</div>
									</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nomor Surat NOL */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nomor Surat Resmi NOL
							</Label>
							<Input
								value={nomorSuratNol}
								onChange={(e) => setNomorSuratNol(e.target.value)}
								placeholder="contoh: 014200.S/LG.01.01/PGN/2026"
								disabled={!canEdit}
								className="text-xs h-9 font-mono font-semibold"
							/>
						</div>

						{/* Tanggal Surat NOL */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tanggal Surat NOL</Label>
							<Input
								type="date"
								value={tanggalSuratNol}
								onChange={(e) => setTanggalSuratNol(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
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

						{/* Tanggal Nota Dinas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tanggal Nota Dinas</Label>
							<Input
								type="date"
								value={tanggalNotaDinas}
								onChange={(e) => setTanggalNotaDinas(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jangka Waktu (Tahun) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Jangka Waktu Kontrak (Tahun)
							</Label>
							<Input
								type="number"
								value={jangkaWaktuTahun}
								onChange={(e) => setJangkaWaktuTahun(e.target.value)}
								placeholder="contoh: 5"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Skema Harga */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Skema Harga yang Disetujui
							</Label>
							<Select
								value={skemaHarga || "NONE"}
								onValueChange={(val) =>
									setSkemaHarga(val === "NONE" ? "" : (val as SkemaHarga))
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Skema Harga" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Dipilih</SelectItem>
									<SelectItem value="Reguler">Reguler</SelectItem>
									<SelectItem value="SiGas">SiGas</SelectItem>
									<SelectItem value="Bersyarat">Bersyarat</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Harga Jual Disetujui */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Harga Jual Disetujui (USD/MMBTU)
							</Label>
							<Input
								type="number"
								step="0.001"
								value={hargaJualNilai}
								onChange={(e) => setHargaJualNilai(e.target.value)}
								placeholder="contoh: 9.85"
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
							Volume pasokan terjamin per periode kontrak
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
									<TableHead className="text-xs font-semibold min-w-[120px]">
										Satuan
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
											colSpan={canEdit ? 7 : 6}
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
														next[idx].periodeMulai =
															e.target.value || undefined;
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
														next[idx].periodeSelesai =
															e.target.value || undefined;
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
													value={row.volumeRataRata ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].volumeRataRata = e.target.value
															? Number(e.target.value)
															: undefined;
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
													value={row.volumeMin ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].volumeMin = e.target.value
															? Number(e.target.value)
															: undefined;
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
													value={row.volumeMax ?? ""}
													onChange={(e) => {
														const next = [...approvedTerms];
														next[idx].volumeMax = e.target.value
															? Number(e.target.value)
															: undefined;
														setApprovedTerms(next);
													}}
													placeholder="1200"
													disabled={!canEdit}
													className="text-xs h-8 font-mono"
												/>
											</TableCell>
											<TableCell>
												<Select
													value={row.unitId || "NONE"}
													onValueChange={(val) => {
														const next = [...approvedTerms];
														next[idx].unitId = val === "NONE" ? undefined : val;
														setApprovedTerms(next);
													}}
													disabled={!canEdit}
												>
													<SelectTrigger className="text-xs h-8">
														<SelectValue placeholder="Satuan" />
													</SelectTrigger>
													<SelectContent>
														<SelectItem value="NONE">-</SelectItem>
														{units?.map((u) => (
															<SelectItem key={u.id} value={u.id}>
																{u.name} ({u.code})
															</SelectItem>
														))}
													</SelectContent>
												</Select>
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

			{/* SECTION 3: SYARAT & KETENTUAN TAMBAHAN (CONDITIONAL TERMS) */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<ScrollText className="size-4 text-amber-500" />
							3. Syarat & Ketentuan Tambahan (Conditional Terms)
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
					{syaratKetentuan.length === 0 ? (
						<p className="text-xs text-muted-foreground text-center py-4">
							Tidak ada klausul syarat tambahan khusus.
						</p>
					) : (
						syaratKetentuan.map((item, idx) => (
							// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
							<div key={idx} className="flex items-center gap-2">
								<span className="text-xs font-mono text-muted-foreground w-6 text-right">
									{idx + 1}.
								</span>
								<Input
									value={item}
									onChange={(e) => {
										const next = [...syaratKetentuan];
										next[idx] = e.target.value;
										setSyaratKetentuan(next);
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

			{/* Catatan / Keterangan */}
			<div className="space-y-1.5">
				<Label className="text-xs font-medium">
					Catatan / Keterangan Penutup
				</Label>
				<Textarea
					placeholder="Catatan penutup penerbitan surat NOL..."
					value={keterangan}
					onChange={(e) => setKeterangan(e.target.value)}
					disabled={!canEdit}
					className="text-xs min-h-[60px]"
				/>
			</div>

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
						Simpan Penerbitan NOL
					</Button>
				</div>
			)}
		</form>
	);
}
