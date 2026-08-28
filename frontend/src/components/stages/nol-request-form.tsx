import { useQueryClient } from "@tanstack/react-query";
import {
	Coins,
	FileCheck,
	FileText,
	Layers,
	Loader2,
	Plus,
	Save,
	Send,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	NolRequestDetail,
	RegistrationType,
	SaveNolRequestDailyRequest,
	SaveNolRequestPeriodRequest,
	SaveNolRequestRequest,
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
import { Checkbox } from "@/components/ui/checkbox";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from "@/components/ui/select";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";

interface NolRequestFormProps {
	companyId: string;
	initialData?: NolRequestDetail | null;
	canEdit?: boolean;
	canSubmit?: boolean;
	onSaved?: () => void;
	onSubmitted?: () => void;
}

const DAY_NAMES: Record<number, string> = {
	1: "Senin",
	2: "Selasa",
	3: "Rabu",
	4: "Kamis",
	5: "Jumat",
	6: "Sabtu",
	0: "Minggu",
};

export function NolRequestForm({
	companyId,
	initialData,
	canEdit = true,
	canSubmit = false,
	onSaved,
	onSubmitted,
}: NolRequestFormProps) {
	const queryClient = useQueryClient();
	const { data: refDocs } = $api.useQuery(
		"get",
		"/api/master/reference-documents",
	);

	// Form State
	const [nomorNotaDinas, setNomorNotaDinas] = React.useState<string>(
		initialData?.nomorNotaDinas || "",
	);
	const [tanggalNotaDinas, setTanggalNotaDinas] = React.useState<string>(
		initialData?.tanggalNotaDinas || "",
	);
	const [registrationType, setRegistrationType] = React.useState<
		RegistrationType | ""
	>(initialData?.registrationType || "PelangganBaru");
	const [isSameWithA1, setIsSameWithA1] = React.useState<boolean>(
		initialData?.isSameWithA1 ?? true,
	);
	const [skemaHarga, setSkemaHarga] = React.useState<SkemaHarga | "">(
		initialData?.skemaHarga || "Reguler",
	);
	const [hargaJualNilai, setHargaJualNilai] = React.useState<string>(
		initialData?.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
	);
	const [jangkaWaktuTahun, setJangkaWaktuTahun] = React.useState<string>(
		initialData?.jangkaWaktuTahun ? String(initialData.jangkaWaktuTahun) : "",
	);
	const [namaPimpinan, setNamaPimpinan] = React.useState<string>(
		initialData?.namaPimpinan || "",
	);
	const [jabatanPimpinan, setJabatanPimpinan] = React.useState<string>(
		initialData?.jabatanPimpinan || "",
	);

	// Connection Costs
	const [biayaCapexPreGr3, setBiayaCapexPreGr3] = React.useState<string>(
		initialData?.biayaPenyambunganCapexPreGr3
			? String(initialData.biayaPenyambunganCapexPreGr3)
			: "",
	);
	const [biayaReguler, setBiayaReguler] = React.useState<string>(
		initialData?.biayaPenyambunganReguler
			? String(initialData.biayaPenyambunganReguler)
			: "",
	);
	const [biayaExtra, setBiayaExtra] = React.useState<string>(
		initialData?.biayaPenyambunganExtra
			? String(initialData.biayaPenyambunganExtra)
			: "",
	);
	const [biayaPenyambunganManual, setBiayaPenyambunganManual] =
		React.useState<string>(
			initialData?.biayaPenyambungan
				? String(initialData.biayaPenyambungan)
				: "",
		);

	const [keterangan, setKeterangan] = React.useState<string>(
		initialData?.keterangan || "",
	);

	// Selected Reference Docs
	const [selectedDocIds, setSelectedDocIds] = React.useState<string[]>(
		initialData?.referenceDocuments?.map((d) => d.referenceDocumentId) || [],
	);

	// Repeating Periods
	const [periods, setPeriods] = React.useState<SaveNolRequestPeriodRequest[]>(
		initialData?.periods?.map((p) => ({
			periodeMulai: p.periodeMulai || undefined,
			periodeSelesai: p.periodeSelesai || undefined,
			volumeRataRata:
				p.volumeRataRata != null ? Number(p.volumeRataRata) : undefined,
			volumeMin: p.volumeMin != null ? Number(p.volumeMin) : undefined,
			volumeMax: p.volumeMax != null ? Number(p.volumeMax) : undefined,
			unitId: p.unitId || undefined,
		})) || [],
	);

	// Daily Periods
	const [dailyPeriods, setDailyPeriods] = React.useState<
		SaveNolRequestDailyRequest[]
	>(
		initialData?.dailyPeriods?.map((d) => ({
			dayOfWeek: d.dayOfWeek,
			volumeMin: d.volumeMin != null ? Number(d.volumeMin) : undefined,
			volumeMax: d.volumeMax != null ? Number(d.volumeMax) : undefined,
			unitId: d.unitId || undefined,
		})) || [],
	);

	// Sync initialData
	React.useEffect(() => {
		if (initialData) {
			setNomorNotaDinas(initialData.nomorNotaDinas || "");
			setTanggalNotaDinas(initialData.tanggalNotaDinas || "");
			setRegistrationType(initialData.registrationType || "PelangganBaru");
			setIsSameWithA1(initialData.isSameWithA1 ?? true);
			setSkemaHarga(initialData.skemaHarga || "Reguler");
			setHargaJualNilai(
				initialData.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
			);
			setJangkaWaktuTahun(
				initialData.jangkaWaktuTahun
					? String(initialData.jangkaWaktuTahun)
					: "",
			);
			setNamaPimpinan(initialData.namaPimpinan || "");
			setJabatanPimpinan(initialData.jabatanPimpinan || "");
			setBiayaCapexPreGr3(
				initialData.biayaPenyambunganCapexPreGr3
					? String(initialData.biayaPenyambunganCapexPreGr3)
					: "",
			);
			setBiayaReguler(
				initialData.biayaPenyambunganReguler
					? String(initialData.biayaPenyambunganReguler)
					: "",
			);
			setBiayaExtra(
				initialData.biayaPenyambunganExtra
					? String(initialData.biayaPenyambunganExtra)
					: "",
			);
			setBiayaPenyambunganManual(
				initialData.biayaPenyambungan
					? String(initialData.biayaPenyambungan)
					: "",
			);
			setKeterangan(initialData.keterangan || "");

			if (initialData.referenceDocuments) {
				setSelectedDocIds(
					initialData.referenceDocuments.map((d) => d.referenceDocumentId),
				);
			}
			if (initialData.periods) {
				setPeriods(
					initialData.periods.map((p) => ({
						periodeMulai: p.periodeMulai || undefined,
						periodeSelesai: p.periodeSelesai || undefined,
						volumeRataRata:
							p.volumeRataRata != null ? Number(p.volumeRataRata) : undefined,
						volumeMin: p.volumeMin != null ? Number(p.volumeMin) : undefined,
						volumeMax: p.volumeMax != null ? Number(p.volumeMax) : undefined,
						unitId: p.unitId || undefined,
					})),
				);
			}
			if (initialData.dailyPeriods) {
				setDailyPeriods(
					initialData.dailyPeriods.map((d) => ({
						dayOfWeek: d.dayOfWeek,
						volumeMin: d.volumeMin != null ? Number(d.volumeMin) : undefined,
						volumeMax: d.volumeMax != null ? Number(d.volumeMax) : undefined,
						unitId: d.unitId || undefined,
					})),
				);
			}
		}
	}, [initialData]);

	// Auto compute total connection cost
	const totalConnectionCostCalculated = React.useMemo(() => {
		const pre = Number(biayaCapexPreGr3) || 0;
		const reg = Number(biayaReguler) || 0;
		const ext = Number(biayaExtra) || 0;
		return pre + reg + ext;
	}, [biayaCapexPreGr3, biayaReguler, biayaExtra]);

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-request",
		{
			onSuccess: () => {
				toast.success("Data Permohonan NOL berhasil disimpan!");
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
						"/api/companies/{id}/nol-request",
						{ params: { path: { id: companyId } } },
					],
				});
				onSaved?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menyimpan Permohonan NOL",
				);
			},
		},
	);

	// Submit Workflow Mutation
	const submitWorkflowMutation = $api.useMutation(
		"post",
		"/api/companies/{id}/workflow/start",
		{
			onSuccess: (res) => {
				toast.success(
					`Permohonan NOL berhasil diajukan! Status: ${res.currentStatus}`,
				);
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
						"/api/companies/{id}/nol-request",
						{ params: { path: { id: companyId } } },
					],
				});
				onSubmitted?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal mengajukan Permohonan NOL",
				);
			},
		},
	);

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const totalCost =
			totalConnectionCostCalculated > 0
				? totalConnectionCostCalculated
				: biayaPenyambunganManual
					? Number(biayaPenyambunganManual)
					: null;

		const request: SaveNolRequestRequest = {
			nomorNotaDinas: nomorNotaDinas || null,
			tanggalNotaDinas: tanggalNotaDinas || null,
			registrationType: registrationType
				? (registrationType as RegistrationType)
				: null,
			isSameWithA1,
			skemaHarga: skemaHarga ? (skemaHarga as SkemaHarga) : null,
			hargaJualNilai: hargaJualNilai ? Number(hargaJualNilai) : null,
			jangkaWaktuTahun: jangkaWaktuTahun ? Number(jangkaWaktuTahun) : null,
			namaPimpinan: namaPimpinan || null,
			jabatanPimpinan: jabatanPimpinan || null,
			biayaPenyambungan: totalCost,
			biayaPenyambunganCapexPreGr3: biayaCapexPreGr3
				? Number(biayaCapexPreGr3)
				: null,
			biayaPenyambunganReguler: biayaReguler ? Number(biayaReguler) : null,
			biayaPenyambunganExtra: biayaExtra ? Number(biayaExtra) : null,
			keterangan: keterangan || null,
			selectedDocIds,
			periods,
			dailyPeriods,
		};

		saveMutation.mutate({
			params: { path: { id: companyId } },
			body: request,
		});
	};

	const addPeriodRow = () => {
		setPeriods([
			...periods,
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
	const removePeriodRow = (index: number) => {
		setPeriods(periods.filter((_, i) => i !== index));
	};

	const addDailyRow = () => {
		setDailyPeriods([
			...dailyPeriods,
			{
				dayOfWeek: 1,
				volumeMin: undefined,
				volumeMax: undefined,
				unitId: undefined,
			},
		]);
	};
	const removeDailyRow = (index: number) => {
		setDailyPeriods(dailyPeriods.filter((_, i) => i !== index));
	};

	const toggleDoc = (docId: string) => {
		if (selectedDocIds.includes(docId)) {
			setSelectedDocIds(selectedDocIds.filter((id) => id !== docId));
		} else {
			setSelectedDocIds([...selectedDocIds, docId]);
		}
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
			{/* Top Bar Summary / Save / Submit */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
				<div className="flex items-center gap-3">
					<div className="size-10 rounded-full bg-amber-100 dark:bg-amber-900/40 text-amber-600 flex items-center justify-center">
						<FileCheck className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Permohonan Surat Kesiapan Gas / Notice of Letter (NOL)
						</h3>
						<p className="text-xs text-muted-foreground">
							Pengajuan komersial, nota dinas, estimasi biaya penyambungan, dan
							dokumen referensi
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					{canEdit && (
						<Button
							type="submit"
							size="sm"
							disabled={saveMutation.isPending}
							className="h-9 text-xs flex items-center gap-1.5 bg-amber-600 hover:bg-amber-700 text-white"
						>
							{saveMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Save className="size-3.5" />
							)}
							Simpan Permohonan NOL
						</Button>
					)}

					{canSubmit && (
						<Button
							type="button"
							size="sm"
							disabled={submitWorkflowMutation.isPending}
							onClick={() =>
								submitWorkflowMutation.mutate({
									params: { path: { id: companyId } },
								})
							}
							className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
						>
							{submitWorkflowMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Send className="size-3.5" />
							)}
							Ajukan ke Evaluasi NOL
						</Button>
					)}
				</div>
			</div>

			{/* SECTION 1: NOTA DINAS & INFORMASI DASAR */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<FileText className="size-4 text-amber-500" />
						1. Data Nota Dinas & Legalitas Permohonan
					</CardTitle>
					<CardDescription className="text-xs">
						Nomor nota dinas sales, jenis registrasi, dan pimpinan penandatangan
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Nomor Nota Dinas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Nomor Nota Dinas</Label>
							<Input
								value={nomorNotaDinas}
								onChange={(e) => setNomorNotaDinas(e.target.value)}
								placeholder="contoh: ND-042/PGN/SLS/2026"
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

						{/* Jenis Registrasi */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jenis Registrasi</Label>
							<Select
								value={registrationType || "NONE"}
								onValueChange={(val) =>
									setRegistrationType(
										val === "NONE" ? "" : (val as RegistrationType),
									)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Jenis Registrasi" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Dipilih</SelectItem>
									<SelectItem value="PelangganBaru">Pelanggan Baru</SelectItem>
									<SelectItem value="PerubahanVolume">
										Perubahan Volume Gas
									</SelectItem>
									<SelectItem value="PerpanjanganKontrak">
										Perpanjangan Kontrak
									</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nama Pimpinan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nama Pimpinan Pemohon
							</Label>
							<Input
								value={namaPimpinan}
								onChange={(e) => setNamaPimpinan(e.target.value)}
								placeholder="contoh: Bambang Soedirman"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jabatan Pimpinan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jabatan Pimpinan</Label>
							<Input
								value={jabatanPimpinan}
								onChange={(e) => setJabatanPimpinan(e.target.value)}
								placeholder="contoh: Presiden Direktur"
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
							<Label className="text-xs font-medium">Skema Harga Gas</Label>
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

						{/* Harga Jual Gas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Harga Jual Gas (USD/MMBTU)
							</Label>
							<Input
								type="number"
								step="0.001"
								value={hargaJualNilai}
								onChange={(e) => setHargaJualNilai(e.target.value)}
								placeholder="contoh: 9.85"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Switch Sama dengan A1 */}
						<div className="flex items-center space-x-3 pt-6">
							<Switch
								id="same-a1"
								checked={isSameWithA1}
								onCheckedChange={setIsSameWithA1}
								disabled={!canEdit}
							/>
							<div>
								<Label
									htmlFor="same-a1"
									className="text-xs font-semibold cursor-pointer"
								>
									Sesuai Data Formulir A1
								</Label>
								<p className="text-[11px] text-muted-foreground">
									Gunakan profil dan ketentuan identik dengan registrasi A1
								</p>
							</div>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: BIAYA PENYAMBUNGAN (CONNECTION CHARGES) */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Coins className="size-4 text-emerald-500" />
						2. Rincian Biaya Penyambungan Gas
					</CardTitle>
					<CardDescription className="text-xs">
						Komponen capex pipa, biaya reguler, dan biaya ekstra penyambungan
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
						{/* Capex Pre GR3 */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Capex Pre GR3 (IDR)</Label>
							<Input
								type="number"
								value={biayaCapexPreGr3}
								onChange={(e) => setBiayaCapexPreGr3(e.target.value)}
								placeholder="contoh: 50000000"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>

						{/* Biaya Reguler */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Biaya Reguler (IDR)</Label>
							<Input
								type="number"
								value={biayaReguler}
								onChange={(e) => setBiayaReguler(e.target.value)}
								placeholder="contoh: 25000000"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>

						{/* Biaya Extra */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Biaya Extra (IDR)</Label>
							<Input
								type="number"
								value={biayaExtra}
								onChange={(e) => setBiayaExtra(e.target.value)}
								placeholder="contoh: 10000000"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>

						{/* Total Biaya Penyambungan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-semibold text-emerald-700 dark:text-emerald-400">
								Total Biaya Penyambungan (IDR)
							</Label>
							<div className="h-9 px-3 border rounded-md bg-emerald-50 dark:bg-emerald-950/40 flex items-center font-mono font-bold text-xs text-emerald-700 dark:text-emerald-300">
								{totalConnectionCostCalculated > 0
									? `Rp ${totalConnectionCostCalculated.toLocaleString("id-ID")}`
									: biayaPenyambunganManual
										? `Rp ${Number(biayaPenyambunganManual).toLocaleString("id-ID")}`
										: "Rp 0"}
							</div>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: REPEATING DEMAND PERIODS & DAILY TABLE */}
			<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
				{/* Demand Periods Table */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-xs font-semibold">
								3. Periode Kebutuhan Gas
							</CardTitle>
							<CardDescription className="text-[11px]">
								Rentang waktu dan volume komitmen
							</CardDescription>
						</div>
						{canEdit && (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={addPeriodRow}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						{periods.length === 0 ? (
							<p className="text-xs text-muted-foreground text-center py-4">
								Belum ada periode kebutuhan.
							</p>
						) : (
							periods.map((row, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-2.5 border rounded-md space-y-2 bg-muted/20"
								>
									<div className="flex items-center justify-between">
										<span className="text-[11px] font-semibold text-muted-foreground">
											Periode #{idx + 1}
										</span>
										{canEdit && (
											<Button
												type="button"
												variant="ghost"
												size="icon"
												onClick={() => removePeriodRow(idx)}
												className="size-6 text-destructive"
											>
												<Trash2 className="size-3" />
											</Button>
										)}
									</div>
									<div className="grid grid-cols-2 gap-2">
										<Input
											type="date"
											value={row.periodeMulai ?? ""}
											onChange={(e) => {
												const next = [...periods];
												next[idx].periodeMulai = e.target.value || undefined;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="date"
											value={row.periodeSelesai ?? ""}
											onChange={(e) => {
												const next = [...periods];
												next[idx].periodeSelesai = e.target.value || undefined;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
									<div className="grid grid-cols-3 gap-2">
										<Input
											type="number"
											value={row.volumeRataRata ?? ""}
											onChange={(e) => {
												const next = [...periods];
												next[idx].volumeRataRata = e.target.value
													? Number(e.target.value)
													: undefined;
												setPeriods(next);
											}}
											placeholder="Vol Rerata"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											value={row.volumeMin ?? ""}
											onChange={(e) => {
												const next = [...periods];
												next[idx].volumeMin = e.target.value
													? Number(e.target.value)
													: undefined;
												setPeriods(next);
											}}
											placeholder="Vol Min"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											value={row.volumeMax ?? ""}
											onChange={(e) => {
												const next = [...periods];
												next[idx].volumeMax = e.target.value
													? Number(e.target.value)
													: undefined;
												setPeriods(next);
											}}
											placeholder="Vol Max"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
								</div>
							))
						)}
					</CardContent>
				</Card>

				{/* Daily Periods Table */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-xs font-semibold">
								4. Pola Pemakaian Harian
							</CardTitle>
							<CardDescription className="text-[11px]">
								Profil beban per hari dalam seminggu
							</CardDescription>
						</div>
						{canEdit && (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={addDailyRow}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						{dailyPeriods.length === 0 ? (
							<p className="text-xs text-muted-foreground text-center py-4">
								Belum ada pola harian.
							</p>
						) : (
							dailyPeriods.map((row, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-2.5 border rounded-md space-y-2 bg-muted/20"
								>
									<div className="flex items-center justify-between">
										<span className="text-[11px] font-semibold text-muted-foreground">
											Hari:{" "}
											{DAY_NAMES[row.dayOfWeek ?? 1] || `Hari ${row.dayOfWeek}`}
										</span>
										{canEdit && (
											<Button
												type="button"
												variant="ghost"
												size="icon"
												onClick={() => removeDailyRow(idx)}
												className="size-6 text-destructive"
											>
												<Trash2 className="size-3" />
											</Button>
										)}
									</div>
									<div className="grid grid-cols-3 gap-2">
										<Select
											value={String(row.dayOfWeek ?? 1)}
											onValueChange={(val) => {
												const next = [...dailyPeriods];
												next[idx].dayOfWeek = Number(val);
												setDailyPeriods(next);
											}}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-7">
												<SelectValue />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="1">Senin</SelectItem>
												<SelectItem value="2">Selasa</SelectItem>
												<SelectItem value="3">Rabu</SelectItem>
												<SelectItem value="4">Kamis</SelectItem>
												<SelectItem value="5">Jumat</SelectItem>
												<SelectItem value="6">Sabtu</SelectItem>
												<SelectItem value="0">Minggu</SelectItem>
											</SelectContent>
										</Select>

										<Input
											type="number"
											value={row.volumeMin ?? ""}
											onChange={(e) => {
												const next = [...dailyPeriods];
												next[idx].volumeMin = e.target.value
													? Number(e.target.value)
													: undefined;
												setDailyPeriods(next);
											}}
											placeholder="Vol Min"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											value={row.volumeMax ?? ""}
											onChange={(e) => {
												const next = [...dailyPeriods];
												next[idx].volumeMax = e.target.value
													? Number(e.target.value)
													: undefined;
												setDailyPeriods(next);
											}}
											placeholder="Vol Max"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
								</div>
							))
						)}
					</CardContent>
				</Card>
			</div>

			{/* SECTION 4: DOKUMEN REFERENSI */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Layers className="size-4 text-purple-500" />
						5. Dokumen Acuan & Referensi
					</CardTitle>
					<CardDescription className="text-xs">
						Pilih surat atau dokumen acuan yang melandasi permohonan NOL ini
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
						{refDocs?.map((doc) => {
							const isChecked = selectedDocIds.includes(doc.id);
							return (
								<button
									key={doc.id}
									type="button"
									onClick={() => canEdit && toggleDoc(doc.id)}
									className={`flex items-start space-x-2.5 p-3 rounded-lg border text-left cursor-pointer transition-colors ${
										isChecked
											? "bg-purple-50 border-purple-300 dark:bg-purple-950/30 dark:border-purple-800"
											: "hover:bg-muted/40"
									}`}
								>
									<Checkbox
										checked={isChecked}
										onCheckedChange={() => canEdit && toggleDoc(doc.id)}
										disabled={!canEdit}
										className="mt-0.5 pointer-events-none"
									/>
									<div className="space-y-0.5">
										<p className="text-xs font-semibold leading-none">
											{doc.title}
										</p>
										{doc.documentNumber && (
											<p className="text-[11px] font-mono text-muted-foreground">
												{doc.documentNumber}
											</p>
										)}
									</div>
								</button>
							);
						})}
					</div>
				</CardContent>
			</Card>

			{/* Keterangan */}
			<div className="space-y-1.5">
				<Label className="text-xs font-medium">
					Catatan / Justifikasi Permohonan
				</Label>
				<Textarea
					placeholder="Tuliskan justifikasi permohonan NOL atau catatan penting lainnya..."
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
						className="h-9 text-xs flex items-center gap-1.5 bg-amber-600 hover:bg-amber-700 text-white"
					>
						{saveMutation.isPending ? (
							<Loader2 className="size-3.5 animate-spin" />
						) : (
							<Save className="size-3.5" />
						)}
						Simpan Permohonan NOL
					</Button>
				</div>
			)}
		</form>
	);
}
