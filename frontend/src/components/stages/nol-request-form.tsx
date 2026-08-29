import { useQueryClient } from "@tanstack/react-query";
import {
	Coins,
	FileCheck,
	FileText,
	Layers,
	Loader2,
	Plus,
	Save,
	Trash2,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	DayOfWeek,
	NolRequestDailyDetail,
	NolRequestDetail,
	NolRequestPeriodDetail,
	RegistrationType,
	SaveNolRequestRequest,
	SkemaHarga,
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

const ALL_DAYS: DayOfWeek[] = [
	"Monday",
	"Tuesday",
	"Wednesday",
	"Thursday",
	"Friday",
	"Saturday",
	"Sunday",
];

const DAY_LABELS: Record<DayOfWeek, string> = {
	Monday: "Senin",
	Tuesday: "Selasa",
	Wednesday: "Rabu",
	Thursday: "Kamis",
	Friday: "Jumat",
	Saturday: "Sabtu",
	Sunday: "Minggu",
};

export function NolRequestForm({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
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
	const [registrationType, setRegistrationType] =
		React.useState<RegistrationType>(
			initialData?.registrationType || "RegistrasiBaru",
		);
	const [samaDenganA1, setSamaDenganA1] = React.useState<boolean>(
		initialData?.samaDenganA1 ?? true,
	);
	const [bulanDimulai, setBulanDimulai] = React.useState<string>(
		initialData?.bulanDimulai || "",
	);
	const [skemaHarga, setSkemaHarga] = React.useState<SkemaHarga | "">(
		initialData?.skemaHarga || "Reguler",
	);
	const [hargaNilai, setHargaNilai] = React.useState<string>(
		initialData?.hargaNilai != null ? String(initialData.hargaNilai) : "",
	);
	const [alasanKontrakBersyarat, setAlasanKontrakBersyarat] =
		React.useState<string>(initialData?.alasanKontrakBersyarat || "");
	const [namaPimpinanPerusahaan, setNamaPimpinanPerusahaan] =
		React.useState<string>(initialData?.namaPimpinanPerusahaan || "");
	const [jangkaWaktuKontrak, setJangkaWaktuKontrak] = React.useState<string>(
		initialData?.jangkaWaktuKontrak || "",
	);

	// Connection Costs
	const [capexPreGr3, setCapexPreGr3] = React.useState<string>(
		initialData?.capexPreGr3 != null ? String(initialData.capexPreGr3) : "",
	);
	const [biayaPenyambunganReguler, setBiayaPenyambunganReguler] =
		React.useState<string>(
			initialData?.biayaPenyambunganReguler != null
				? String(initialData.biayaPenyambunganReguler)
				: "",
		);
	const [biayaPenyambunganExtra, setBiayaPenyambunganExtra] =
		React.useState<string>(
			initialData?.biayaPenyambunganExtra != null
				? String(initialData.biayaPenyambunganExtra)
				: "",
		);

	// Selected Reference Docs
	const [referenceDocumentIds, setReferenceDocumentIds] = React.useState<
		string[]
	>(initialData?.referenceDocumentIds || []);

	// Repeating Periods
	const [periods, setPeriods] = React.useState<NolRequestPeriodDetail[]>(
		initialData?.periods?.map((p, idx) => ({
			id: p.id || crypto.randomUUID(),
			periodeMulai: p.periodeMulai || "",
			periodeSelesai: p.periodeSelesai || "",
			rataRata: Number(p.rataRata) || 0,
			kontrakMinimum: Number(p.kontrakMinimum) || 0,
			kontrakMaksimum: Number(p.kontrakMaksimum) || 0,
			sortOrder: Number(p.sortOrder) || idx + 1,
		})) || [],
	);

	// Repeating Daily Rows
	const [dailyBasisRows, setDailyBasisRows] = React.useState<
		NolRequestDailyDetail[]
	>(
		initialData?.dailyBasisRows && initialData.dailyBasisRows.length > 0
			? initialData.dailyBasisRows.map((d) => ({
					id: d.id || crypto.randomUUID(),
					hari: d.hari,
					min: Number(d.min) || 0,
					max: Number(d.max) || 0,
				}))
			: ALL_DAYS.map((h) => ({
					id: crypto.randomUUID(),
					hari: h,
					min: 0,
					max: 0,
				})),
	);

	// Calculated Total Biaya Penyambungan
	const totalBiayaPenyambungan =
		(Number(biayaPenyambunganReguler) || 0) +
		(Number(biayaPenyambunganExtra) || 0);

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			setNomorNotaDinas(initialData.nomorNotaDinas || "");
			setRegistrationType(initialData.registrationType || "RegistrasiBaru");
			setSamaDenganA1(initialData.samaDenganA1 ?? true);
			setBulanDimulai(initialData.bulanDimulai || "");
			setSkemaHarga(initialData.skemaHarga || "Reguler");
			setHargaNilai(
				initialData.hargaNilai != null ? String(initialData.hargaNilai) : "",
			);
			setAlasanKontrakBersyarat(initialData.alasanKontrakBersyarat || "");
			setNamaPimpinanPerusahaan(initialData.namaPimpinanPerusahaan || "");
			setJangkaWaktuKontrak(initialData.jangkaWaktuKontrak || "");
			setCapexPreGr3(
				initialData.capexPreGr3 != null ? String(initialData.capexPreGr3) : "",
			);
			setBiayaPenyambunganReguler(
				initialData.biayaPenyambunganReguler != null
					? String(initialData.biayaPenyambunganReguler)
					: "",
			);
			setBiayaPenyambunganExtra(
				initialData.biayaPenyambunganExtra != null
					? String(initialData.biayaPenyambunganExtra)
					: "",
			);
			setReferenceDocumentIds(initialData.referenceDocumentIds || []);

			if (initialData.periods) {
				setPeriods(
					initialData.periods.map((p, idx) => ({
						id: p.id || crypto.randomUUID(),
						periodeMulai: p.periodeMulai || "",
						periodeSelesai: p.periodeSelesai || "",
						rataRata: Number(p.rataRata) || 0,
						kontrakMinimum: Number(p.kontrakMinimum) || 0,
						kontrakMaksimum: Number(p.kontrakMaksimum) || 0,
						sortOrder: Number(p.sortOrder) || idx + 1,
					})),
				);
			}

			if (initialData.dailyBasisRows && initialData.dailyBasisRows.length > 0) {
				setDailyBasisRows(
					initialData.dailyBasisRows.map((d) => ({
						id: d.id || crypto.randomUUID(),
						hari: d.hari,
						min: Number(d.min) || 0,
						max: Number(d.max) || 0,
					})),
				);
			}
		}
	}, [initialData]);

	// Save Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/nol-request",
		{
			onSuccess: () => {
				toast.success("Data Permohonan Surat NOL berhasil disimpan!");
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
						: "Gagal menyimpan permohonan NOL",
				);
			},
		},
	);

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveNolRequestRequest = {
			nomorNotaDinas: nomorNotaDinas || null,
			registrationType,
			samaDenganA1,
			bulanDimulai: bulanDimulai || null,
			basisKontrak: initialData?.basisKontrak || null,
			skemaHarga: skemaHarga ? (skemaHarga as SkemaHarga) : null,
			segmentId: initialData?.segmentId || null,
			kodeHarga: initialData?.kodeHarga || null,
			hargaNilai: hargaNilai ? Number(hargaNilai) : null,
			hargaCurrency: initialData?.hargaCurrency || null,
			hargaUnit: initialData?.hargaUnit || null,
			alasanKontrakBersyarat: alasanKontrakBersyarat || null,
			namaPimpinanPerusahaan: namaPimpinanPerusahaan || null,
			jangkaWaktuKontrak: jangkaWaktuKontrak || null,
			capexPreGr3: capexPreGr3 ? Number(capexPreGr3) : null,
			biayaPenyambunganReguler: biayaPenyambunganReguler
				? Number(biayaPenyambunganReguler)
				: null,
			biayaPenyambunganExtra: biayaPenyambunganExtra
				? Number(biayaPenyambunganExtra)
				: null,
			periods: periods.map((p, idx) => ({
				id: p.id || crypto.randomUUID(),
				periodeMulai: p.periodeMulai,
				periodeSelesai: p.periodeSelesai,
				rataRata: Number(p.rataRata) || 0,
				kontrakMinimum: Number(p.kontrakMinimum) || 0,
				kontrakMaksimum: Number(p.kontrakMaksimum) || 0,
				sortOrder: idx + 1,
			})),
			dailyBasisRows: dailyBasisRows.map((d) => ({
				id: d.id || crypto.randomUUID(),
				hari: d.hari,
				min: Number(d.min) || 0,
				max: Number(d.max) || 0,
			})),
			referenceDocumentIds,
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
				id: crypto.randomUUID(),
				periodeMulai: "",
				periodeSelesai: "",
				rataRata: 0,
				kontrakMinimum: 0,
				kontrakMaksimum: 0,
				sortOrder: periods.length + 1,
			},
		]);
	};

	const removePeriodRow = (index: number) => {
		setPeriods(periods.filter((_, i) => i !== index));
	};

	const toggleDoc = (id: string) => {
		if (referenceDocumentIds.includes(id)) {
			setReferenceDocumentIds(referenceDocumentIds.filter((d) => d !== id));
		} else {
			setReferenceDocumentIds([...referenceDocumentIds, id]);
		}
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
			{/* Top Bar Summary / Save */}
			<div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 bg-muted/40 rounded-lg border">
				<div className="flex items-center gap-3">
					<div className="size-10 rounded-full bg-amber-100 dark:bg-amber-900/40 text-amber-600 flex items-center justify-center">
						<FileCheck className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Formulir Permohonan Surat NOL (Notice of Letter)
						</h3>
						<p className="text-xs text-muted-foreground">
							Nota dinas permohonan, verifikasi syarat komersial, dan komitmen
							penyambungan
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="nol-request"
						label="Unduh Nota Dinas NOL (.docx)"
					/>
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
				</div>
			</div>

			{/* SECTION 1: NOTA DINAS & TIPE REGISTRASI */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<FileText className="size-4 text-amber-500" />
						1. Data Nota Dinas & Status Registrasi
					</CardTitle>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Nomor Nota Dinas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Nomor Nota Dinas</Label>
							<Input
								value={nomorNotaDinas}
								onChange={(e) => setNomorNotaDinas(e.target.value)}
								placeholder="contoh: ND-012/PGN/SA/2026"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Tipe Registrasi */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tipe Registrasi</Label>
							<Select
								value={registrationType}
								onValueChange={(val) =>
									setRegistrationType(val as RegistrationType)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="RegistrasiBaru">
										Pelanggan Baru (Registrasi Baru)
									</SelectItem>
									<SelectItem value="Amendemen">Amendemen Kontrak</SelectItem>
									<SelectItem value="Perpanjangan">
										Perpanjangan Kontrak
									</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nama Pimpinan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nama Pimpinan Perusahaan
							</Label>
							<Input
								value={namaPimpinanPerusahaan}
								onChange={(e) => setNamaPimpinanPerusahaan(e.target.value)}
								placeholder="contoh: Budi Santoso"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jangka Waktu Kontrak */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Jangka Waktu Kontrak
							</Label>
							<Input
								value={jangkaWaktuKontrak}
								onChange={(e) => setJangkaWaktuKontrak(e.target.value)}
								placeholder="contoh: 5 Tahun"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Bulan Dimulai */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Rencana Mulai Penyaluran
							</Label>
							<Input
								type="date"
								value={bulanDimulai}
								onChange={(e) => setBulanDimulai(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Sama dengan A1 Switch */}
						<div className="space-y-1.5 flex flex-col justify-end">
							<div className="flex items-center space-x-2 pb-2">
								<Switch
									id="sameWithA1"
									checked={samaDenganA1}
									onCheckedChange={setSamaDenganA1}
									disabled={!canEdit}
								/>
								<Label
									htmlFor="sameWithA1"
									className="text-xs font-medium cursor-pointer"
								>
									Ketentuan Sama dengan Formulir A1
								</Label>
							</div>
						</div>
					</div>

					{/* Skema Harga & Alasan Bersyarat */}
					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t">
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Skema Harga</Label>
							<Select
								value={skemaHarga || "NONE"}
								onValueChange={(val) =>
									setSkemaHarga(val === "NONE" ? "" : (val as SkemaHarga))
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Skema" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Dipilih</SelectItem>
									<SelectItem value="Reguler">Reguler</SelectItem>
									<SelectItem value="Sigas">SiGas</SelectItem>
									<SelectItem value="Bersyarat">Bersyarat</SelectItem>
								</SelectContent>
							</Select>
						</div>

						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tarif / Harga Gas (USD/MMBTU)
							</Label>
							<Input
								type="number"
								step="0.01"
								value={hargaNilai}
								onChange={(e) => setHargaNilai(e.target.value)}
								placeholder="contoh: 9.85"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>
					</div>

					{skemaHarga === "Bersyarat" && (
						<div className="space-y-1.5">
							<Label className="text-xs font-medium text-amber-700 dark:text-amber-400">
								Alasan / Justifikasi Skema Bersyarat
							</Label>
							<Textarea
								value={alasanKontrakBersyarat}
								onChange={(e) => setAlasanKontrakBersyarat(e.target.value)}
								placeholder="Jelaskan alasan penetapan skema bersyarat atau tarif khusus..."
								disabled={!canEdit}
								className="text-xs min-h-[60px]"
							/>
						</div>
					)}
				</CardContent>
			</Card>

			{/* SECTION 2: BIAYA PENYAMBUNGAN & CAPEX PRE-GR3 */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Coins className="size-4 text-emerald-500" />
						2. Biaya Penyambungan & Capex Pre-GR3
					</CardTitle>
					<CardDescription className="text-xs">
						Rincian biaya penyambungan jaringan dan alokasi capex awal
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
						{/* Biaya Reguler */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Biaya Penyambungan Reguler (IDR)
							</Label>
							<Input
								type="number"
								value={biayaPenyambunganReguler}
								onChange={(e) => setBiayaPenyambunganReguler(e.target.value)}
								placeholder="contoh: 15000000"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Biaya Extra */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Biaya Tambahan / Extra (IDR)
							</Label>
							<Input
								type="number"
								value={biayaPenyambunganExtra}
								onChange={(e) => setBiayaPenyambunganExtra(e.target.value)}
								placeholder="contoh: 5000000"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Total Biaya Penyambungan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium text-primary">
								Total Biaya Penyambungan (IDR)
							</Label>
							<div className="h-9 px-3 flex items-center bg-muted/60 border rounded-md font-mono text-xs font-semibold">
								Rp {totalBiayaPenyambungan.toLocaleString("id-ID")}
							</div>
						</div>

						{/* Capex Pre-GR3 */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Capex Pre-GR3 (USD)</Label>
							<Input
								type="number"
								step="0.01"
								value={capexPreGr3}
								onChange={(e) => setCapexPreGr3(e.target.value)}
								placeholder="contoh: 50000"
								disabled={!canEdit}
								className="text-xs h-9 font-mono"
							/>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: PERIODE PERMOHONAN & KEBUTUHAN HARIAN */}
			<div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
				{/* Periods */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-sm font-semibold">
								3. Periode & Volume Komitmen
							</CardTitle>
							<CardDescription className="text-xs">
								Rata-rata, min, dan max volume kontrak
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
								Belum ada periode komitmen.
							</p>
						) : (
							periods.map((row, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-3 border rounded-md space-y-2 bg-muted/20 text-xs"
								>
									<div className="flex items-center justify-between">
										<span className="font-semibold text-muted-foreground">
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
											value={row.periodeMulai}
											onChange={(e) => {
												const next = [...periods];
												next[idx].periodeMulai = e.target.value;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="date"
											value={row.periodeSelesai}
											onChange={(e) => {
												const next = [...periods];
												next[idx].periodeSelesai = e.target.value;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
									<div className="grid grid-cols-3 gap-2">
										<Input
											type="number"
											placeholder="Rata2"
											value={row.rataRata}
											onChange={(e) => {
												const next = [...periods];
												next[idx].rataRata = Number(e.target.value) || 0;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											placeholder="Min"
											value={row.kontrakMinimum}
											onChange={(e) => {
												const next = [...periods];
												next[idx].kontrakMinimum = Number(e.target.value) || 0;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											placeholder="Max"
											value={row.kontrakMaksimum}
											onChange={(e) => {
												const next = [...periods];
												next[idx].kontrakMaksimum = Number(e.target.value) || 0;
												setPeriods(next);
											}}
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
								</div>
							))
						)}
					</CardContent>
				</Card>

				{/* Daily Basis */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3">
						<CardTitle className="text-sm font-semibold">
							4. Pola Beban Harian (Senin - Minggu)
						</CardTitle>
						<CardDescription className="text-xs">
							Estimasi min dan max penyaluran harian (MMBTU/hari)
						</CardDescription>
					</CardHeader>
					<CardContent className="space-y-2">
						{dailyBasisRows.map((d, idx) => (
							<div
								key={d.hari}
								className="flex items-center justify-between gap-3 text-xs py-1 border-b last:border-b-0"
							>
								<span className="w-20 font-medium">
									{DAY_LABELS[d.hari] || d.hari}
								</span>
								<div className="flex items-center gap-2 flex-1">
									<Input
										type="number"
										step="0.1"
										placeholder="Min"
										value={d.min}
										onChange={(e) => {
											const next = [...dailyBasisRows];
											next[idx].min = Number(e.target.value) || 0;
											setDailyBasisRows(next);
										}}
										disabled={!canEdit}
										className="text-xs h-7 font-mono"
									/>
									<span className="text-muted-foreground">-</span>
									<Input
										type="number"
										step="0.1"
										placeholder="Max"
										value={d.max}
										onChange={(e) => {
											const next = [...dailyBasisRows];
											next[idx].max = Number(e.target.value) || 0;
											setDailyBasisRows(next);
										}}
										disabled={!canEdit}
										className="text-xs h-7 font-mono"
									/>
								</div>
							</div>
						))}
					</CardContent>
				</Card>
			</div>

			{/* SECTION 4: DOKUMEN REFERENSI */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Layers className="size-4 text-purple-500" />
						5. Dokumen Referensi Kebijakan / Ketentuan Acuan
					</CardTitle>
					<CardDescription className="text-xs">
						Pilih dasar acuan regulasi atau surat keputusan yang menjadi dasar
						permohonan NOL
					</CardDescription>
				</CardHeader>
				<CardContent>
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
						{refDocs?.map((doc) => (
							<div
								key={doc.id}
								className="flex items-start space-x-2.5 p-2.5 rounded-md border bg-muted/20"
							>
								<Checkbox
									id={`doc-${doc.id}`}
									checked={referenceDocumentIds.includes(doc.id)}
									onCheckedChange={() => toggleDoc(doc.id)}
									disabled={!canEdit}
									className="mt-0.5"
								/>
								<div className="space-y-0.5 leading-none text-xs">
									<Label
										htmlFor={`doc-${doc.id}`}
										className="font-medium cursor-pointer"
									>
										{doc.name}
									</Label>
									<p className="text-[11px] text-muted-foreground">
										Versi {doc.version} (Berlaku sejak {doc.effectiveFrom})
									</p>
								</div>
							</div>
						))}
					</div>
				</CardContent>
			</Card>

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
