import { useForm } from "@tanstack/react-form";
import { useQueryClient } from "@tanstack/react-query";
import { Flame, Loader2, Plus, Save, Trash2, Zap } from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	Asal,
	BahanBakarEksisting,
	KebutuhanEnergiJenis,
	SaveSurveyEquipmentRequest,
	SaveSurveyMarketRequest,
	SaveSurveyProductRequest,
	SaveSurveyRawMaterialRequest,
	SaveSurveyRequest,
	SurveyDetail,
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
import {
	Table,
	TableBody,
	TableCell,
	TableHead,
	TableHeader,
	TableRow,
} from "@/components/ui/table";
import { Textarea } from "@/components/ui/textarea";
import { type SurveyKk0FormValues, surveyKk0Schema } from "@/lib/schemas";

interface SurveyKk0FormProps {
	companyId: string;
	initialData?: SurveyDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
}

function getDefaultValues(
	initialData?: SurveyDetail | null,
): SurveyKk0FormValues {
	return {
		tanggalSurvey: initialData?.tanggalSurvey || "",
		surveyorUserId: initialData?.surveyorUserId || "",
		jumlahKaryawan:
			initialData?.jumlahKaryawan != null
				? String(initialData.jumlahKaryawan)
				: "",
		jumlahShift:
			initialData?.jumlahShift != null ? String(initialData.jumlahShift) : "",
		jamKerjaPerHari:
			initialData?.jamKerjaPerHari != null
				? String(initialData.jamKerjaPerHari)
				: "",
		hariPerMinggu:
			initialData?.hariPerMinggu != null
				? String(initialData.hariPerMinggu)
				: "",
		bebanPuncak1Mulai: initialData?.bebanPuncak1Mulai || "",
		bebanPuncak1Selesai: initialData?.bebanPuncak1Selesai || "",
		bebanPuncak2Mulai: initialData?.bebanPuncak2Mulai || "",
		bebanPuncak2Selesai: initialData?.bebanPuncak2Selesai || "",
		kebutuhanEnergi: initialData?.kebutuhanEnergi || "",
		kebutuhanEnergiLainnya: initialData?.kebutuhanEnergiLainnya || "",
		kapasitasNilai:
			initialData?.kapasitasNilai != null
				? String(initialData.kapasitasNilai)
				: "",
		kapasitasUnitId: initialData?.kapasitasUnitId || "",
		pemakaianNilai:
			initialData?.pemakaianNilai != null
				? String(initialData.pemakaianNilai)
				: "",
		pemakaianUnitId: initialData?.pemakaianUnitId || "",
		pipaTerdekatJarakM:
			initialData?.pipaTerdekatJarakM != null
				? String(initialData.pipaTerdekatJarakM)
				: "",
		pipaTerdekatDiameter:
			initialData?.pipaTerdekatDiameter != null
				? String(initialData.pipaTerdekatDiameter)
				: "",
		pipaTerdekatTekanan:
			initialData?.pipaTerdekatTekanan != null
				? String(initialData.pipaTerdekatTekanan)
				: "",
		bahanBakarEksisting: initialData?.bahanBakarEksisting || "",
		namaPemasok: initialData?.namaPemasok || "",
		kapasitasListrikKw:
			initialData?.kapasitasListrikKw != null
				? String(initialData.kapasitasListrikKw)
				: "",
		pemakaianListrikKwh:
			initialData?.pemakaianListrikKwh != null
				? String(initialData.pemakaianListrikKwh)
				: "",
		rencanaPemanfaatanGas: initialData?.rencanaPemanfaatanGas || "",
		deskripsiProsesProduksi: initialData?.deskripsiProsesProduksi || "",
		minEfisiensiDiharapkanPct:
			initialData?.minEfisiensiDiharapkanPct != null
				? String(initialData.minEfisiensiDiharapkanPct)
				: "",
		willingnessToPayUsdMmbtu:
			initialData?.willingnessToPayUsdMmbtu != null
				? String(initialData.willingnessToPayUsdMmbtu)
				: "",
		keteranganLain: initialData?.keteranganLain || "",
		products:
			initialData?.products?.map((p) => ({
				id: crypto.randomUUID(),
				produk: p.produk,
				kapasitas: p.kapasitas != null ? p.kapasitas : null,
				hargaProduk: p.hargaProduk != null ? p.hargaProduk : null,
				catatan: p.catatan || null,
			})) || [],
		rawMaterials:
			initialData?.rawMaterials?.map((m) => ({
				id: crypto.randomUUID(),
				bahan: m.bahan || null,
				asal: m.asal || null,
				countryId: m.countryId || null,
				volume: m.volume != null ? m.volume : null,
				satuanUnitId: m.satuanUnitId || null,
			})) || [],
		markets:
			initialData?.markets?.map((m) => ({
				id: crypto.randomUUID(),
				bahan: m.bahan || null,
				asal: m.asal || null,
				countryId: m.countryId || null,
				volume: m.volume != null ? m.volume : null,
				satuanUnitId: m.satuanUnitId || null,
			})) || [],
		equipment:
			initialData?.equipment?.map((e) => ({
				id: crypto.randomUUID(),
				jenisPeralatan: e.jenisPeralatan,
				kapasitas: e.kapasitas != null ? e.kapasitas : null,
				kapasitasUnitId: e.kapasitasUnitId || null,
				jamPerHari: e.jamPerHari != null ? e.jamPerHari : null,
				hariPerMinggu: e.hariPerMinggu != null ? e.hariPerMinggu : null,
				fuelTypeId: e.fuelTypeId || null,
				hargaBahanBakar: e.hargaBahanBakar != null ? e.hargaBahanBakar : null,
				konsumsiPerBulan:
					e.konsumsiPerBulan != null ? e.konsumsiPerBulan : null,
				konsumsiUnitId: e.konsumsiUnitId || null,
				konversiKeGas: e.konversiKeGas != null ? e.konversiKeGas : 0,
			})) || [],
	};
}

