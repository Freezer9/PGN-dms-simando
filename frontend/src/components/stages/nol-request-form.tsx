import { useForm } from "@tanstack/react-form";
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
	NolRequestDetail,
	RegistrationType,
	SaveNolRequestDailyRequest,
	SaveNolRequestPeriodRequest,
	SaveNolRequestRequest,
	SkemaHarga,
} from "@/api/types";
import { DocumentDownloadButton } from "@/components/documents/document-download-buttons";
import { FormField } from "@/components/form/form-field";
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
import { type NolRequestFormValues, nolRequestSchema } from "@/lib/schemas";

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

function getDefaultValues(
	initialData?: NolRequestDetail | null,
): NolRequestFormValues {
	return {
		nomorNotaDinas: initialData?.nomorNotaDinas || "",
		registrationType: initialData?.registrationType || "RegistrasiBaru",
		samaDenganA1: initialData?.samaDenganA1 ?? true,
		bulanDimulai: initialData?.bulanDimulai || "",
		skemaHarga: initialData?.skemaHarga || "Reguler",
		hargaNilai:
			initialData?.hargaNilai != null ? String(initialData.hargaNilai) : "",
		alasanKontrakBersyarat: initialData?.alasanKontrakBersyarat || "",
		namaPimpinanPerusahaan: initialData?.namaPimpinanPerusahaan || "",
		jangkaWaktuKontrak: initialData?.jangkaWaktuKontrak || "",
		capexPreGr3:
			initialData?.capexPreGr3 != null ? String(initialData.capexPreGr3) : "",
		biayaPenyambunganReguler:
			initialData?.biayaPenyambunganReguler != null
				? String(initialData.biayaPenyambunganReguler)
				: "",
		biayaPenyambunganExtra:
			initialData?.biayaPenyambunganExtra != null
				? String(initialData.biayaPenyambunganExtra)
				: "",
		referenceDocumentIds: initialData?.referenceDocumentIds || [],
		periods:
			initialData?.periods?.map((p, idx) => ({
				id: p.id || crypto.randomUUID(),
				periodeMulai: p.periodeMulai || "",
				periodeSelesai: p.periodeSelesai || "",
				rataRata: Number(p.rataRata) || 0,
				kontrakMinimum: Number(p.kontrakMinimum) || 0,
				kontrakMaksimum: Number(p.kontrakMaksimum) || 0,
				sortOrder: Number(p.sortOrder) || idx + 1,
			})) || [],
		dailyBasisRows:
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
	};
}

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

	const form = useForm({
		defaultValues: getDefaultValues(initialData),
		validators: {
			onSubmit: nolRequestSchema,
		},
		onSubmit: async ({ value }) => {
			const periods: SaveNolRequestPeriodRequest[] = (value.periods || []).map(
				(p, idx) => ({
					id: p.id || crypto.randomUUID(),
					periodeMulai: p.periodeMulai || "",
					periodeSelesai: p.periodeSelesai || "",
					rataRata: Number(p.rataRata) || 0,
					kontrakMinimum: Number(p.kontrakMinimum) || 0,
					kontrakMaksimum: Number(p.kontrakMaksimum) || 0,
					sortOrder: idx + 1,
				}),
			);

			const dailyBasisRows: SaveNolRequestDailyRequest[] = (
				value.dailyBasisRows || []
			).map((d) => ({
				id: d.id || crypto.randomUUID(),
				hari: d.hari as DayOfWeek,
				min: Number(d.min) || 0,
				max: Number(d.max) || 0,
			}));

			const request: SaveNolRequestRequest = {
				nomorNotaDinas: value.nomorNotaDinas || null,
				registrationType:
					(value.registrationType as RegistrationType) || "RegistrasiBaru",
				samaDenganA1: value.samaDenganA1 ?? true,
				bulanDimulai: value.bulanDimulai || null,
				basisKontrak: initialData?.basisKontrak || null,
				skemaHarga: value.skemaHarga ? (value.skemaHarga as SkemaHarga) : null,
				segmentId: initialData?.segmentId || null,
				kodeHarga: initialData?.kodeHarga || null,
				hargaNilai: value.hargaNilai ? Number(value.hargaNilai) : null,
				hargaCurrency: initialData?.hargaCurrency || null,
				hargaUnit: initialData?.hargaUnit || null,
				alasanKontrakBersyarat: value.alasanKontrakBersyarat || null,
				namaPimpinanPerusahaan: value.namaPimpinanPerusahaan || null,
				jangkaWaktuKontrak: value.jangkaWaktuKontrak || null,
				capexPreGr3: value.capexPreGr3 ? Number(value.capexPreGr3) : null,
				biayaPenyambunganReguler: value.biayaPenyambunganReguler
					? Number(value.biayaPenyambunganReguler)
					: null,
				biayaPenyambunganExtra: value.biayaPenyambunganExtra
					? Number(value.biayaPenyambunganExtra)
					: null,
				periods,
				dailyBasisRows,
				referenceDocumentIds: value.referenceDocumentIds || [],
			};

			await saveMutation.mutateAsync({
				params: { path: { id: companyId } },
				body: request,
			});
		},
	});

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			form.reset(getDefaultValues(initialData));
		}
	}, [initialData, form]);

	return (
		<form
			onSubmit={(e) => {
				e.preventDefault();
				e.stopPropagation();
				form.handleSubmit();
			}}
			className="space-y-6"
		>
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
						<form.Subscribe
							selector={(state) => [state.canSubmit, state.isSubmitting]}
						>
							{([canSubmit, isSubmitting]) => (
								<Button
									type="submit"
									size="sm"
									disabled={!canSubmit || isSubmitting}
									className="h-9 text-xs flex items-center gap-1.5 bg-amber-600 hover:bg-amber-700 text-white"
								>
									{isSubmitting ? (
										<Loader2 className="size-3.5 animate-spin" />
									) : (
										<Save className="size-3.5" />
									)}
									Simpan Permohonan NOL
								</Button>
							)}
						</form.Subscribe>
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
						<form.Field name="nomorNotaDinas">
							{(field) => (
								<FormField label="Nomor Nota Dinas">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: ND-012/PGN/SA/2026"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Tipe Registrasi */}
						<form.Field name="registrationType">
							{(field) => (
								<FormField label="Tipe Registrasi">
									<Select
										value={field.state.value || "RegistrasiBaru"}
										onValueChange={(val) =>
											field.handleChange(val as RegistrationType)
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
											<SelectItem value="Amendemen">
												Amendemen Kontrak
											</SelectItem>
											<SelectItem value="Perpanjangan">
												Perpanjangan Kontrak
											</SelectItem>
											<SelectItem value="Reaktivasi">Reaktivasi</SelectItem>
											<SelectItem value="TambahVolume">
												Tambah Volume
											</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Nama Pimpinan */}
						<form.Field name="namaPimpinanPerusahaan">
							{(field) => (
								<FormField label="Nama Pimpinan Perusahaan">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Budi Santoso"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Jangka Waktu Kontrak */}
						<form.Field name="jangkaWaktuKontrak">
							{(field) => (
								<FormField label="Jangka Waktu Kontrak">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 5 Tahun"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Bulan Dimulai */}
						<form.Field name="bulanDimulai">
							{(field) => (
								<FormField label="Rencana Mulai Penyaluran">
									<Input
										type="date"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Sama dengan A1 Switch */}
						<form.Field name="samaDenganA1">
							{(field) => (
								<div className="space-y-1.5 flex flex-col justify-end">
									<div className="flex items-center space-x-2 pb-2">
										<Switch
											id="sameWithA1"
											checked={field.state.value ?? true}
											onCheckedChange={(checked) => field.handleChange(checked)}
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
							)}
						</form.Field>
					</div>

					{/* Skema Harga & Alasan Bersyarat */}
					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t">
						<form.Field name="skemaHarga">
							{(field) => (
								<FormField label="Skema Harga">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
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
								</FormField>
							)}
						</form.Field>

						<form.Field name="hargaNilai">
							{(field) => (
								<FormField label="Tarif / Harga Gas (USD/MMBTU)">
									<Input
										type="number"
										step="0.01"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 9.85"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					<form.Subscribe selector={(state) => state.values.skemaHarga}>
						{(skemaHarga) =>
							skemaHarga === "Bersyarat" ? (
								<form.Field name="alasanKontrakBersyarat">
									{(field) => (
										<FormField
											label="Alasan / Justifikasi Skema Bersyarat"
											className="space-y-1.5"
										>
											<Textarea
												value={field.state.value}
												onChange={(e) => field.handleChange(e.target.value)}
												placeholder="Jelaskan alasan penetapan skema bersyarat atau tarif khusus..."
												disabled={!canEdit}
												className="text-xs min-h-[60px]"
											/>
										</FormField>
									)}
								</form.Field>
							) : null
						}
					</form.Subscribe>
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
						<form.Field name="biayaPenyambunganReguler">
							{(field) => (
								<FormField label="Biaya Penyambungan Reguler (IDR)">
									<Input
										type="number"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 15000000"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Biaya Extra */}
						<form.Field name="biayaPenyambunganExtra">
							{(field) => (
								<FormField label="Biaya Tambahan / Extra (IDR)">
									<Input
										type="number"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 5000000"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Total Biaya Penyambungan (Subscribed Calculation) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium text-primary">
								Total Biaya Penyambungan (IDR)
							</Label>
							<form.Subscribe
								selector={(state) => {
									const reg =
										Number(state.values.biayaPenyambunganReguler) || 0;
									const ext = Number(state.values.biayaPenyambunganExtra) || 0;
									return reg + ext;
								}}
							>
								{(totalBiayaPenyambungan) => (
									<div className="h-9 px-3 flex items-center bg-muted/60 border rounded-md font-mono text-xs font-semibold">
										Rp {totalBiayaPenyambungan.toLocaleString("id-ID")}
									</div>
								)}
							</form.Subscribe>
						</div>

						{/* Capex Pre-GR3 */}
						<form.Field name="capexPreGr3">
							{(field) => (
								<FormField label="Capex Pre-GR3 (USD)">
									<Input
										type="number"
										step="0.01"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 50000"
										disabled={!canEdit}
										className="text-xs h-9 font-mono"
									/>
								</FormField>
							)}
						</form.Field>
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
								onClick={() => {
									const current = form.getFieldValue("periods") || [];
									form.setFieldValue("periods", [
										...current,
										{
											id: crypto.randomUUID(),
											periodeMulai: "",
											periodeSelesai: "",
											rataRata: 0,
											kontrakMinimum: 0,
											kontrakMaksimum: 0,
											sortOrder: current.length + 1,
										},
									]);
								}}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						<form.Field name="periods">
							{(field) => {
								const periodList = field.state.value || [];
								return periodList.length === 0 ? (
									<p className="text-xs text-muted-foreground text-center py-4">
										Belum ada periode komitmen.
									</p>
								) : (
									periodList.map((row, idx) => (
										<div
											key={row.id || `period-${idx}`}
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
														onClick={() => {
															field.handleChange(
																periodList.filter((_, i) => i !== idx),
															);
														}}
														className="size-6 text-destructive"
													>
														<Trash2 className="size-3" />
													</Button>
												)}
											</div>
											<div className="grid grid-cols-2 gap-2">
												<Input
													type="date"
													value={row.periodeMulai || ""}
													onChange={(e) => {
														const next = [...periodList];
														next[idx] = {
															...row,
															periodeMulai: e.target.value,
														};
														field.handleChange(next);
													}}
													disabled={!canEdit}
													className="text-xs h-7"
												/>
												<Input
													type="date"
													value={row.periodeSelesai || ""}
													onChange={(e) => {
														const next = [...periodList];
														next[idx] = {
															...row,
															periodeSelesai: e.target.value,
														};
														field.handleChange(next);
													}}
													disabled={!canEdit}
													className="text-xs h-7"
												/>
											</div>
											<div className="grid grid-cols-3 gap-2">
												<Input
													type="number"
													placeholder="Rata2"
													value={
														row.rataRata != null ? String(row.rataRata) : ""
													}
													onChange={(e) => {
														const next = [...periodList];
														next[idx] = {
															...row,
															rataRata: Number(e.target.value) || 0,
														};
														field.handleChange(next);
													}}
													disabled={!canEdit}
													className="text-xs h-7"
												/>
												<Input
													type="number"
													placeholder="Min"
													value={
														row.kontrakMinimum != null
															? String(row.kontrakMinimum)
															: ""
													}
													onChange={(e) => {
														const next = [...periodList];
														next[idx] = {
															...row,
															kontrakMinimum: Number(e.target.value) || 0,
														};
														field.handleChange(next);
													}}
													disabled={!canEdit}
													className="text-xs h-7"
												/>
												<Input
													type="number"
													placeholder="Max"
													value={
														row.kontrakMaksimum != null
															? String(row.kontrakMaksimum)
															: ""
													}
													onChange={(e) => {
														const next = [...periodList];
														next[idx] = {
															...row,
															kontrakMaksimum: Number(e.target.value) || 0,
														};
														field.handleChange(next);
													}}
													disabled={!canEdit}
													className="text-xs h-7"
												/>
											</div>
										</div>
									))
								);
							}}
						</form.Field>
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
						<form.Field name="dailyBasisRows">
							{(field) => {
								const dailyRows = field.state.value || [];
								return dailyRows.map((d, idx) => (
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
												value={d.min != null ? String(d.min) : ""}
												onChange={(e) => {
													const next = [...dailyRows];
													next[idx] = {
														...d,
														min: Number(e.target.value) || 0,
													};
													field.handleChange(next);
												}}
												disabled={!canEdit}
												className="text-xs h-7 font-mono"
											/>
											<span className="text-muted-foreground">-</span>
											<Input
												type="number"
												step="0.1"
												placeholder="Max"
												value={d.max != null ? String(d.max) : ""}
												onChange={(e) => {
													const next = [...dailyRows];
													next[idx] = {
														...d,
														max: Number(e.target.value) || 0,
													};
													field.handleChange(next);
												}}
												disabled={!canEdit}
												className="text-xs h-7 font-mono"
											/>
										</div>
									</div>
								));
							}}
						</form.Field>
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
					<form.Field name="referenceDocumentIds">
						{(field) => {
							const selectedDocIds = field.state.value || [];
							const toggleDoc = (id: string) => {
								if (selectedDocIds.includes(id)) {
									field.handleChange(selectedDocIds.filter((d) => d !== id));
								} else {
									field.handleChange([...selectedDocIds, id]);
								}
							};

							return (
								<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-3">
									{refDocs?.map((doc) => (
										<div
											key={doc.id}
											className="flex items-start space-x-2.5 p-2.5 rounded-md border bg-muted/20"
										>
											<Checkbox
												id={`doc-${doc.id}`}
												checked={selectedDocIds.includes(doc.id)}
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
													Versi {doc.version} (Berlaku sejak {doc.effectiveFrom}
													)
												</p>
											</div>
										</div>
									))}
								</div>
							);
						}}
					</form.Field>
				</CardContent>
			</Card>
		</form>
	);
}
