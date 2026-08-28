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
	const { data: units } = $api.useQuery("get", "/api/master/units");

	// Form State
	const [source, setSource] = React.useState<RegistrasiSource>(
		initialData?.source || "Offline",
	);
	const [namaPic, setNamaPic] = React.useState<string>(
		initialData?.namaPic || "",
	);
	const [jabatanPic, setJabatanPic] = React.useState<string>(
		initialData?.jabatanPic || "",
	);
	const [teleponPic, setTeleponPic] = React.useState<string>(
		initialData?.teleponPic || "",
	);
	const [emailPic, setEmailPic] = React.useState<string>(
		initialData?.emailPic || "",
	);
	const [bulanTahunMulai, setBulanTahunMulai] = React.useState<string>(
		initialData?.bulanTahunMulai || "",
	);
	const [basisKontrak, setBasisKontrak] = React.useState<BasisKontrak | "">(
		initialData?.basisKontrak || "Bulanan",
	);
	const [skemaHarga, setSkemaHarga] = React.useState<SkemaHarga | "">(
		initialData?.skemaHarga || "Reguler",
	);
	const [hargaCurrency, setHargaCurrency] = React.useState<HargaCurrency | "">(
		initialData?.hargaCurrency || "USD",
	);
	const [hargaUnit, setHargaUnit] = React.useState<HargaUnit | "">(
		initialData?.hargaUnit || "MMBTU",
	);
	const [hargaJualNilai, setHargaJualNilai] = React.useState<string>(
		initialData?.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
	);
	const [statusBangunan, setStatusBangunan] = React.useState<
		StatusBangunan | ""
	>(initialData?.statusBangunan || "MilikSendiri");
	const [sektor, setSektor] = React.useState<Sektor | "">(
		initialData?.sektor || "Industri",
	);
	const [peralatanGas, setPeralatanGas] = React.useState<string>(
		initialData?.peralatanGas || "",
	);
	const [tekananOperasiBarg, setTekananOperasiBarg] = React.useState<string>(
		initialData?.tekananOperasiBarg
			? String(initialData.tekananOperasiBarg)
			: "",
	);
	const [signatureMethod, setSignatureMethod] = React.useState<
		SignatureMethod | ""
	>(initialData?.signatureMethod || "Basah");
	const [isSiGasMom, setIsSiGasMom] = React.useState<boolean>(
		initialData?.isSiGasMom ?? false,
	);
	const [capexAwal, setCapexAwal] = React.useState<string>(
		initialData?.capexAwal ? String(initialData.capexAwal) : "",
	);

	// Repeating usage periods
	const [periods, setPeriods] = React.useState<SaveA1UsagePeriodRequest[]>(
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

	// Synchronize when initialData changes
	React.useEffect(() => {
		if (initialData) {
			setSource(initialData.source || "Offline");
			setNamaPic(initialData.namaPic || "");
			setJabatanPic(initialData.jabatanPic || "");
			setTeleponPic(initialData.teleponPic || "");
			setEmailPic(initialData.emailPic || "");
			setBulanTahunMulai(initialData.bulanTahunMulai || "");
			setBasisKontrak(initialData.basisKontrak || "Bulanan");
			setSkemaHarga(initialData.skemaHarga || "Reguler");
			setHargaCurrency(initialData.hargaCurrency || "USD");
			setHargaUnit(initialData.hargaUnit || "MMBTU");
			setHargaJualNilai(
				initialData.hargaJualNilai ? String(initialData.hargaJualNilai) : "",
			);
			setStatusBangunan(initialData.statusBangunan || "MilikSendiri");
			setSektor(initialData.sektor || "Industri");
			setPeralatanGas(initialData.peralatanGas || "");
			setTekananOperasiBarg(
				initialData.tekananOperasiBarg
					? String(initialData.tekananOperasiBarg)
					: "",
			);
			setSignatureMethod(initialData.signatureMethod || "Basah");
			setIsSiGasMom(initialData.isSiGasMom ?? false);
			setCapexAwal(initialData.capexAwal ? String(initialData.capexAwal) : "");

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
			source,
			namaPic: namaPic || null,
			jabatanPic: jabatanPic || null,
			teleponPic: teleponPic || null,
			emailPic: emailPic || null,
			bulanTahunMulai: bulanTahunMulai || null,
			basisKontrak: basisKontrak ? (basisKontrak as BasisKontrak) : null,
			skemaHarga: skemaHarga ? (skemaHarga as SkemaHarga) : null,
			hargaCurrency: hargaCurrency ? (hargaCurrency as HargaCurrency) : null,
			hargaUnit: hargaUnit ? (hargaUnit as HargaUnit) : null,
			hargaJualNilai: hargaJualNilai ? Number(hargaJualNilai) : null,
			statusBangunan: statusBangunan
				? (statusBangunan as StatusBangunan)
				: null,
			sektor: sektor ? (sektor as Sektor) : null,
			peralatanGas: peralatanGas || null,
			tekananOperasiBarg: tekananOperasiBarg
				? Number(tekananOperasiBarg)
				: null,
			signatureMethod: signatureMethod
				? (signatureMethod as SignatureMethod)
				: null,
			isSiGasMom,
			capexAwal: capexAwal ? Number(capexAwal) : null,
			periods,
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
							Legalitas pelanggan, person in charge (PIC), skema tarif, dan
							periode penggunaan gas
						</p>
					</div>
				</div>

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

			{/* SECTION 1: DATA PIC & SUMBER REGISTRASI */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold flex items-center gap-2">
						<UserCheck className="size-4 text-blue-500" />
						1. Data Kontak Person in Charge (PIC) & Pendaftaran
					</CardTitle>
					<CardDescription className="text-xs">
						Petugas yang berwenang menandatangani kontrak atau mewakili
						pelanggan
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Kanal Pendaftaran */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Kanal Pendaftaran</Label>
							<Select
								value={source}
								onValueChange={(val) => setSource(val as RegistrasiSource)}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="Offline">Offline / Tatap Muka</SelectItem>
									<SelectItem value="Online">Online / Portal</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nama PIC */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Nama Lengkap PIC</Label>
							<Input
								value={namaPic}
								onChange={(e) => setNamaPic(e.target.value)}
								placeholder="contoh: Hendra Gunawan"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jabatan PIC */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jabatan PIC</Label>
							<Input
								value={jabatanPic}
								onChange={(e) => setJabatanPic(e.target.value)}
								placeholder="contoh: Direktur Operasional"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Telepon PIC */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nomor Telepon / WA PIC
							</Label>
							<Input
								value={teleponPic}
								onChange={(e) => setTeleponPic(e.target.value)}
								placeholder="contoh: 08123456789"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Email PIC */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Alamat Email PIC</Label>
							<Input
								type="email"
								value={emailPic}
								onChange={(e) => setEmailPic(e.target.value)}
								placeholder="contoh: pic@perusahaan.co.id"
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
									<SelectItem value="Basah">Tanda Tangan Basah</SelectItem>
									<SelectItem value="Digital">Digital / E-Sign</SelectItem>
								</SelectContent>
							</Select>
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
						{/* Bulan Tahun Mulai */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Rencana Mulai Penyaluran Gas
							</Label>
							<Input
								type="month"
								value={bulanTahunMulai}
								onChange={(e) => setBulanTahunMulai(e.target.value)}
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
									<SelectItem value="MultiYear">Multi-Year</SelectItem>
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
									<SelectItem value="SiGas">SiGas</SelectItem>
									<SelectItem value="Bersyarat">Bersyarat</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Harga Jual Nilai */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Harga Jual Gas</Label>
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

						{/* Mata Uang Harga */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Mata Uang Harga</Label>
							<Select
								value={hargaCurrency || "USD"}
								onValueChange={(val) => setHargaCurrency(val as HargaCurrency)}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="USD">USD ($)</SelectItem>
									<SelectItem value="IDR">IDR (Rp)</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Satuan Harga */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Satuan Volume Gas</Label>
							<Select
								value={hargaUnit || "MMBTU"}
								onValueChange={(val) => setHargaUnit(val as HargaUnit)}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="MMBTU">MMBTU</SelectItem>
									<SelectItem value="M3">M³</SelectItem>
									<SelectItem value="BBTUD">BBTUD</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Status Bangunan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Status Bangunan / Pabrik
							</Label>
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
									<SelectItem value="MilikSendiri">Milik Sendiri</SelectItem>
									<SelectItem value="Sewa">Sewa / Kontrak</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Sektor */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Sektor Usaha</Label>
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
									<SelectItem value="RumahTangga">Rumah Tangga</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Tekanan Operasi (barg) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tekanan Operasi yang Dibutuhkan (barg)
							</Label>
							<Input
								type="number"
								step="0.01"
								value={tekananOperasiBarg}
								onChange={(e) => setTekananOperasiBarg(e.target.value)}
								placeholder="contoh: 2.5"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
					</div>

					{/* Switch SiGas MOM & Capex */}
					<div className="pt-2 border-t flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
						<div className="flex items-center space-x-3">
							<Switch
								id="sigas-mom"
								checked={isSiGasMom}
								onCheckedChange={setIsSiGasMom}
								disabled={!canEdit}
							/>
							<div>
								<Label
									htmlFor="sigas-mom"
									className="text-xs font-semibold cursor-pointer"
								>
									Skema SiGas MOM (Minimum Order Monthly)
								</Label>
								<p className="text-[11px] text-muted-foreground">
									Centang apabila kontrak menggunakan batas minimum penyerapan
									gas
								</p>
							</div>
						</div>

						<div className="w-full sm:w-64 space-y-1">
							<Label className="text-xs font-medium">
								Estimasi Capex Awal (IDR)
							</Label>
							<Input
								type="number"
								value={capexAwal}
								onChange={(e) => setCapexAwal(e.target.value)}
								placeholder="contoh: 150000000"
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
					</div>

					{/* Peralatan Gas */}
					<div className="space-y-1.5 pt-2">
						<Label className="text-xs font-medium">
							Daftar Peralatan Gas yang Digunakan
						</Label>
						<Textarea
							value={peralatanGas}
							onChange={(e) => setPeralatanGas(e.target.value)}
							placeholder="Rincian burner, furnace, boiler yang akan dialiri gas..."
							disabled={!canEdit}
							className="text-xs min-h-[60px]"
						/>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: REPEATING USAGE PERIODS TABLE */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold">
							3. Periode & Profil Pemakaian Gas
						</CardTitle>
						<CardDescription className="text-xs">
							Jadwal komitmen pemakaian volume rata-rata, minimum, dan maksimum
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
							<Plus className="size-3.5" /> Tambah Periode
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
								{periods.length === 0 ? (
									<TableRow>
										<TableCell
											colSpan={canEdit ? 7 : 6}
											className="h-20 text-center text-xs text-muted-foreground"
										>
											Belum ada data periode pemakaian. Klik "+ Tambah Periode"
											untuk menambahkan.
										</TableCell>
									</TableRow>
								) : (
									periods.map((row, idx) => (
										// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
										<TableRow key={idx}>
											<TableCell>
												<Input
													type="date"
													value={row.periodeMulai ?? ""}
													onChange={(e) => {
														const next = [...periods];
														next[idx].periodeMulai =
															e.target.value || undefined;
														setPeriods(next);
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
														const next = [...periods];
														next[idx].periodeSelesai =
															e.target.value || undefined;
														setPeriods(next);
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
														const next = [...periods];
														next[idx].volumeRataRata = e.target.value
															? Number(e.target.value)
															: undefined;
														setPeriods(next);
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
														const next = [...periods];
														next[idx].volumeMin = e.target.value
															? Number(e.target.value)
															: undefined;
														setPeriods(next);
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
														const next = [...periods];
														next[idx].volumeMax = e.target.value
															? Number(e.target.value)
															: undefined;
														setPeriods(next);
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
														const next = [...periods];
														next[idx].unitId = val === "NONE" ? undefined : val;
														setPeriods(next);
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
