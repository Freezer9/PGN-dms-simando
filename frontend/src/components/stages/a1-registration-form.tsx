import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import {
	Building2,
	FileText,
	Loader2,
	Plus,
	Save,
	Trash2,
	UserCheck,
} from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	A1RegistrationDetail,
	BasisKontrak,
	HargaCurrency,
	HargaUnit,
	RegistrasiSource,
	SaveA1RegistrationRequest,
	SaveA1UsagePeriodRequest,
	Sektor,
	SignatureMethod,
	SkemaHarga,
	StatusBangunan,
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
	type A1RegistrationFormValues,
	a1RegistrationSchema,
} from "@/lib/schemas";

interface A1RegistrationFormProps {
	companyId: string;
	initialData?: A1RegistrationDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
}

function getDefaultValues(
	initialData?: A1RegistrationDetail | null,
): A1RegistrationFormValues {
	return {
		tanggalRegistrasi: initialData?.tanggalRegistrasi || "",
		registrasiSource: initialData?.registrasiSource || "Manual",
		namaPenanggungJawab: initialData?.namaPenanggungJawab || "",
		jabatan: initialData?.jabatan || "",
		bulanDimulai: initialData?.bulanDimulai || "",
		basisKontrak: initialData?.basisKontrak || "Bulanan",
		skemaHarga: initialData?.skemaHarga || "Reguler",
		segmentId: initialData?.segmentId || "",
		kodeHarga: initialData?.kodeHarga || "",
		hargaCurrency: initialData?.hargaCurrency || "USD",
		hargaUnit: initialData?.hargaUnit || "MMBtu",
		hargaNilai:
			initialData?.hargaNilai != null ? String(initialData.hargaNilai) : "",
		capexAwal:
			initialData?.capexAwal != null ? String(initialData.capexAwal) : "",
		momSigasTersedia: initialData?.momSigasTersedia ?? false,
		statusBangunan: initialData?.statusBangunan || "Eksisting",
		sektor: initialData?.sektor || "Industri",
		produksiUtama: initialData?.produksiUtama || "",
		jenisPeralatanGas: initialData?.jenisPeralatanGas || "",
		tekananOperasiBarg:
			initialData?.tekananOperasiBarg != null
				? String(initialData.tekananOperasiBarg)
				: "",
		signatureMethod: initialData?.signatureMethod || "Wet",
		usagePeriods:
			initialData?.usagePeriods?.map((p, idx) => ({
				id: p.id || crypto.randomUUID(),
				periodeMulai: p.periodeMulai || "",
				periodeSelesai: p.periodeSelesai || "",
				rataRata: p.rataRata != null ? Number(p.rataRata) : 0,
				minimum: p.minimum != null ? Number(p.minimum) : 0,
				maksimum: p.maksimum != null ? Number(p.maksimum) : 0,
				sortOrder: p.sortOrder != null ? Number(p.sortOrder) : idx + 1,
			})) || [],
	};
}