export function SurveyKk0Form({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
}: SurveyKk0FormProps) {
	const queryClient = useQueryClient();

	// Master data queries
	const { data: fuelTypes } = $api.useQuery("get", "/api/master/fuel-types");
	const { data: units } = $api.useQuery("get", "/api/master/units");
	const { data: salesUsers } = $api.useQuery("get", "/api/master/sales-users");

	// Save Survey Mutation
	const saveMutation = $api.useMutation("put", "/api/companies/{id}/survey", {
		onSuccess: () => {
			toast.success("Data Survei KK0 berhasil disimpan!");
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
					"/api/companies/{id}/survey",
					{ params: { path: { id: companyId } } },
				],
			});
			onSaved?.();
		},
		onError: (error) => {
			toast.error(
				error instanceof Error
					? error.message
					: "Gagal menyimpan data Survei KK0",
			);
		},
	});

	const form = useForm({
		defaultValues: getDefaultValues(initialData),
		validators: {
			onChange: surveyKk0Schema,
		},
		onSubmit: async ({ value }) => {
			const request: SaveSurveyRequest = {
				tanggalSurvey: value.tanggalSurvey || null,
				surveyorUserId: value.surveyorUserId || null,
				jumlahKaryawan: value.jumlahKaryawan
					? Number(value.jumlahKaryawan)
					: null,
				jumlahShift: value.jumlahShift ? Number(value.jumlahShift) : null,
				jamKerjaPerHari: value.jamKerjaPerHari
					? Number(value.jamKerjaPerHari)
					: null,
				hariPerMinggu: value.hariPerMinggu ? Number(value.hariPerMinggu) : null,
				bebanPuncak1Mulai: value.bebanPuncak1Mulai || null,
				bebanPuncak1Selesai: value.bebanPuncak1Selesai || null,
				bebanPuncak2Mulai: value.bebanPuncak2Mulai || null,
				bebanPuncak2Selesai: value.bebanPuncak2Selesai || null,
				kebutuhanEnergi: value.kebutuhanEnergi
					? (value.kebutuhanEnergi as KebutuhanEnergiJenis)
					: null,
				kebutuhanEnergiLainnya: value.kebutuhanEnergiLainnya || null,
				kapasitasNilai: value.kapasitasNilai
					? Number(value.kapasitasNilai)
					: null,
				kapasitasUnitId: value.kapasitasUnitId || null,
				pemakaianNilai: value.pemakaianNilai
					? Number(value.pemakaianNilai)
					: null,
				pemakaianUnitId: value.pemakaianUnitId || null,
				pipaTerdekatJarakM: value.pipaTerdekatJarakM
					? Number(value.pipaTerdekatJarakM)
					: null,
				pipaTerdekatDiameter: value.pipaTerdekatDiameter
					? Number(value.pipaTerdekatDiameter)
					: null,
				pipaTerdekatTekanan: value.pipaTerdekatTekanan
					? Number(value.pipaTerdekatTekanan)
					: null,
				bahanBakarEksisting: value.bahanBakarEksisting
					? (value.bahanBakarEksisting as BahanBakarEksisting)
					: null,
				namaPemasok: value.namaPemasok || null,
				kapasitasListrikKw: value.kapasitasListrikKw
					? Number(value.kapasitasListrikKw)
					: null,
				pemakaianListrikKwh: value.pemakaianListrikKwh
					? Number(value.pemakaianListrikKwh)
					: null,
				rencanaPemanfaatanGas: value.rencanaPemanfaatanGas || null,
				deskripsiProsesProduksi: value.deskripsiProsesProduksi || null,
				minEfisiensiDiharapkanPct: value.minEfisiensiDiharapkanPct
					? Number(value.minEfisiensiDiharapkanPct)
					: null,
				willingnessToPayUsdMmbtu: value.willingnessToPayUsdMmbtu
					? Number(value.willingnessToPayUsdMmbtu)
					: null,
				keteranganLain: value.keteranganLain || null,
			};

			const products: SaveSurveyProductRequest[] = (value.products || []).map(
				(p) => ({
					produk: p.produk,
					kapasitas: p.kapasitas != null ? Number(p.kapasitas) : null,
					hargaProduk: p.hargaProduk != null ? Number(p.hargaProduk) : null,
					catatan: p.catatan || null,
				}),
			);

			const rawMaterials: SaveSurveyRawMaterialRequest[] = (
				value.rawMaterials || []
			).map((m) => ({
				bahan: m.bahan || null,
				asal: m.asal ? (m.asal as Asal) : null,
				countryId: m.countryId || null,
				volume: m.volume != null ? Number(m.volume) : null,
				satuanUnitId: m.satuanUnitId || null,
			}));

			const markets: SaveSurveyMarketRequest[] = (value.markets || []).map(
				(m) => ({
					bahan: m.bahan || null,
					asal: m.asal ? (m.asal as Asal) : null,
					countryId: m.countryId || null,
					volume: m.volume != null ? Number(m.volume) : null,
					satuanUnitId: m.satuanUnitId || null,
				}),
			);

			const equipment: SaveSurveyEquipmentRequest[] = (
				value.equipment || []
			).map((e) => ({
				jenisPeralatan: e.jenisPeralatan,
				kapasitas: e.kapasitas != null ? Number(e.kapasitas) : null,
				kapasitasUnitId: e.kapasitasUnitId || null,
				jamPerHari: e.jamPerHari != null ? Number(e.jamPerHari) : null,
				hariPerMinggu: e.hariPerMinggu != null ? Number(e.hariPerMinggu) : null,
				fuelTypeId: e.fuelTypeId || null,
				hargaBahanBakar:
					e.hargaBahanBakar != null ? Number(e.hargaBahanBakar) : null,
				konsumsiPerBulan:
					e.konsumsiPerBulan != null ? Number(e.konsumsiPerBulan) : null,
				konsumsiUnitId: e.konsumsiUnitId || null,
				konversiKeGas: Number(e.konversiKeGas) || 0,
			}));

			await saveMutation.mutateAsync({
				params: { path: { id: companyId } },
				body: {
					request,
					products,
					rawMaterials,
					markets,
					equipment,
				},
			});
		},
	});

	// Synchronize when initialData updates
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
					<div className="size-10 rounded-full bg-emerald-100 dark:bg-emerald-900/40 text-emerald-600 flex items-center justify-center">
						<Flame className="size-5" />
					</div>
					<div>
						<h3 className="text-sm font-semibold">
							Formulir Survei Calon Pelanggan (KK0 / Lampiran 10)
						</h3>
						<form.Subscribe
							selector={(state) => {
								const eq = state.values.equipment || [];
								return eq.reduce(
									(sum, item) => sum + (Number(item.konversiKeGas) || 0),
									0,
								);
							}}
						>
							{(totalGasConversion) => (
								<p className="text-xs text-muted-foreground">
									Total Konversi Gas:{" "}
									<strong className="text-foreground font-mono">
										{totalGasConversion.toLocaleString("id-ID", {
											maximumFractionDigits: 2,
										})}{" "}
										MMBTU/Bulan
									</strong>
								</p>
							)}
						</form.Subscribe>
					</div>
				</div>

				<div className="flex items-center gap-2">
					<DocumentDownloadButton
						companyId={companyId}
						documentType="kk0"
						label="Unduh Formulir KK0 (.docx)"
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
									className="h-9 text-xs flex items-center gap-1.5 bg-emerald-600 hover:bg-emerald-700 text-white"
								>
									{isSubmitting ? (
										<Loader2 className="size-3.5 animate-spin" />
									) : (
										<Save className="size-3.5" />
									)}
									Simpan Data Survei KK0
								</Button>
							)}
						</form.Subscribe>
					)}
				</div>
			</div>

			{/* SECTION 1: PELAKSANAAN & OPERASIONAL */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						1. Data Pelaksanaan Survei & Jam Kerja Operasional
					</CardTitle>
					<CardDescription className="text-xs">
						Petugas surveyor dan jadwal operasional harian pabrik/fasilitas
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Tanggal Survey */}
						<form.Field name="tanggalSurvey">
							{(field) => (
								<FormField label="Tanggal Survei">
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

						{/* Surveyor */}
						<form.Field name="surveyorUserId">
							{(field) => (
								<FormField label="Petugas Surveyor">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Surveyor" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											{salesUsers?.map((u) => (
												<SelectItem key={u.id} value={u.id}>
													{u.fullName}
												</SelectItem>
											))}
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Jumlah Karyawan */}
						<form.Field name="jumlahKaryawan">
							{(field) => (
								<FormField label="Jumlah Karyawan">
									<Input
										type="number"
										placeholder="contoh: 250"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Jumlah Shift */}
						<form.Field name="jumlahShift">
							{(field) => (
								<FormField label="Jumlah Shift / Hari">
									<Input
										type="number"
										placeholder="contoh: 3"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Jam Kerja Per Hari */}
						<form.Field name="jamKerjaPerHari">
							{(field) => (
								<FormField label="Jam Kerja / Hari">
									<Input
										type="number"
										step="0.5"
										placeholder="contoh: 24"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Hari Kerja Per Minggu */}
						<form.Field name="hariPerMinggu">
							{(field) => (
								<FormField label="Hari Kerja / Minggu">
									<Input
										type="number"
										placeholder="contoh: 7"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					{/* Beban Puncak */}
					<div className="pt-2 border-t">
						<Label className="text-xs font-semibold mb-2 block text-muted-foreground">
							Jadwal Beban Puncak Operasi
						</Label>
						<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
							<form.Field name="bebanPuncak1Mulai">
								{(field) => (
									<FormField label="Beban Puncak 1 (Mulai)">
										<Input
											type="time"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="bebanPuncak1Selesai">
								{(field) => (
									<FormField label="Beban Puncak 1 (Selesai)">
										<Input
											type="time"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="bebanPuncak2Mulai">
								{(field) => (
									<FormField label="Beban Puncak 2 (Mulai)">
										<Input
											type="time"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="bebanPuncak2Selesai">
								{(field) => (
									<FormField label="Beban Puncak 2 (Selesai)">
										<Input
											type="time"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: KEBUTUHAN ENERGI & KEDEKATAN JALUR PIPA */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						2. Kebutuhan Energi & Kedekatan Jalur Pipa
					</CardTitle>
					<CardDescription className="text-xs">
						Profil konsumsi daya, kapasitas terpasang, dan spesifikasi pipa gas
						terdekat
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Jenis Kebutuhan Energi */}
						<form.Field name="kebutuhanEnergi">
							{(field) => (
								<FormField label="Jenis Kebutuhan Energi">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Jenis Energi" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Listrik">Listrik</SelectItem>
											<SelectItem value="Panas">Panas</SelectItem>
											<SelectItem value="BahanBaku">Bahan Baku</SelectItem>
											<SelectItem value="Lainnya">Lainnya</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Kebutuhan Energi Lainnya */}
						<form.Field name="kebutuhanEnergiLainnya">
							{(field) => (
								<FormField label="Keterangan Energi Lainnya">
									<Input
										placeholder="Diisi jika memilih 'Lainnya'"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Kapasitas Terpasang */}
						<div className="grid grid-cols-2 gap-2">
							<form.Field name="kapasitasNilai">
								{(field) => (
									<FormField label="Kapasitas Nilai">
										<Input
											type="number"
											step="0.01"
											placeholder="Kapasitas"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>
							<form.Field name="kapasitasUnitId">
								{(field) => (
									<FormField label="Satuan">
										<Select
											value={field.state.value || "NONE"}
											onValueChange={(val) =>
												field.handleChange(val === "NONE" ? "" : val)
											}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-9">
												<SelectValue placeholder="Satuan" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">-</SelectItem>
												{units?.map((u) => (
													<SelectItem key={u.id} value={u.id}>
														{u.code}
													</SelectItem>
												))}
											</SelectContent>
										</Select>
									</FormField>
								)}
							</form.Field>
						</div>

						{/* Pemakaian Eksisting */}
						<div className="grid grid-cols-2 gap-2">
							<form.Field name="pemakaianNilai">
								{(field) => (
									<FormField label="Pemakaian Nilai">
										<Input
											type="number"
											step="0.01"
											placeholder="Pemakaian"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>
							<form.Field name="pemakaianUnitId">
								{(field) => (
									<FormField label="Satuan">
										<Select
											value={field.state.value || "NONE"}
											onValueChange={(val) =>
												field.handleChange(val === "NONE" ? "" : val)
											}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-9">
												<SelectValue placeholder="Satuan" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="NONE">-</SelectItem>
												{units?.map((u) => (
													<SelectItem key={u.id} value={u.id}>
														{u.code}
													</SelectItem>
												))}
											</SelectContent>
										</Select>
									</FormField>
								)}
							</form.Field>
						</div>

						{/* Bahan Bakar Eksisting */}
						<form.Field name="bahanBakarEksisting">
							{(field) => (
								<FormField label="Bahan Bakar Eksisting">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Bahan Bakar" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="Batubara">Batubara</SelectItem>
											<SelectItem value="Solar">Solar</SelectItem>
											<SelectItem value="Biomass">Biomass</SelectItem>
											<SelectItem value="LPG">LPG</SelectItem>
											<SelectItem value="CNG">CNG</SelectItem>
											<SelectItem value="MFO">MFO</SelectItem>
											<SelectItem value="KayuBakar">Kayu Bakar</SelectItem>
											<SelectItem value="Lainnya">Lainnya</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Nama Pemasok */}
						<form.Field name="namaPemasok">
							{(field) => (
								<FormField label="Nama Pemasok Bahan Bakar">
									<Input
										placeholder="contoh: PT Pemasok Energi Nusantara"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Kapasitas Listrik (kW) */}
						<form.Field name="kapasitasListrikKw">
							{(field) => (
								<FormField label="Kapasitas Listrik (kW)">
									<Input
										type="number"
										step="0.01"
										placeholder="contoh: 500"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Pemakaian Listrik (kWh) */}
						<form.Field name="pemakaianListrikKwh">
							{(field) => (
								<FormField label="Pemakaian Listrik (kWh/bln)">
									<Input
										type="number"
										step="0.01"
										placeholder="contoh: 120000"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					{/* Pipeline Proximity */}
					<div className="pt-2 border-t">
						<Label className="text-xs font-semibold mb-2 block text-muted-foreground">
							Kedekatan dengan Pipa Eksisting PGN
						</Label>
						<div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
							<form.Field name="pipaTerdekatJarakM">
								{(field) => (
									<FormField label="Jarak Pipa Terdekat (meter)">
										<Input
											type="number"
											step="0.1"
											placeholder="contoh: 150"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="pipaTerdekatDiameter">
								{(field) => (
									<FormField label="Diameter Pipa (inch)">
										<Input
											type="number"
											step="0.1"
											placeholder="contoh: 4"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>

							<form.Field name="pipaTerdekatTekanan">
								{(field) => (
									<FormField label="Tekanan Pipa (barg)">
										<Input
											type="number"
											step="0.1"
											placeholder="contoh: 16"
											value={field.state.value}
											onChange={(e) => field.handleChange(e.target.value)}
											disabled={!canEdit}
											className="text-xs h-9"
										/>
									</FormField>
								)}
							</form.Field>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: RENCANA PEMANFAATAN GAS & KEEKONOMIAN */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						3. Rencana Pemanfaatan Gas & Keekonomian
					</CardTitle>
					<CardDescription className="text-xs">
						Ekspektasi efisiensi, kesanggupan harga, dan rincian proses produksi
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Rencana Pemanfaatan Gas */}
						<form.Field name="rencanaPemanfaatanGas">
							{(field) => (
								<FormField label="Rencana Pemanfaatan Gas">
									<Select
										value={field.state.value || "NONE"}
										onValueChange={(val) =>
											field.handleChange(val === "NONE" ? "" : val)
										}
										disabled={!canEdit}
									>
										<SelectTrigger className="text-xs h-9">
											<SelectValue placeholder="Pilih Pemanfaatan Gas" />
										</SelectTrigger>
										<SelectContent>
											<SelectItem value="NONE">Belum Dipilih</SelectItem>
											<SelectItem value="SubstitusiBahanBakar">
												Substitusi Bahan Bakar
											</SelectItem>
											<SelectItem value="PembangkitListrik">
												Pembangkit Listrik / Genset
											</SelectItem>
											<SelectItem value="BahanBaku">Bahan Baku</SelectItem>
											<SelectItem value="EkspansiPabrik">
												Ekspansi Pabrik Baru
											</SelectItem>
											<SelectItem value="Lainnya">Lainnya</SelectItem>
										</SelectContent>
									</Select>
								</FormField>
							)}
						</form.Field>

						{/* Min Efisiensi Diharapkan (%) */}
						<form.Field name="minEfisiensiDiharapkanPct">
							{(field) => (
								<FormField label="Min Efisiensi Diharapkan (%)">
									<Input
										type="number"
										step="0.1"
										placeholder="contoh: 15"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Willingness To Pay (USD/MMBTU) */}
						<form.Field name="willingnessToPayUsdMmbtu">
							{(field) => (
								<FormField label="Willingness to Pay (USD/MMBTU)">
									<Input
										type="number"
										step="0.01"
										placeholder="contoh: 8.50"
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs h-9"
									/>
								</FormField>
							)}
						</form.Field>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
						{/* Deskripsi Proses Produksi */}
						<form.Field name="deskripsiProsesProduksi">
							{(field) => (
								<FormField label="Deskripsi Proses Produksi">
									<Textarea
										placeholder="Jelaskan alur proses produksi dan titik utilisasi panas/energi..."
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs min-h-[70px]"
									/>
								</FormField>
							)}
						</form.Field>

						{/* Keterangan Lain */}
						<form.Field name="keteranganLain">
							{(field) => (
								<FormField label="Keterangan Tambahan">
									<Textarea
										placeholder="Catatan khusus kondisi lapangan, akses lahan, perizinan, dll..."
										value={field.state.value}
										onChange={(e) => field.handleChange(e.target.value)}
										disabled={!canEdit}
										className="text-xs min-h-[70px]"
									/>
								</FormField>
							)}
						</form.Field>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 4: PERALATAN DAN KONVERSI GAS */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<Zap className="size-4 text-amber-500" />
							4. Daftar Peralatan Pembakar & Konversi ke Gas
						</CardTitle>
						<CardDescription className="text-xs">
							Boiler, burner, kiln, genset dan estimasi konsumsi gas bumi
							(MMBTU/bln)
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							size="sm"
							variant="outline"
							onClick={() => {
								const current = form.getFieldValue("equipment") || [];
								form.setFieldValue("equipment", [
									...current,
									{
										id: crypto.randomUUID(),
										jenisPeralatan: "",
										kapasitas: null,
										kapasitasUnitId: null,
										jamPerHari: null,
										hariPerMinggu: null,
										fuelTypeId: null,
										hargaBahanBakar: null,
										konsumsiPerBulan: null,
										konsumsiUnitId: null,
										konversiKeGas: 0,
									},
								]);
							}}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" />
							Tambah Alat
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<form.Field name="equipment">
						{(field) => {
							const equipmentList = field.state.value || [];
							return (
								<div className="overflow-x-auto">
									<Table>
										<TableHeader>
											<TableRow className="text-xs">
												<TableHead>Jenis Peralatan</TableHead>
												<TableHead>Kapasitas & Satuan</TableHead>
												<TableHead>Jam/Hari</TableHead>
												<TableHead>Hari/Mgg</TableHead>
												<TableHead>Bahan Bakar</TableHead>
												<TableHead>Konsumsi / Bln</TableHead>
												<TableHead>Konversi Gas (MMBTU/Bln)</TableHead>
												{canEdit && <TableHead className="w-10" />}
											</TableRow>
										</TableHeader>
										<TableBody>
											{equipmentList.length === 0 ? (
												<TableRow>
													<TableCell
														colSpan={8}
														className="text-center py-6 text-xs text-muted-foreground"
													>
														Belum ada data peralatan pembakar. Klik &apos;Tambah
														Alat&apos; untuk menambahkan.
													</TableCell>
												</TableRow>
											) : (
												equipmentList.map((item, idx) => (
													<TableRow key={item.id || `eq-${idx}`}>
														<TableCell className="min-w-[140px]">
															<Input
																placeholder="Boiler 1"
																value={item.jenisPeralatan}
																onChange={(e) => {
																	const next = [...equipmentList];
																	next[idx] = {
																		...item,
																		jenisPeralatan: e.target.value,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell className="min-w-[160px]">
															<div className="flex gap-1">
																<Input
																	type="number"
																	placeholder="Nilai"
																	value={
																		item.kapasitas != null
																			? String(item.kapasitas)
																			: ""
																	}
																	onChange={(e) => {
																		const next = [...equipmentList];
																		next[idx] = {
																			...item,
																			kapasitas: e.target.value
																				? Number(e.target.value)
																				: null,
																		};
																		field.handleChange(next);
																	}}
																	disabled={!canEdit}
																	className="text-xs h-8 w-20"
																/>
																<Select
																	value={item.kapasitasUnitId || "NONE"}
																	onValueChange={(val) => {
																		const next = [...equipmentList];
																		next[idx] = {
																			...item,
																			kapasitasUnitId:
																				val === "NONE" ? null : val,
																		};
																		field.handleChange(next);
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
																				{u.code}
																			</SelectItem>
																		))}
																	</SelectContent>
																</Select>
															</div>
														</TableCell>
														<TableCell className="w-20">
															<Input
																type="number"
																step="0.5"
																placeholder="24"
																value={
																	item.jamPerHari != null
																		? String(item.jamPerHari)
																		: ""
																}
																onChange={(e) => {
																	const next = [...equipmentList];
																	next[idx] = {
																		...item,
																		jamPerHari: e.target.value
																			? Number(e.target.value)
																			: null,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell className="w-20">
															<Input
																type="number"
																placeholder="7"
																value={
																	item.hariPerMinggu != null
																		? String(item.hariPerMinggu)
																		: ""
																}
																onChange={(e) => {
																	const next = [...equipmentList];
																	next[idx] = {
																		...item,
																		hariPerMinggu: e.target.value
																			? Number(e.target.value)
																			: null,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8"
															/>
														</TableCell>
														<TableCell className="min-w-[130px]">
															<Select
																value={item.fuelTypeId || "NONE"}
																onValueChange={(val) => {
																	const next = [...equipmentList];
																	next[idx] = {
																		...item,
																		fuelTypeId: val === "NONE" ? null : val,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
															>
																<SelectTrigger className="text-xs h-8">
																	<SelectValue placeholder="Bahan Bakar" />
																</SelectTrigger>
																<SelectContent>
																	<SelectItem value="NONE">-</SelectItem>
																	{fuelTypes?.map((f) => (
																		<SelectItem key={f.id} value={f.id}>
																			{f.name}
																		</SelectItem>
																	))}
																</SelectContent>
															</Select>
														</TableCell>
														<TableCell className="min-w-[150px]">
															<div className="flex gap-1">
																<Input
																	type="number"
																	step="0.01"
																	placeholder="Konsumsi"
																	value={
																		item.konsumsiPerBulan != null
																			? String(item.konsumsiPerBulan)
																			: ""
																	}
																	onChange={(e) => {
																		const next = [...equipmentList];
																		next[idx] = {
																			...item,
																			konsumsiPerBulan: e.target.value
																				? Number(e.target.value)
																				: null,
																		};
																		field.handleChange(next);
																	}}
																	disabled={!canEdit}
																	className="text-xs h-8 w-20"
																/>
																<Select
																	value={item.konsumsiUnitId || "NONE"}
																	onValueChange={(val) => {
																		const next = [...equipmentList];
																		next[idx] = {
																			...item,
																			konsumsiUnitId:
																				val === "NONE" ? null : val,
																		};
																		field.handleChange(next);
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
																				{u.code}
																			</SelectItem>
																		))}
																	</SelectContent>
																</Select>
															</div>
														</TableCell>
														<TableCell className="min-w-[130px]">
															<Input
																type="number"
																step="0.01"
																placeholder="0.00"
																value={
																	item.konversiKeGas != null
																		? String(item.konversiKeGas)
																		: ""
																}
																onChange={(e) => {
																	const next = [...equipmentList];
																	next[idx] = {
																		...item,
																		konversiKeGas: e.target.value
																			? Number(e.target.value)
																			: 0,
																	};
																	field.handleChange(next);
																}}
																disabled={!canEdit}
																className="text-xs h-8 font-mono font-medium text-emerald-600 dark:text-emerald-400"
															/>
														</TableCell>
														{canEdit && (
															<TableCell>
																<Button
																	type="button"
																	size="icon"
																	variant="ghost"
																	onClick={() => {
																		field.handleChange(
																			equipmentList.filter((_, i) => i !== idx),
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

			{/* SECTION 5: PRODUK, BAHAN BAKU & PASAR */}
			<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
				{/* 5A. Produk */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-sm font-semibold">
								5A. Produk yang Dihasilkan
							</CardTitle>
						</div>
						{canEdit && (
							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => {
									const current = form.getFieldValue("products") || [];
									form.setFieldValue("products", [
										...current,
										{
											id: crypto.randomUUID(),
											produk: "",
											kapasitas: null,
											hargaProduk: null,
											catatan: null,
										},
									]);
								}}
								className="h-7 text-xs flex items-center gap-1"
							>
								<Plus className="size-3" />
								Tambah
							</Button>
						)}
					</CardHeader>
					<CardContent>
						<form.Field name="products">
							{(field) => {
								const productList = field.state.value || [];
								return (
									<div className="space-y-3">
										{productList.length === 0 ? (
											<p className="text-xs text-muted-foreground text-center py-4">
												Belum ada data produk.
											</p>
										) : (
											productList.map((item, idx) => (
												<div
													key={item.id || `prod-${idx}`}
													className="p-3 border rounded-md space-y-2 bg-muted/20 relative"
												>
													{canEdit && (
														<Button
															type="button"
															size="icon"
															variant="ghost"
															onClick={() => {
																field.handleChange(
																	productList.filter((_, i) => i !== idx),
																);
															}}
															className="size-6 text-destructive absolute top-2 right-2"
														>
															<Trash2 className="size-3" />
														</Button>
													)}
													<Input
														placeholder="Nama Produk"
														value={item.produk}
														onChange={(e) => {
															const next = [...productList];
															next[idx] = { ...item, produk: e.target.value };
															field.handleChange(next);
														}}
														disabled={!canEdit}
														className="text-xs h-7"
													/>
													<div className="grid grid-cols-2 gap-2">
														<Input
															type="number"
															placeholder="Kapasitas/bln"
															value={
																item.kapasitas != null
																	? String(item.kapasitas)
																	: ""
															}
															onChange={(e) => {
																const next = [...productList];
																next[idx] = {
																	...item,
																	kapasitas: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															disabled={!canEdit}
															className="text-xs h-7"
														/>
														<Input
															type="number"
															placeholder="Harga (Rp)"
															value={
																item.hargaProduk != null
																	? String(item.hargaProduk)
																	: ""
															}
															onChange={(e) => {
																const next = [...productList];
																next[idx] = {
																	...item,
																	hargaProduk: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															disabled={!canEdit}
															className="text-xs h-7"
														/>
													</div>
												</div>
											))
										)}
									</div>
								);
							}}
						</form.Field>
					</CardContent>
				</Card>

				{/* 5B. Bahan Baku */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-sm font-semibold">
								5B. Bahan Baku Utama
							</CardTitle>
						</div>
						{canEdit && (
							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => {
									const current = form.getFieldValue("rawMaterials") || [];
									form.setFieldValue("rawMaterials", [
										...current,
										{
											id: crypto.randomUUID(),
											bahan: "",
											asal: "Lokal",
											countryId: null,
											volume: null,
											satuanUnitId: null,
										},
									]);
								}}
								className="h-7 text-xs flex items-center gap-1"
							>
								<Plus className="size-3" />
								Tambah
							</Button>
						)}
					</CardHeader>
					<CardContent>
						<form.Field name="rawMaterials">
							{(field) => {
								const rawList = field.state.value || [];
								return (
									<div className="space-y-3">
										{rawList.length === 0 ? (
											<p className="text-xs text-muted-foreground text-center py-4">
												Belum ada data bahan baku.
											</p>
										) : (
											rawList.map((item, idx) => (
												<div
													key={item.id || `raw-${idx}`}
													className="p-3 border rounded-md space-y-2 bg-muted/20 relative"
												>
													{canEdit && (
														<Button
															type="button"
															size="icon"
															variant="ghost"
															onClick={() => {
																field.handleChange(
																	rawList.filter((_, i) => i !== idx),
																);
															}}
															className="size-6 text-destructive absolute top-2 right-2"
														>
															<Trash2 className="size-3" />
														</Button>
													)}
													<Input
														placeholder="Nama Bahan Baku"
														value={item.bahan || ""}
														onChange={(e) => {
															const next = [...rawList];
															next[idx] = { ...item, bahan: e.target.value };
															field.handleChange(next);
														}}
														disabled={!canEdit}
														className="text-xs h-7"
													/>
													<div className="grid grid-cols-2 gap-2">
														<Select
															value={item.asal || "Lokal"}
															onValueChange={(val) => {
																const next = [...rawList];
																next[idx] = { ...item, asal: val as Asal };
																field.handleChange(next);
															}}
															disabled={!canEdit}
														>
															<SelectTrigger className="text-xs h-7">
																<SelectValue placeholder="Asal" />
															</SelectTrigger>
															<SelectContent>
																<SelectItem value="Lokal">Lokal</SelectItem>
																<SelectItem value="Impor">Impor</SelectItem>
															</SelectContent>
														</Select>
														<Input
															type="number"
															placeholder="Volume"
															value={
																item.volume != null ? String(item.volume) : ""
															}
															onChange={(e) => {
																const next = [...rawList];
																next[idx] = {
																	...item,
																	volume: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															disabled={!canEdit}
															className="text-xs h-7"
														/>
													</div>
												</div>
											))
										)}
									</div>
								);
							}}
						</form.Field>
					</CardContent>
				</Card>

				{/* 5C. Pasar */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-sm font-semibold">
								5C. Target Pasar Produk
							</CardTitle>
						</div>
						{canEdit && (
							<Button
								type="button"
								size="sm"
								variant="outline"
								onClick={() => {
									const current = form.getFieldValue("markets") || [];
									form.setFieldValue("markets", [
										...current,
										{
											id: crypto.randomUUID(),
											bahan: "",
											asal: "Lokal",
											countryId: null,
											volume: null,
											satuanUnitId: null,
										},
									]);
								}}
								className="h-7 text-xs flex items-center gap-1"
							>
								<Plus className="size-3" />
								Tambah
							</Button>
						)}
					</CardHeader>
					<CardContent>
						<form.Field name="markets">
							{(field) => {
								const marketList = field.state.value || [];
								return (
									<div className="space-y-3">
										{marketList.length === 0 ? (
											<p className="text-xs text-muted-foreground text-center py-4">
												Belum ada data target pasar.
											</p>
										) : (
											marketList.map((item, idx) => (
												<div
													key={item.id || `mkt-${idx}`}
													className="p-3 border rounded-md space-y-2 bg-muted/20 relative"
												>
													{canEdit && (
														<Button
															type="button"
															size="icon"
															variant="ghost"
															onClick={() => {
																field.handleChange(
																	marketList.filter((_, i) => i !== idx),
																);
															}}
															className="size-6 text-destructive absolute top-2 right-2"
														>
															<Trash2 className="size-3" />
														</Button>
													)}
													<Input
														placeholder="Nama Komoditas / Pasar"
														value={item.bahan || ""}
														onChange={(e) => {
															const next = [...marketList];
															next[idx] = { ...item, bahan: e.target.value };
															field.handleChange(next);
														}}
														disabled={!canEdit}
														className="text-xs h-7"
													/>
													<div className="grid grid-cols-2 gap-2">
														<Select
															value={item.asal || "Lokal"}
															onValueChange={(val) => {
																const next = [...marketList];
																next[idx] = { ...item, asal: val as Asal };
																field.handleChange(next);
															}}
															disabled={!canEdit}
														>
															<SelectTrigger className="text-xs h-7">
																<SelectValue placeholder="Pasar" />
															</SelectTrigger>
															<SelectContent>
																<SelectItem value="Lokal">Domestik</SelectItem>
																<SelectItem value="Impor">Ekspor</SelectItem>
															</SelectContent>
														</Select>
														<Input
															type="number"
															placeholder="Volume"
															value={
																item.volume != null ? String(item.volume) : ""
															}
															onChange={(e) => {
																const next = [...marketList];
																next[idx] = {
																	...item,
																	volume: e.target.value
																		? Number(e.target.value)
																		: null,
																};
																field.handleChange(next);
															}}
															disabled={!canEdit}
															className="text-xs h-7"
														/>
													</div>
												</div>
											))
										)}
									</div>
								);
							}}
						</form.Field>
					</CardContent>
				</Card>
			</div>
		</form>
	);
}
