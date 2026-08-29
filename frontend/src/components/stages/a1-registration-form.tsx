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
	A1UsagePeriodDetail,
	BasisKontrak,
	HargaCurrency,
	HargaUnit,
	RegistrasiSource,
	SaveA1RegistrationRequest,
	Sektor,
	SignatureMethod,
	SkemaHarga,
	StatusBangunan,
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

interface A1RegistrationFormProps {
	companyId: string;
	initialData?: A1RegistrationDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
}

export function A1RegistrationForm({
	companyId,
	initialData,
	canEdit = true,
	onSaved,
}: A1RegistrationFormProps) {
	const queryClient = useQueryClient();
	const { data: segments } = $api.useQuery("get", "/api/master/segments");

	// Form State
	const [tanggalRegistrasi, setTanggalRegistrasi] = React.useState<string>(
		initialData?.tanggalRegistrasi || "",
	);
	const [registrasiSource, setRegistrasiSource] =
		React.useState<RegistrasiSource>(initialData?.registrasiSource || "Manual");
	const [namaPenanggungJawab, setNamaPenanggungJawab] = React.useState<string>(
		initialData?.namaPenanggungJawab || "",
	);
	const [jabatan, setJabatan] = React.useState<string>(
		initialData?.jabatan || "",
	);
	const [bulanDimulai, setBulanDimulai] = React.useState<string>(
		initialData?.bulanDimulai || "",
	);
	const [basisKontrak, setBasisKontrak] = React.useState<BasisKontrak | "">(
		initialData?.basisKontrak || "Bulanan",
	);
	const [skemaHarga, setSkemaHarga] = React.useState<SkemaHarga | "">(
		initialData?.skemaHarga || "Reguler",
	);
	const [segmentId, setSegmentId] = React.useState<string>(
		initialData?.segmentId || "",
	);
	const [kodeHarga, setKodeHarga] = React.useState<string>(
		initialData?.kodeHarga || "",
	);
	const [hargaCurrency, setHargaCurrency] = React.useState<HargaCurrency | "">(
		initialData?.hargaCurrency || "USD",
	);
	const [hargaUnit, setHargaUnit] = React.useState<HargaUnit | "">(
		initialData?.hargaUnit || "MMBtu",
	);
	const [hargaNilai, setHargaNilai] = React.useState<string>(
		initialData?.hargaNilai != null ? String(initialData.hargaNilai) : "",
	);
	const [capexAwal, setCapexAwal] = React.useState<string>(
		initialData?.capexAwal != null ? String(initialData.capexAwal) : "",
	);
	const [momSigasTersedia, setMomSigasTersedia] = React.useState<boolean>(
		initialData?.momSigasTersedia ?? false,
	);
	const [statusBangunan, setStatusBangunan] = React.useState<
		StatusBangunan | ""
	>(initialData?.statusBangunan || "Eksisting");
	const [sektor, setSektor] = React.useState<Sektor | "">(
		initialData?.sektor || "Industri",
	);
	const [produksiUtama, setProduksiUtama] = React.useState<string>(
		initialData?.produksiUtama || "",
	);
	const [jenisPeralatanGas, setJenisPeralatanGas] = React.useState<string>(
		initialData?.jenisPeralatanGas || "",
	);
	const [tekananOperasiBarg, setTekananOperasiBarg] = React.useState<string>(
		initialData?.tekananOperasiBarg != null
			? String(initialData.tekananOperasiBarg)
			: "",
	);
	const [signatureMethod, setSignatureMethod] = React.useState<
		SignatureMethod | ""
	>(initialData?.signatureMethod || "Wet");

	// Repeating usage periods
	const [usagePeriods, setUsagePeriods] = React.useState<A1UsagePeriodDetail[]>(
		initialData?.usagePeriods?.map((p, idx) => ({
			id: p.id || crypto.randomUUID(),
			periodeMulai: p.periodeMulai || "",
			periodeSelesai: p.periodeSelesai || "",
			rataRata: p.rataRata != null ? Number(p.rataRata) : 0,
			minimum: p.minimum != null ? Number(p.minimum) : 0,
			maksimum: p.maksimum != null ? Number(p.maksimum) : 0,
			sortOrder: p.sortOrder != null ? Number(p.sortOrder) : idx + 1,
		})) || [],
	);

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			setTanggalRegistrasi(initialData.tanggalRegistrasi || "");
			setRegistrasiSource(initialData.registrasiSource || "Manual");
			setNamaPenanggungJawab(initialData.namaPenanggungJawab || "");
			setJabatan(initialData.jabatan || "");
			setBulanDimulai(initialData.bulanDimulai || "");
			setBasisKontrak(initialData.basisKontrak || "Bulanan");
			setSkemaHarga(initialData.skemaHarga || "Reguler");
			setSegmentId(initialData.segmentId || "");
			setKodeHarga(initialData.kodeHarga || "");
			setHargaCurrency(initialData.hargaCurrency || "USD");
			setHargaUnit(initialData.hargaUnit || "MMBtu");
			setHargaNilai(
				initialData.hargaNilai != null ? String(initialData.hargaNilai) : "",
			);
			setCapexAwal(
				initialData.capexAwal != null ? String(initialData.capexAwal) : "",
			);
			setMomSigasTersedia(initialData.momSigasTersedia ?? false);
			setStatusBangunan(initialData.statusBangunan || "Eksisting");
			setSektor(initialData.sektor || "Industri");
			setProduksiUtama(initialData.produksiUtama || "");
			setJenisPeralatanGas(initialData.jenisPeralatanGas || "");
			setTekananOperasiBarg(
				initialData.tekananOperasiBarg != null
					? String(initialData.tekananOperasiBarg)
					: "",
			);
			setSignatureMethod(initialData.signatureMethod || "Wet");

			if (initialData.usagePeriods) {
				setUsagePeriods(
					initialData.usagePeriods.map((p, idx) => ({
						id: p.id || crypto.randomUUID(),
						periodeMulai: p.periodeMulai || "",
						periodeSelesai: p.periodeSelesai || "",
						rataRata: p.rataRata != null ? Number(p.rataRata) : 0,
						minimum: p.minimum != null ? Number(p.minimum) : 0,
						maksimum: p.maksimum != null ? Number(p.maksimum) : 0,
						sortOrder: p.sortOrder != null ? Number(p.sortOrder) : idx + 1,
					})),
				);
			}
		}
	}, [initialData]);

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

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveA1RegistrationRequest = {
			tanggalRegistrasi: tanggalRegistrasi || null,
			namaPenanggungJawab: namaPenanggungJawab || null,
			jabatan: jabatan || null,
			bulanDimulai: bulanDimulai || null,
			basisKontrak: basisKontrak ? (basisKontrak as BasisKontrak) : null,
			skemaHarga: skemaHarga ? (skemaHarga as SkemaHarga) : null,
			segmentId: segmentId || null,
			kodeHarga: kodeHarga || null,
			hargaNilai: hargaNilai ? Number(hargaNilai) : null,
			hargaCurrency: hargaCurrency ? (hargaCurrency as HargaCurrency) : null,
			hargaUnit: hargaUnit ? (hargaUnit as HargaUnit) : null,
			capexAwal: capexAwal ? Number(capexAwal) : null,
			momSigasTersedia,
			statusBangunan: statusBangunan
				? (statusBangunan as StatusBangunan)
				: null,
			sektor: sektor ? (sektor as Sektor) : null,
			produksiUtama: produksiUtama || null,
			jenisPeralatanGas: jenisPeralatanGas || null,
			tekananOperasiBarg: tekananOperasiBarg
				? Number(tekananOperasiBarg)
				: null,
			signedDocumentId: initialData?.signedDocumentId || null,
			signatureMethod: signatureMethod
				? (signatureMethod as SignatureMethod)
				: null,
			usagePeriods: usagePeriods.map((p, idx) => ({
				id: p.id || crypto.randomUUID(),
				periodeMulai: p.periodeMulai,
				periodeSelesai: p.periodeSelesai,
				rataRata: Number(p.rataRata) || 0,
				minimum: Number(p.minimum) || 0,
				maksimum: Number(p.maksimum) || 0,
				sortOrder: idx + 1,
			})),
		};

		saveMutation.mutate({
			params: { path: { id: companyId } },
			body: request,
		});
	};

	const addPeriodRow = () => {
		setUsagePeriods([
			...usagePeriods,
			{
				id: crypto.randomUUID(),
				periodeMulai: "",
				periodeSelesai: "",
				rataRata: 0,
				minimum: 0,
				maksimum: 0,
				sortOrder: usagePeriods.length + 1,
			},
		]);
	};

	const removePeriodRow = (index: number) => {
		setUsagePeriods(usagePeriods.filter((_, i) => i !== index));
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
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
						<Button
							type="submit"
							size="sm"
							disabled={saveMutation.isPending}
							className="h-9 text-xs flex items-center gap-1.5 bg-blue-600 hover:bg-blue-700 text-white"
						>
							{saveMutation.isPending ? (
								<Loader2 className="size-3.5 animate-spin" />
							) : (
								<Save className="size-3.5" />
							)}
							Simpan Formulir A1
						</Button>
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
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tanggal Registrasi</Label>
							<Input
								type="date"
								value={tanggalRegistrasi}
								onChange={(e) => setTanggalRegistrasi(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Kanal Pendaftaran */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Kanal Pendaftaran</Label>
							<Select
								value={registrasiSource}
								onValueChange={(val) =>
									setRegistrasiSource(val as RegistrasiSource)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="Manual">Manual / Tatap Muka</SelectItem>
									<SelectItem value="Online">Online / Portal</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nama Penanggung Jawab */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nama Penanggung Jawab
							</Label>
							<Input
								value={namaPenanggungJawab}
								onChange={(e) => setNamaPenanggungJawab(e.target.value)}
								placeholder="contoh: Hendra Gunawan"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jabatan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jabatan</Label>
							<Input
								value={jabatan}
								onChange={(e) => setJabatan(e.target.value)}
								placeholder="contoh: Direktur Operasional"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Metode Tanda Tangan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Metode Tanda Tangan</Label>
							<Select
								value={signatureMethod || "NONE"}
								onValueChange={(val) =>
									setSignatureMethod(
										val === "NONE" ? "" : (val as SignatureMethod),
									)
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
						</div>

						{/* MoM SiGas */}
						<div className="space-y-1.5 flex flex-col justify-end">
							<div className="flex items-center space-x-2 pb-2">
								<Switch
									id="momSigas"
									checked={momSigasTersedia}
									onCheckedChange={setMomSigasTersedia}
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
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Rencana Mulai Penyaluran Gas
							</Label>
							<Input
								type="date"
								value={bulanDimulai}
								onChange={(e) => setBulanDimulai(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Basis Kontrak */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Basis Kontrak</Label>
							<Select
								value={basisKontrak || "NONE"}
								onValueChange={(val) =>
									setBasisKontrak(val === "NONE" ? "" : (val as BasisKontrak))
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
						</div>

						{/* Skema Harga */}
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
									<SelectValue placeholder="Pilih Skema Harga" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Dipilih</SelectItem>
									<SelectItem value="Reguler">Reguler</SelectItem>
									<SelectItem value="Sigas">SiGas</SelectItem>
									<SelectItem value="Bersyarat">Bersyarat</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Segment */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Segmen Pelanggan</Label>
							<Select
								value={segmentId || "NONE"}
								onValueChange={(val) => setSegmentId(val === "NONE" ? "" : val)}
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
						</div>

						{/* Kode Harga */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Kode Harga</Label>
							<Input
								value={kodeHarga}
								onChange={(e) => setKodeHarga(e.target.value)}
								placeholder="contoh: IND-1"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Harga Nilai */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tarif / Harga Jual Gas
							</Label>
							<div className="flex gap-2">
								<Input
									type="number"
									step="0.01"
									value={hargaNilai}
									onChange={(e) => setHargaNilai(e.target.value)}
									placeholder="contoh: 9.85"
									disabled={!canEdit}
									className="text-xs h-9 font-mono"
								/>
								<Select
									value={hargaCurrency || "USD"}
									onValueChange={(val) =>
										setHargaCurrency(val as HargaCurrency)
									}
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
								<Select
									value={hargaUnit || "MMBtu"}
									onValueChange={(val) => setHargaUnit(val as HargaUnit)}
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
							</div>
						</div>

						{/* Capex Awal */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Capex Awal / Estimasi (USD)
							</Label>
							<Input
								type="number"
								step="0.01"
								value={capexAwal}
								onChange={(e) => setCapexAwal(e.target.value)}
								placeholder="contoh: 75000"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Status Bangunan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Status Bangunan</Label>
							<Select
								value={statusBangunan || "NONE"}
								onValueChange={(val) =>
									setStatusBangunan(
										val === "NONE" ? "" : (val as StatusBangunan),
									)
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
									<SelectItem value="DalamRencana">Dalam Rencana</SelectItem>
									<SelectItem value="ProsesEkspansi">
										Proses Ekspansi
									</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Sektor */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Sektor</Label>
							<Select
								value={sektor || "NONE"}
								onValueChange={(val) =>
									setSektor(val === "NONE" ? "" : (val as Sektor))
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
						</div>
					</div>

					<div className="grid grid-cols-1 sm:grid-cols-2 gap-4 pt-2 border-t">
						{/* Produksi Utama */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Produksi / Hasil Utama
							</Label>
							<Input
								value={produksiUtama}
								onChange={(e) => setProduksiUtama(e.target.value)}
								placeholder="contoh: Keramik Ubin Lantai"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Tekanan Operasi (Barg) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tekanan Operasi yang Dibutuhkan (Barg)
							</Label>
							<Input
								type="number"
								step="0.1"
								value={tekananOperasiBarg}
								onChange={(e) => setTekananOperasiBarg(e.target.value)}
								placeholder="contoh: 2.0"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
					</div>

					{/* Jenis Peralatan Gas */}
					<div className="space-y-1.5">
						<Label className="text-xs font-medium">
							Jenis Peralatan Gas yang Digunakan
						</Label>
						<Textarea
							value={jenisPeralatanGas}
							onChange={(e) => setJenisPeralatanGas(e.target.value)}
							placeholder="contoh: 2 unit Boiler Miura 5 Ton, 1 unit Burner Riello"
							disabled={!canEdit}
							className="text-xs min-h-[60px]"
						/>
					</div>
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
							onClick={addPeriodRow}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" />
							Tambah Periode
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<div className="rounded-lg border overflow-hidden">
						<Table>
							<TableHeader className="bg-muted/40">
								<TableRow>
									<TableHead className="text-xs">Periode Mulai</TableHead>
									<TableHead className="text-xs">Periode Selesai</TableHead>
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
								{usagePeriods.length === 0 ? (
									<TableRow>
										<TableCell
											colSpan={canEdit ? 6 : 5}
											className="text-center text-xs py-6 text-muted-foreground"
										>
											Belum ada data periode penggunaan gas. Klik "+ Tambah
											Periode" untuk menambahkan.
										</TableCell>
									</TableRow>
								) : (
									usagePeriods.map((row, idx) => (
										// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
										<TableRow key={idx}>
											<TableCell>
												<Input
													type="date"
													value={row.periodeMulai}
													onChange={(e) => {
														const next = [...usagePeriods];
														next[idx].periodeMulai = e.target.value;
														setUsagePeriods(next);
													}}
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="date"
													value={row.periodeSelesai}
													onChange={(e) => {
														const next = [...usagePeriods];
														next[idx].periodeSelesai = e.target.value;
														setUsagePeriods(next);
													}}
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.rataRata}
													onChange={(e) => {
														const next = [...usagePeriods];
														next[idx].rataRata = Number(e.target.value) || 0;
														setUsagePeriods(next);
													}}
													placeholder="0"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.minimum}
													onChange={(e) => {
														const next = [...usagePeriods];
														next[idx].minimum = Number(e.target.value) || 0;
														setUsagePeriods(next);
													}}
													placeholder="0"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.maksimum}
													onChange={(e) => {
														const next = [...usagePeriods];
														next[idx].maksimum = Number(e.target.value) || 0;
														setUsagePeriods(next);
													}}
													placeholder="0"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											{canEdit && (
												<TableCell className="text-center">
													<Button
														type="button"
														variant="ghost"
														size="icon"
														onClick={() => removePeriodRow(idx)}
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

			{canEdit && (
				<div className="flex justify-end pt-2">
					<Button
						type="submit"
						disabled={saveMutation.isPending}
						className="h-9 text-xs flex items-center gap-1.5 bg-blue-600 hover:bg-blue-700 text-white"
					>
						{saveMutation.isPending ? (
							<Loader2 className="size-3.5 animate-spin" />
						) : (
							<Save className="size-3.5" />
						)}
						Simpan Formulir A1
					</Button>
				</div>
			)}
		</form>
	);
}