export function A1RegistrationForm({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
}: A1RegistrationFormProps) {
	const queryClient = useQueryClient();
	const { data: segments } = $api.useQuery("get", "/api/master/segments");

	// Save Registration Mutation
	const saveMutation = $api.useMutation(
		"put",
		"/api/companies/{id}/registration",
		{
			onSuccess: () => {
				toast.success("Formulir Registrasi A1 berhasil disimpan!");
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
						"/api/companies/{id}/registration",
						{ params: { path: { id: companyId } } },
					],
				});
				onSaved?.();
			},
			onError: (error) => {
				toast.error(
					error instanceof Error
						? error.message
						: "Gagal menyimpan Formulir Registrasi A1",
				);
			},
		},
	);

	const form = useForm({
		defaultValues: getDefaultValues(initialData),
		validators: {
			onChange: a1RegistrationSchema,
		},
		onSubmit: async ({ value }) => {
			const usagePeriods: SaveA1UsagePeriodRequest[] = (
				value.usagePeriods || []
			).map((p, idx) => ({
				id: p.id || crypto.randomUUID(),
				periodeMulai: p.periodeMulai || "",
				periodeSelesai: p.periodeSelesai || "",
				rataRata: Number(p.rataRata) || 0,
				minimum: Number(p.minimum) || 0,
				maksimum: Number(p.maksimum) || 0,
				sortOrder: idx + 1,
			}));

			const request: SaveA1RegistrationRequest = {
				tanggalRegistrasi: value.tanggalRegistrasi || null,
				namaPenanggungJawab: value.namaPenanggungJawab || null,
				jabatan: value.jabatan || null,
				bulanDimulai: value.bulanDimulai || null,
				basisKontrak: value.basisKontrak
					? (value.basisKontrak as BasisKontrak)
					: null,
				skemaHarga: value.skemaHarga ? (value.skemaHarga as SkemaHarga) : null,
				segmentId: value.segmentId || null,
				kodeHarga: value.kodeHarga || null,
				hargaNilai: value.hargaNilai ? Number(value.hargaNilai) : null,
				hargaCurrency: value.hargaCurrency
					? (value.hargaCurrency as HargaCurrency)
					: null,
				hargaUnit: value.hargaUnit ? (value.hargaUnit as HargaUnit) : null,
				capexAwal: value.capexAwal ? Number(value.capexAwal) : null,
				momSigasTersedia: value.momSigasTersedia ?? false,
				statusBangunan: value.statusBangunan
					? (value.statusBangunan as StatusBangunan)
					: null,
				sektor: value.sektor ? (value.sektor as Sektor) : null,
				produksiUtama: value.produksiUtama || null,
				jenisPeralatanGas: value.jenisPeralatanGas || null,
				tekananOperasiBarg: value.tekananOperasiBarg
					? Number(value.tekananOperasiBarg)
					: null,
				signedDocumentId: initialData?.signedDocumentId || null,
				signatureMethod: value.signatureMethod
					? (value.signatureMethod as SignatureMethod)
					: null,
				usagePeriods,
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
					<div className="size-10 rounded-full bg-blue-100 dark:bg-blue-900/40 text-blue-600 flex items-center justify-center">
						<FileText className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Formulir Berlangganan Gas Bumi (Formulir A1)
						</h3>
						<p className="text-xs text-muted-foreground">
							Legalitas pelanggan, penanggung jawab, skema tarif, dan periode
							penggunaan gas
						</p>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="a1"
						label="Unduh Formulir A1 (.docx)"
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
									className="h-9 text-xs flex items-center gap-1.5 bg-blue-600 hover:bg-blue-700 text-white"
								>
									{isSubmitting ? (
										<Loader2 className="size-3.5 animate-spin" />
									) : (
										<Save className="size-3.5" />
									)}
									Simpan Formulir A1
								</Button>
							)}
						</form.Subscribe>
					)}
				</div>
			</div>

			{/* SECTION 1: DATA PIC & PENANGGUNG JAWAB */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<UserCheck className="size-4 text-blue-500" />
						1. Data Penanggung Jawab & Pendaftaran
					</CardTitle>
					<CardDescription className="text-xs">
						Petugas yang berwenang menandatangani kontrak atau mewakili
						pelanggan
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Tanggal Registrasi */}
						<form.Field name="tanggalRegistrasi">
							{(field) => (
								<FormField label="Tanggal Registrasi">
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

						{/* Kanal Pendaftaran */}
						<form.Field name="registrasiSource">
							{(field) => (
								<FormField label="Kanal Pendaftaran">
									<Select
										value={field.state.value || "Manual"}
										onValueChange={(val) =>
											field.handleChange(val as RegistrasiSource)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="Manual">
												Manual / Tatap Muka
											</SelectItem>
											<SelectItem value="Online">Online / Portal</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Nama Penanggung Jawab */}
						<form.Field name="namaPenanggungJawab">
							{(field) => (
								<FormField label="Nama Penanggung Jawab">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Hendra Gunawan"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Jabatan */}
						<form.Field name="jabatan">
							{(field) => (
								<FormField label="Jabatan">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Direktur Operasional"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Metode Tanda Tangan */}
						<form.Field name="signatureMethod">
							{(field) => (
								<FormField label="Metode Tanda Tangan">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Metode" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Wet">Tanda Tangan Basah</SelectItem>
											<SelectItem value="Digital">Digital / E-Sign</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* MoM SiGas */}
						<form.Field name="momSigasTersedia">
							{(field) => (
								<div className="space-y-1.5 flex flex-col justify-end">
									<div className="flex items-center space-x-2 pb-2">
										<Switch
											id="momSigas"
											checked={field.state.value ?? false}
											onCheckedChange={(checked) => field.handleChange(checked)}
											disabled={!canEdit}
										/>
										<Label
											htmlFor="momSigas"
											className="text-xs font-medium cursor-pointer"
										>
											MoM SiGas Tersedia
										</Label>
									</div>
								</div>
							)}
						</form.Field>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: SKEMA HARGA & KONTRAK */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<Building2 className="size-4 text-indigo-500" />
						2. Skema Harga, Kontrak & Status Bangunan
					</CardTitle>
					<CardDescription className="text-xs">
						Struktur harga jual gas, basis kontrak penyaluran, dan fasilitas
						fisik
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Bulan Dimulai */}
						<form.Field name="bulanDimulai">
							{(field) => (
								<FormField label="Rencana Mulai Penyaluran Gas">
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

						{/* Basis Kontrak */}
						<form.Field name="basisKontrak">
							{(field) => (
								<FormField label="Basis Kontrak">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Basis Kontrak" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Bulanan">Bulanan</SelectItem>
											<SelectItem value="Harian">Harian</SelectItem>
											<SelectItem value="Tahunan">Tahunan</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Skema Harga */}
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
											<SelectValue placeholder="Pilih Skema Harga" />
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

						{/* Segment */}
						<form.Field name="segmentId">
							{(field) => (
								<FormField label="Segmen Pelanggan">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Segmen" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											{segments?.map((s) => (
												<SelectItem key={s.id} value={s.id}>
													{s.name}
												</SelectItem>
											))}
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Kode Harga */}
						<form.Field name="kodeHarga">
							{(field) => (
								<FormField label="Kode Harga">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: IND-1"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Harga Nilai & Currency & Unit */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tarif / Harga Jual Gas
							</Label>
							<div className="flex gap-2">
								<form.Field name="hargaNilai">
									{(field) => (
										<Input
											type="number"
											step="0.01"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											placeholder="contoh: 9.85"
											disabled={!canEdit}
											className="text-xs h-9 font-mono"
										/>
									)}
								</form.Field>

								<form.Field name="hargaCurrency">
									{(field) => (
										<Select
											value={field.state.value || "USD"}
											onValueChange={(val) => field.handleChange(val)}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-9 w-20">
												<SelectValue />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="USD">USD</SelectItem>
												<SelectItem value="IDR">IDR</SelectItem>
											</SelectContent>
										</Select>
									)}
								</form.Field>

								<form.Field name="hargaUnit">
									{(field) => (
										<Select
											value={field.state.value || "MMBtu"}
											onValueChange={(val) => field.handleChange(val)}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-9 w-24">
												<SelectValue />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="MMBtu">/ MMBTU</SelectItem>
												<SelectItem value="M3">/ M3</SelectItem>
											</SelectContent>
										</Select>
									)}
								</form.Field>
							</div>
						</div>

						{/* Capex Awal */}
						<form.Field name="capexAwal">
							{(field) => (
								<FormField label="Capex Awal / Estimasi (USD)">
									<Input
										type="number"
										step="0.01"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 75000"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Status Bangunan */}
						<form.Field name="statusBangunan">
							{(field) => (
								<FormField label="Status Bangunan">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Status Bangunan" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Eksisting">Eksisting</SelectItem>
											<SelectItem value="DalamPembangunan">
												Dalam Pembangunan
											</SelectItem>
											<SelectItem value="DalamRencana">
												Dalam Rencana
											</SelectItem>
											<SelectItem value="ProsesEkspansi">
												Proses Ekspansi
											</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Sektor */}
						<form.Field name="sektor">
							{(field) => (
								<FormField label="Sektor">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Sektor" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Industri">Industri</SelectItem>
											<SelectItem value="Komersial">Komersial</SelectItem>
											<SelectItem value="Transportasi">Transportasi</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t">
						{/* Produksi Utama */}
						<form.Field name="produksiUtama">
							{(field) => (
								<FormField label="Produksi / Hasil Utama">
									<Input
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: Keramik Ubin Lantai"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Tekanan Operasi (Barg) */}
						<form.Field name="tekananOperasiBarg">
							{(field) => (
								<FormField label="Tekanan Operasi yang Dibutuhkan (Barg)">
									<Input
										type="number"
										step="0.1"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										placeholder="contoh: 2.0"
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					{/* Jenis Peralatan Gas */}
					<form.Field name="jenisPeralatanGas">
						{(field) => (
							<FormField label="Jenis Peralatan Gas yang Digunakan">
								<Textarea
									value={field.state.value}
									onChange={(e) => field.handleChange(e.target.value)}
									placeholder="contoh: 2 unit Boiler Miura 5 Ton, 1 unit Burner Riello"
									disabled={!canEdit}
									className="text-xs min-h-[60px]"
								/>
							</FormField>
						)}
					</form.Field>
				</CardContent>
			</Card>

			{/* SECTION 3: PERIODE PENGGUNAAN GAS */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold">
							3. Periode & Volume Penggunaan Gas
						</CardTitle>
						<CardDescription className="text-xs">
							Jadwal komitmen volume penyaluran gas (rata-rata, minimum,
							maksimum)
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={() => {
								const current = form.getFieldValue("usagePeriods") || [];
								form.setFieldValue("usagePeriods", [
									...current,
									{
										id: crypto.randomUUID(),
										periodeMulai: "",
										periodeSelesai: "",
										rataRata: 0,
										minimum: 0,
										maksimum: 0,
										sortOrder: current.length + 1,
									},
								]);
							}}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" />
							Tambah Periode
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<form.Field name="usagePeriods">
						{(field) => {
							const periods = field.state.value || [];
							return (
								<div className="rounded-lg border overflow-hidden">
									<Table>
										<TableHeader className="bg-muted/40">
											<TableRow>
												<TableHead className="text-xs">Periode Mulai</TableHead>
												<TableHead className="text-xs">
													Periode Selesai
												</TableHead>
												<TableHead className="text-xs">Rata-rata</TableHead>
												<TableHead className="text-xs">Minimum</TableHead>
												<TableHead className="text-xs">Maksimum</TableHead>
												{canEdit && (
													<TableHead className="text-xs text-center w-12">
														Hapus
													</TableHead>
												)}
											</TableRow>
										</TableHeader>
										<TableBody>
											{periods.length === 0 ? (
												<TableRow>
													<TableCell
														colSpan={canEdit ? 6 : 5}
														className="text-center text-xs py-6 text-muted-foreground"
													>
														Belum ada data periode penggunaan gas. Klik &quot;+
														Tambah Periode&quot; untuk menambahkan.
													</TableCell>
												</TableRow>
											) : (
												periods.map((row, idx) => (
													<TableRow key={row.id || `period-${idx}`}>
														<TableCell>
															<Input
																type="date"
																value={row.periodeMulai || ""}
																onChange={(e) => {
																	const next = [...periods];
																	next[idx] = {
																		...row,
																		periodeMulai: e.target.value,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="date"
																value={row.periodeSelesai || ""}
																onChange={(e) => {
																	const next = [...periods];
																	next[idx] = {
																		...row,
																		periodeSelesai: e.target.value,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.rataRata != null
																		? String(row.rataRata)
																		: ""
																}
																onChange={(e) => {
																	const next = [...periods];
																	next[idx] = {
																		...row,
																		rataRata: Number(e.target.value) || 0,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8 font-mono"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.minimum != null ? String(row.minimum) : ""
																}
																onChange={(e) => {
																	const next = [...periods];
																	next[idx] = {
																		...row,
																		minimum: Number(e.target.value) || 0,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8 font-mono"
															/>
														</TableCell>
														<TableCell>
															<Input
																type="number"
																step="0.01"
																value={
																	row.maksimum != null
																		? String(row.maksimum)
																		: ""
																}
																onChange={(e) => {
																	const next = [...periods];
																	next[idx] = {
																		...row,
																		maksimum: Number(e.target.value) || 0,
																	};
																	field.handleChange(next);
																}}
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
																	onClick={() => {
																		field.handleChange(
																			periods.filter((_, i) => i !== idx),
																		);
																	}}
																	className="size-7 text-destructive hover:bg-destructive/10"
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
							);
						}}
					</form.Field>
				</CardContent>
			</Card>
		</form>
	);
}
