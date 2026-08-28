import { useQueryClient } from "@tanstack/react-query";
import { Flame, Loader2, Plus, Save, Trash2, Zap } from "lucide-react";
import * as React from "react";
import { toast } from "sonner";
import { $api } from "@/api/client";
import type {
	Asal,
	BahanBakarEksisting,
	KebutuhanEnergiJenis,
	RencanaPemanfaatanGas,
	SaveSurveyEquipmentRequest,
	SaveSurveyMarketRequest,
	SaveSurveyProductRequest,
	SaveSurveyRawMaterialRequest,
	SaveSurveyRequest,
	SurveyDetail,
} from "@/api/types";
import { Badge } from "@/components/ui/badge";
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

interface SurveyKk0FormProps {
	companyId: string;
	initialData?: SurveyDetail | null;
	canEdit?: boolean;
	onSaved?: () => void;
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

	// Form state
	const [tanggalSurvey, setTanggalSurvey] = React.useState<string>(
		initialData?.tanggalSurvey || "",
	);
	const [surveyorUserId, setSurveyorUserId] = React.useState<string>(
		initialData?.surveyorUserId || "",
	);
	const [jumlahKaryawan, setJumlahKaryawan] = React.useState<string>(
		initialData?.jumlahKaryawan ? String(initialData.jumlahKaryawan) : "",
	);
	const [jumlahShift, setJumlahShift] = React.useState<string>(
		initialData?.jumlahShift ? String(initialData.jumlahShift) : "",
	);
	const [jamKerjaPerHari, setJamKerjaPerHari] = React.useState<string>(
		initialData?.jamKerjaPerHari ? String(initialData.jamKerjaPerHari) : "",
	);
	const [hariPerMinggu, setHariPerMinggu] = React.useState<string>(
		initialData?.hariPerMinggu ? String(initialData.hariPerMinggu) : "",
	);

	// Peak load
	const [bebanPuncak1Mulai, setBebanPuncak1Mulai] = React.useState<string>(
		initialData?.bebanPuncak1Mulai || "",
	);
	const [bebanPuncak1Selesai, setBebanPuncak1Selesai] = React.useState<string>(
		initialData?.bebanPuncak1Selesai || "",
	);
	const [bebanPuncak2Mulai, setBebanPuncak2Mulai] = React.useState<string>(
		initialData?.bebanPuncak2Mulai || "",
	);
	const [bebanPuncak2Selesai, setBebanPuncak2Selesai] = React.useState<string>(
		initialData?.bebanPuncak2Selesai || "",
	);

	// Energy needs & Pipeline
	const [kebutuhanEnergi, setKebutuhanEnergi] = React.useState<
		KebutuhanEnergiJenis | ""
	>(initialData?.kebutuhanEnergi || "");
	const [kebutuhanEnergiLainnya, setKebutuhanEnergiLainnya] =
		React.useState<string>(initialData?.kebutuhanEnergiLainnya || "");
	const [kapasitasNilai, setKapasitasNilai] = React.useState<string>(
		initialData?.kapasitasNilai ? String(initialData.kapasitasNilai) : "",
	);
	const [kapasitasUnitId, setKapasitasUnitId] = React.useState<string>(
		initialData?.kapasitasUnitId || "",
	);
	const [pemakaianNilai, setPemakaianNilai] = React.useState<string>(
		initialData?.pemakaianNilai ? String(initialData.pemakaianNilai) : "",
	);
	const [pemakaianUnitId, setPemakaianUnitId] = React.useState<string>(
		initialData?.pemakaianUnitId || "",
	);
	const [pipaTerdekatJarakM, setPipaTerdekatJarakM] = React.useState<string>(
		initialData?.pipaTerdekatJarakM
			? String(initialData.pipaTerdekatJarakM)
			: "",
	);
	const [pipaTerdekatDiameter, setPipaTerdekatDiameter] =
		React.useState<string>(
			initialData?.pipaTerdekatDiameter
				? String(initialData.pipaTerdekatDiameter)
				: "",
		);
	const [pipaTerdekatTekanan, setPipaTerdekatTekanan] = React.useState<string>(
		initialData?.pipaTerdekatTekanan
			? String(initialData.pipaTerdekatTekanan)
			: "",
	);
	const [bahanBakarEksisting, setBahanBakarEksisting] = React.useState<
		BahanBakarEksisting | ""
	>(initialData?.bahanBakarEksisting || "");
	const [namaPemasok, setNamaPemasok] = React.useState<string>(
		initialData?.namaPemasok || "",
	);
	const [kapasitasListrikKw, setKapasitasListrikKw] = React.useState<string>(
		initialData?.kapasitasListrikKw
			? String(initialData.kapasitasListrikKw)
			: "",
	);
	const [pemakaianListrikKwh, setPemakaianListrikKwh] = React.useState<string>(
		initialData?.pemakaianListrikKwh
			? String(initialData.pemakaianListrikKwh)
			: "",
	);
	const [rencanaPemanfaatanGas, setRencanaPemanfaatanGas] = React.useState<
		RencanaPemanfaatanGas | ""
	>(initialData?.rencanaPemanfaatanGas || "");
	const [deskripsiProsesProduksi, setDeskripsiProsesProduksi] =
		React.useState<string>(initialData?.deskripsiProsesProduksi || "");
	const [minEfisiensiDiharapkanPct, setMinEfisiensiDiharapkanPct] =
		React.useState<string>(
			initialData?.minEfisiensiDiharapkanPct
				? String(initialData.minEfisiensiDiharapkanPct)
				: "",
		);
	const [willingnessToPayUsdMmbtu, setWillingnessToPayUsdMmbtu] =
		React.useState<string>(
			initialData?.willingnessToPayUsdMmbtu
				? String(initialData.willingnessToPayUsdMmbtu)
				: "",
		);
	const [keteranganLain, setKeteranganLain] = React.useState<string>(
		initialData?.keteranganLain || "",
	);

	// Repeating Child Tables
	const [products, setProducts] = React.useState<SaveSurveyProductRequest[]>(
		initialData?.products?.map((p) => ({
			produk: p.produk,
			kapasitas: p.kapasitas != null ? Number(p.kapasitas) : undefined,
			hargaProduk: p.hargaProduk != null ? Number(p.hargaProduk) : undefined,
			catatan: p.catatan || undefined,
		})) || [],
	);

	const [rawMaterials, setRawMaterials] = React.useState<
		SaveSurveyRawMaterialRequest[]
	>(
		initialData?.rawMaterials?.map((m) => ({
			bahan: m.bahan || undefined,
			asal: m.asal || undefined,
			countryId: m.countryId || undefined,
			volume: m.volume != null ? Number(m.volume) : undefined,
			satuanUnitId: m.satuanUnitId || undefined,
		})) || [],
	);

	const [markets, setMarkets] = React.useState<SaveSurveyMarketRequest[]>(
		initialData?.markets?.map((m) => ({
			bahan: m.bahan || undefined,
			asal: m.asal || undefined,
			countryId: m.countryId || undefined,
			volume: m.volume != null ? Number(m.volume) : undefined,
			satuanUnitId: m.satuanUnitId || undefined,
		})) || [],
	);

	const [equipment, setEquipment] = React.useState<
		SaveSurveyEquipmentRequest[]
	>(
		initialData?.equipment?.map((e) => ({
			jenisPeralatan: e.jenisPeralatan,
			kapasitas: e.kapasitas != null ? Number(e.kapasitas) : undefined,
			kapasitasUnitId: e.kapasitasUnitId || undefined,
			jamPerHari: e.jamPerHari != null ? Number(e.jamPerHari) : undefined,
			hariPerMinggu:
				e.hariPerMinggu != null ? Number(e.hariPerMinggu) : undefined,
			fuelTypeId: e.fuelTypeId || undefined,
			hargaBahanBakar:
				e.hargaBahanBakar != null ? Number(e.hargaBahanBakar) : undefined,
			konsumsiPerBulan:
				e.konsumsiPerBulan != null ? Number(e.konsumsiPerBulan) : undefined,
			konsumsiUnitId: e.konsumsiUnitId || undefined,
			konversiKeGas: Number(e.konversiKeGas) || 0,
		})) || [],
	);

	// Synchronize when initialData updates
	React.useEffect(() => {
		if (initialData) {
			setTanggalSurvey(initialData.tanggalSurvey || "");
			setSurveyorUserId(initialData.surveyorUserId || "");
			setJumlahKaryawan(
				initialData.jumlahKaryawan ? String(initialData.jumlahKaryawan) : "",
			);
			setJumlahShift(
				initialData.jumlahShift ? String(initialData.jumlahShift) : "",
			);
			setJamKerjaPerHari(
				initialData.jamKerjaPerHari ? String(initialData.jamKerjaPerHari) : "",
			);
			setHariPerMinggu(
				initialData.hariPerMinggu ? String(initialData.hariPerMinggu) : "",
			);
			setBebanPuncak1Mulai(initialData.bebanPuncak1Mulai || "");
			setBebanPuncak1Selesai(initialData.bebanPuncak1Selesai || "");
			setBebanPuncak2Mulai(initialData.bebanPuncak2Mulai || "");
			setBebanPuncak2Selesai(initialData.bebanPuncak2Selesai || "");
			setKebutuhanEnergi(initialData.kebutuhanEnergi || "");
			setKebutuhanEnergiLainnya(initialData.kebutuhanEnergiLainnya || "");
			setKapasitasNilai(
				initialData.kapasitasNilai ? String(initialData.kapasitasNilai) : "",
			);
			setKapasitasUnitId(initialData.kapasitasUnitId || "");
			setPemakaianNilai(
				initialData.pemakaianNilai ? String(initialData.pemakaianNilai) : "",
			);
			setPemakaianUnitId(initialData.pemakaianUnitId || "");
			setPipaTerdekatJarakM(
				initialData.pipaTerdekatJarakM
					? String(initialData.pipaTerdekatJarakM)
					: "",
			);
			setPipaTerdekatDiameter(
				initialData.pipaTerdekatDiameter
					? String(initialData.pipaTerdekatDiameter)
					: "",
			);
			setPipaTerdekatTekanan(
				initialData.pipaTerdekatTekanan
					? String(initialData.pipaTerdekatTekanan)
					: "",
			);
			setBahanBakarEksisting(initialData.bahanBakarEksisting || "");
			setNamaPemasok(initialData.namaPemasok || "");
			setKapasitasListrikKw(
				initialData.kapasitasListrikKw
					? String(initialData.kapasitasListrikKw)
					: "",
			);
			setPemakaianListrikKwh(
				initialData.pemakaianListrikKwh
					? String(initialData.pemakaianListrikKwh)
					: "",
			);
			setRencanaPemanfaatanGas(initialData.rencanaPemanfaatanGas || "");
			setDeskripsiProsesProduksi(initialData.deskripsiProsesProduksi || "");
			setMinEfisiensiDiharapkanPct(
				initialData.minEfisiensiDiharapkanPct
					? String(initialData.minEfisiensiDiharapkanPct)
					: "",
			);
			setWillingnessToPayUsdMmbtu(
				initialData.willingnessToPayUsdMmbtu
					? String(initialData.willingnessToPayUsdMmbtu)
					: "",
			);
			setKeteranganLain(initialData.keteranganLain || "");

			if (initialData.products) {
				setProducts(
					initialData.products.map((p) => ({
						produk: p.produk,
						kapasitas: p.kapasitas != null ? Number(p.kapasitas) : undefined,
						hargaProduk:
							p.hargaProduk != null ? Number(p.hargaProduk) : undefined,
						catatan: p.catatan || undefined,
					})),
				);
			}
			if (initialData.rawMaterials) {
				setRawMaterials(
					initialData.rawMaterials.map((m) => ({
						bahan: m.bahan || undefined,
						asal: m.asal || undefined,
						countryId: m.countryId || undefined,
						volume: m.volume != null ? Number(m.volume) : undefined,
						satuanUnitId: m.satuanUnitId || undefined,
					})),
				);
			}
			if (initialData.markets) {
				setMarkets(
					initialData.markets.map((m) => ({
						bahan: m.bahan || undefined,
						asal: m.asal || undefined,
						countryId: m.countryId || undefined,
						volume: m.volume != null ? Number(m.volume) : undefined,
						satuanUnitId: m.satuanUnitId || undefined,
					})),
				);
			}
			if (initialData.equipment) {
				setEquipment(
					initialData.equipment.map((e) => ({
						jenisPeralatan: e.jenisPeralatan,
						kapasitas: e.kapasitas != null ? Number(e.kapasitas) : undefined,
						kapasitasUnitId: e.kapasitasUnitId || undefined,
						jamPerHari: e.jamPerHari != null ? Number(e.jamPerHari) : undefined,
						hariPerMinggu:
							e.hariPerMinggu != null ? Number(e.hariPerMinggu) : undefined,
						fuelTypeId: e.fuelTypeId || undefined,
						hargaBahanBakar:
							e.hargaBahanBakar != null ? Number(e.hargaBahanBakar) : undefined,
						konsumsiPerBulan:
							e.konsumsiPerBulan != null
								? Number(e.konsumsiPerBulan)
								: undefined,
						konsumsiUnitId: e.konsumsiUnitId || undefined,
						konversiKeGas: Number(e.konversiKeGas) || 0,
					})),
				);
			}
		}
	}, [initialData]);

	// Live sum of total gas conversion
	const totalGasConversion = React.useMemo(() => {
		return equipment.reduce(
			(sum, item) => sum + (Number(item.konversiKeGas) || 0),
			0,
		);
	}, [equipment]);

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

	const handleSubmit = (e: React.FormEvent) => {
		e.preventDefault();

		const request: SaveSurveyRequest = {
			tanggalSurvey: tanggalSurvey || null,
			surveyorUserId: surveyorUserId || null,
			jumlahKaryawan: jumlahKaryawan ? Number(jumlahKaryawan) : null,
			jumlahShift: jumlahShift ? Number(jumlahShift) : null,
			jamKerjaPerHari: jamKerjaPerHari ? Number(jamKerjaPerHari) : null,
			hariPerMinggu: hariPerMinggu ? Number(hariPerMinggu) : null,
			bebanPuncak1Mulai: bebanPuncak1Mulai || null,
			bebanPuncak1Selesai: bebanPuncak1Selesai || null,
			bebanPuncak2Mulai: bebanPuncak2Mulai || null,
			bebanPuncak2Selesai: bebanPuncak2Selesai || null,
			kebutuhanEnergi: kebutuhanEnergi ? kebutuhanEnergi : null,
			kebutuhanEnergiLainnya: kebutuhanEnergiLainnya || null,
			kapasitasNilai: kapasitasNilai ? Number(kapasitasNilai) : null,
			kapasitasUnitId: kapasitasUnitId || null,
			pemakaianNilai: pemakaianNilai ? Number(pemakaianNilai) : null,
			pemakaianUnitId: pemakaianUnitId || null,
			pipaTerdekatJarakM: pipaTerdekatJarakM
				? Number(pipaTerdekatJarakM)
				: null,
			pipaTerdekatDiameter: pipaTerdekatDiameter
				? Number(pipaTerdekatDiameter)
				: null,
			pipaTerdekatTekanan: pipaTerdekatTekanan
				? Number(pipaTerdekatTekanan)
				: null,
			bahanBakarEksisting: bahanBakarEksisting ? bahanBakarEksisting : null,
			namaPemasok: namaPemasok || null,
			kapasitasListrikKw: kapasitasListrikKw
				? Number(kapasitasListrikKw)
				: null,
			pemakaianListrikKwh: pemakaianListrikKwh
				? Number(pemakaianListrikKwh)
				: null,
			rencanaPemanfaatanGas: rencanaPemanfaatanGas
				? rencanaPemanfaatanGas
				: null,
			deskripsiProsesProduksi: deskripsiProsesProduksi || null,
			minEfisiensiDiharapkanPct: minEfisiensiDiharapkanPct
				? Number(minEfisiensiDiharapkanPct)
				: null,
			willingnessToPayUsdMmbtu: willingnessToPayUsdMmbtu
				? Number(willingnessToPayUsdMmbtu)
				: null,
			keteranganLain: keteranganLain || null,
		};

		saveMutation.mutate({
			params: { path: { id: companyId } },
			body: {
				request,
				products,
				rawMaterials,
				markets,
				equipment,
			},
		});
	};

	// Row Handlers
	const addProductRow = () => {
		setProducts([
			...products,
			{ produk: "", kapasitas: undefined, hargaProduk: undefined, catatan: "" },
		]);
	};
	const removeProductRow = (index: number) => {
		setProducts(products.filter((_, i) => i !== index));
	};

	const addRawMaterialRow = () => {
		setRawMaterials([
			...rawMaterials,
			{
				bahan: "",
				asal: "Domestik",
				countryId: undefined,
				volume: undefined,
				satuanUnitId: undefined,
			},
		]);
	};
	const removeRawMaterialRow = (index: number) => {
		setRawMaterials(rawMaterials.filter((_, i) => i !== index));
	};

	const addMarketRow = () => {
		setMarkets([
			...markets,
			{
				bahan: "",
				asal: "Domestik",
				countryId: undefined,
				volume: undefined,
				satuanUnitId: undefined,
			},
		]);
	};
	const removeMarketRow = (index: number) => {
		setMarkets(markets.filter((_, i) => i !== index));
	};

	const addEquipmentRow = () => {
		setEquipment([
			...equipment,
			{
				jenisPeralatan: "",
				kapasitas: undefined,
				kapasitasUnitId: undefined,
				jamPerHari: undefined,
				hariPerMinggu: undefined,
				fuelTypeId: undefined,
				hargaBahanBakar: undefined,
				konsumsiPerBulan: undefined,
				konsumsiUnitId: undefined,
				konversiKeGas: 0,
			},
		]);
	};
	const removeEquipmentRow = (index: number) => {
		setEquipment(equipment.filter((_, i) => i !== index));
	};

	return (
		<form onSubmit={handleSubmit} className="space-y-6">
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
						<p className="text-xs text-muted-foreground">
							Total Konversi Gas:{" "}
							<strong className="text-foreground font-mono">
								{totalGasConversion.toLocaleString("id-ID", {
									maximumFractionDigits: 2,
								})}{" "}
								MMBTU/Bulan
							</strong>
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
						Simpan Data Survei KK0
					</Button>
				)}
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
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Tanggal Survei</Label>
							<Input
								type="date"
								value={tanggalSurvey}
								onChange={(e) => setTanggalSurvey(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Surveyor */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Petugas Surveyor</Label>
							<Select
								value={surveyorUserId || "NONE"}
								onValueChange={(val) =>
									setSurveyorUserId(val === "NONE" ? "" : val)
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
						</div>

						{/* Jumlah Karyawan */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jumlah Karyawan</Label>
							<Input
								type="number"
								placeholder="contoh: 250"
								value={jumlahKaryawan}
								onChange={(e) => setJumlahKaryawan(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jumlah Shift */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jumlah Shift / Hari</Label>
							<Input
								type="number"
								placeholder="contoh: 3"
								value={jumlahShift}
								onChange={(e) => setJumlahShift(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jam Kerja Per Hari */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Jam Kerja / Hari</Label>
							<Input
								type="number"
								step="0.5"
								placeholder="contoh: 24"
								value={jamKerjaPerHari}
								onChange={(e) => setJamKerjaPerHari(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Hari Kerja Per Minggu */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">Hari Kerja / Minggu</Label>
							<Input
								type="number"
								placeholder="contoh: 7"
								value={hariPerMinggu}
								onChange={(e) => setHariPerMinggu(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
					</div>

					{/* Beban Puncak */}
					<div className="pt-2 border-t">
						<Label className="text-xs font-semibold mb-2 block text-muted-foreground">
							Jadwal Beban Puncak Operasi
						</Label>
						<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4">
							<div className="space-y-1">
								<Label className="text-[11px]">Beban Puncak 1 (Mulai)</Label>
								<Input
									type="time"
									value={bebanPuncak1Mulai}
									onChange={(e) => setBebanPuncak1Mulai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label className="text-[11px]">Beban Puncak 1 (Selesai)</Label>
								<Input
									type="time"
									value={bebanPuncak1Selesai}
									onChange={(e) => setBebanPuncak1Selesai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label className="text-[11px]">Beban Puncak 2 (Mulai)</Label>
								<Input
									type="time"
									value={bebanPuncak2Mulai}
									onChange={(e) => setBebanPuncak2Mulai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
							<div className="space-y-1">
								<Label className="text-[11px]">Beban Puncak 2 (Selesai)</Label>
								<Input
									type="time"
									value={bebanPuncak2Selesai}
									onChange={(e) => setBebanPuncak2Selesai(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
						</div>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 2: KEBUTUHAN ENERGI & INFRASTRUKTUR */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3">
					<CardTitle className="text-sm font-semibold">
						2. Kebutuhan Energi & Kedekatan Jalur Pipa
					</CardTitle>
					<CardDescription className="text-xs">
						Jenis energi eksisting, pasokan listrik, dan estimasi pipa gas
						terdekat
					</CardDescription>
				</CardHeader>
				<CardContent className="space-y-4">
					<div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
						{/* Kebutuhan Energi */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Jenis Kebutuhan Energi
							</Label>
							<Select
								value={kebutuhanEnergi || "NONE"}
								onValueChange={(val) =>
									setKebutuhanEnergi(
										val === "NONE" ? "" : (val as KebutuhanEnergiJenis),
									)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Jenis Energi" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Ditentukan</SelectItem>
									<SelectItem value="Listrik">Listrik</SelectItem>
									<SelectItem value="Steam">Steam</SelectItem>
									<SelectItem value="Panas">Panas / Thermal</SelectItem>
									<SelectItem value="Dingin">Dingin / Chiller</SelectItem>
									<SelectItem value="Lainnya">Lainnya</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Kebutuhan Energi Lainnya */}
						{kebutuhanEnergi === "Lainnya" && (
							<div className="space-y-1.5">
								<Label className="text-xs font-medium">
									Keterangan Energi Lainnya
								</Label>
								<Input
									placeholder="Sebutkan jenis energi..."
									value={kebutuhanEnergiLainnya}
									onChange={(e) => setKebutuhanEnergiLainnya(e.target.value)}
									disabled={!canEdit}
									className="text-xs h-9"
								/>
							</div>
						)}

						{/* Kapasitas Listrik (kW) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Kapasitas Listrik (kW)
							</Label>
							<Input
								type="number"
								placeholder="contoh: 1500"
								value={kapasitasListrikKw}
								onChange={(e) => setKapasitasListrikKw(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Pemakaian Listrik (kWh) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Pemakaian Listrik Bulanan (kWh)
							</Label>
							<Input
								type="number"
								placeholder="contoh: 350000"
								value={pemakaianListrikKwh}
								onChange={(e) => setPemakaianListrikKwh(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Jarak Pipa Terdekat (meter) */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Jarak Pipa Terdekat (meter)
							</Label>
							<Input
								type="number"
								step="0.1"
								placeholder="contoh: 250"
								value={pipaTerdekatJarakM}
								onChange={(e) => setPipaTerdekatJarakM(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Diameter Pipa */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Diameter Pipa Terdekat (inch)
							</Label>
							<Input
								type="number"
								step="0.5"
								placeholder="contoh: 6"
								value={pipaTerdekatDiameter}
								onChange={(e) => setPipaTerdekatDiameter(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Tekanan Pipa */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Tekanan Pipa Terdekat (barg)
							</Label>
							<Input
								type="number"
								step="0.1"
								placeholder="contoh: 4"
								value={pipaTerdekatTekanan}
								onChange={(e) => setPipaTerdekatTekanan(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Bahan Bakar Eksisting */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Bahan Bakar Eksisting
							</Label>
							<Select
								value={bahanBakarEksisting || "NONE"}
								onValueChange={(val) =>
									setBahanBakarEksisting(
										val === "NONE" ? "" : (val as BahanBakarEksisting),
									)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Bahan Bakar" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Ada</SelectItem>
									<SelectItem value="Solar">Solar / HSD</SelectItem>
									<SelectItem value="Batubara">Batubara</SelectItem>
									<SelectItem value="Lpg">LPG</SelectItem>
									<SelectItem value="Biomassa">Biomassa / Cangkang</SelectItem>
									<SelectItem value="Lainnya">Lainnya</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Nama Pemasok Eksisting */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Nama Pemasok BBM Eksisting
							</Label>
							<Input
								placeholder="contoh: Pertamina Patra Niaga"
								value={namaPemasok}
								onChange={(e) => setNamaPemasok(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Rencana Pemanfaatan Gas */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Rencana Pemanfaatan Gas
							</Label>
							<Select
								value={rencanaPemanfaatanGas || "NONE"}
								onValueChange={(val) =>
									setRencanaPemanfaatanGas(
										val === "NONE" ? "" : (val as RencanaPemanfaatanGas),
									)
								}
								disabled={!canEdit}
							>
								<SelectTrigger className="text-xs h-9">
									<SelectValue placeholder="Pilih Pemanfaatan Gas" />
								</SelectTrigger>
								<SelectContent>
									<SelectItem value="NONE">Belum Ditentukan</SelectItem>
									<SelectItem value="BahanBakar">
										Bahan Bakar Pembakaran
									</SelectItem>
									<SelectItem value="BahanBaku">
										Bahan Baku Proses Kimia
									</SelectItem>
									<SelectItem value="Pembangkit">
										Pembangkit Listrik Mandiri
									</SelectItem>
									<SelectItem value="Lainnya">Lainnya</SelectItem>
								</SelectContent>
							</Select>
						</div>

						{/* Willingness To Pay */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Willingness to Pay (USD/MMBTU)
							</Label>
							<Input
								type="number"
								step="0.01"
								placeholder="contoh: 9.50"
								value={willingnessToPayUsdMmbtu}
								onChange={(e) => setWillingnessToPayUsdMmbtu(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>

						{/* Min Efisiensi */}
						<div className="space-y-1.5">
							<Label className="text-xs font-medium">
								Target Efisiensi Biaya (%)
							</Label>
							<Input
								type="number"
								step="0.1"
								placeholder="contoh: 15"
								value={minEfisiensiDiharapkanPct}
								onChange={(e) => setMinEfisiensiDiharapkanPct(e.target.value)}
								disabled={!canEdit}
								className="text-xs h-9"
							/>
						</div>
					</div>

					{/* Deskripsi Proses Produksi */}
					<div className="space-y-1.5 pt-2">
						<Label className="text-xs font-medium">
							Deskripsi Alur Proses Produksi
						</Label>
						<Textarea
							placeholder="Jelaskan proses produksi dari bahan baku hingga barang jadi..."
							value={deskripsiProsesProduksi}
							onChange={(e) => setDeskripsiProsesProduksi(e.target.value)}
							disabled={!canEdit}
							className="text-xs min-h-[60px]"
						/>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 3: REPEATING EQUIPMENT & GAS CONVERSION TABLE */}
			<Card className="border-border/60 shadow-xs">
				<CardHeader className="pb-3 flex flex-row items-center justify-between">
					<div>
						<CardTitle className="text-sm font-semibold flex items-center gap-2">
							<Zap className="size-4 text-amber-500" />
							3. Tabel Peralatan Pembakar & Konversi Volume Gas
						</CardTitle>
						<CardDescription className="text-xs">
							Daftar burner/boiler dan perhitungan konversi gas bulanan (MMBTU)
						</CardDescription>
					</div>
					{canEdit && (
						<Button
							type="button"
							variant="outline"
							size="sm"
							onClick={addEquipmentRow}
							className="h-8 text-xs flex items-center gap-1"
						>
							<Plus className="size-3.5" /> Tambah Peralatan
						</Button>
					)}
				</CardHeader>
				<CardContent>
					<div className="border rounded-lg overflow-x-auto">
						<Table>
							<TableHeader className="bg-muted/50">
								<TableRow>
									<TableHead className="text-xs font-semibold min-w-[140px]">
										Jenis Peralatan
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[100px]">
										Kapasitas
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[110px]">
										Satuan
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[90px]">
										Jam/Hari
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[90px]">
										Hari/Minggu
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[130px]">
										Bahan Bakar
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[110px]">
										Konsumsi/Bln
									</TableHead>
									<TableHead className="text-xs font-semibold min-w-[130px]">
										Konversi Gas (MMBTU)
									</TableHead>
									{canEdit && (
										<TableHead className="text-xs font-semibold w-12 text-center">
											Aksi
										</TableHead>
									)}
								</TableRow>
							</TableHeader>
							<TableBody>
								{equipment.length === 0 ? (
									<TableRow>
										<TableCell
											colSpan={canEdit ? 9 : 8}
											className="h-20 text-center text-xs text-muted-foreground"
										>
											Belum ada data peralatan. Klik "+ Tambah Peralatan" untuk
											menambahkan.
										</TableCell>
									</TableRow>
								) : (
									equipment.map((row, idx) => (
										// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
										<TableRow key={idx}>
											<TableCell>
												<Input
													value={row.jenisPeralatan}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].jenisPeralatan = e.target.value;
														setEquipment(next);
													}}
													placeholder="Boiler 10 Ton"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													value={row.kapasitas ?? ""}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].kapasitas = e.target.value
															? Number(e.target.value)
															: undefined;
														setEquipment(next);
													}}
													placeholder="10"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Select
													value={row.kapasitasUnitId || "NONE"}
													onValueChange={(val) => {
														const next = [...equipment];
														next[idx].kapasitasUnitId =
															val === "NONE" ? undefined : val;
														setEquipment(next);
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
											<TableCell>
												<Input
													type="number"
													value={row.jamPerHari ?? ""}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].jamPerHari = e.target.value
															? Number(e.target.value)
															: undefined;
														setEquipment(next);
													}}
													placeholder="24"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													value={row.hariPerMinggu ?? ""}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].hariPerMinggu = e.target.value
															? Number(e.target.value)
															: undefined;
														setEquipment(next);
													}}
													placeholder="7"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Select
													value={row.fuelTypeId || "NONE"}
													onValueChange={(val) => {
														const next = [...equipment];
														next[idx].fuelTypeId =
															val === "NONE" ? undefined : val;
														setEquipment(next);
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
											<TableCell>
												<Input
													type="number"
													value={row.konsumsiPerBulan ?? ""}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].konsumsiPerBulan = e.target.value
															? Number(e.target.value)
															: undefined;
														setEquipment(next);
													}}
													placeholder="15000"
													disabled={!canEdit}
													className="text-xs h-8"
												/>
											</TableCell>
											<TableCell>
												<Input
													type="number"
													step="0.01"
													value={row.konversiKeGas}
													onChange={(e) => {
														const next = [...equipment];
														next[idx].konversiKeGas =
															Number(e.target.value) || 0;
														setEquipment(next);
													}}
													placeholder="500"
													disabled={!canEdit}
													className="text-xs h-8 font-mono font-medium"
												/>
											</TableCell>
											{canEdit && (
												<TableCell className="text-center">
													<Button
														type="button"
														variant="ghost"
														size="icon"
														onClick={() => removeEquipmentRow(idx)}
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

					<div className="mt-3 flex items-center justify-end gap-2 text-xs font-semibold">
						<span className="text-muted-foreground">Total Konversi Gas:</span>
						<Badge
							variant="outline"
							className="text-xs font-mono font-bold bg-emerald-50 text-emerald-700 border-emerald-300"
						>
							{totalGasConversion.toLocaleString("id-ID", {
								maximumFractionDigits: 2,
							})}{" "}
							MMBTU/Bulan
						</Badge>
					</div>
				</CardContent>
			</Card>

			{/* SECTION 4: PRODUK UTAMA, BAHAN BAKU & PASAR */}
			<div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
				{/* Produk Utama */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-xs font-semibold">
								4. Produk Utama
							</CardTitle>
							<CardDescription className="text-[11px]">
								Barang hasil produksi
							</CardDescription>
						</div>
						{canEdit && (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={addProductRow}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						{products.length === 0 ? (
							<p className="text-xs text-muted-foreground text-center py-4">
								Belum ada produk.
							</p>
						) : (
							products.map((p, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-2.5 border rounded-md space-y-2 bg-muted/20"
								>
									<div className="flex items-center justify-between">
										<span className="text-[11px] font-semibold text-muted-foreground">
											Produk #{idx + 1}
										</span>
										{canEdit && (
											<Button
												type="button"
												variant="ghost"
												size="icon"
												onClick={() => removeProductRow(idx)}
												className="size-6 text-destructive"
											>
												<Trash2 className="size-3" />
											</Button>
										)}
									</div>
									<Input
										value={p.produk}
										onChange={(e) => {
											const next = [...products];
											next[idx].produk = e.target.value;
											setProducts(next);
										}}
										placeholder="Nama Produk"
										disabled={!canEdit}
										className="text-xs h-7"
									/>
									<div className="grid grid-cols-2 gap-2">
										<Input
											type="number"
											value={p.kapasitas ?? ""}
											onChange={(e) => {
												const next = [...products];
												next[idx].kapasitas = e.target.value
													? Number(e.target.value)
													: undefined;
												setProducts(next);
											}}
											placeholder="Kapasitas"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
										<Input
											type="number"
											value={p.hargaProduk ?? ""}
											onChange={(e) => {
												const next = [...products];
												next[idx].hargaProduk = e.target.value
													? Number(e.target.value)
													: undefined;
												setProducts(next);
											}}
											placeholder="Harga (IDR)"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
								</div>
							))
						)}
					</CardContent>
				</Card>

				{/* Bahan Baku */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-xs font-semibold">
								5. Bahan Baku
							</CardTitle>
							<CardDescription className="text-[11px]">
								Bahan mentah proses
							</CardDescription>
						</div>
						{canEdit && (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={addRawMaterialRow}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						{rawMaterials.length === 0 ? (
							<p className="text-xs text-muted-foreground text-center py-4">
								Belum ada bahan baku.
							</p>
						) : (
							rawMaterials.map((m, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-2.5 border rounded-md space-y-2 bg-muted/20"
								>
									<div className="flex items-center justify-between">
										<span className="text-[11px] font-semibold text-muted-foreground">
											Bahan #{idx + 1}
										</span>
										{canEdit && (
											<Button
												type="button"
												variant="ghost"
												size="icon"
												onClick={() => removeRawMaterialRow(idx)}
												className="size-6 text-destructive"
											>
												<Trash2 className="size-3" />
											</Button>
										)}
									</div>
									<Input
										value={m.bahan ?? ""}
										onChange={(e) => {
											const next = [...rawMaterials];
											next[idx].bahan = e.target.value || undefined;
											setRawMaterials(next);
										}}
										placeholder="Nama Bahan Baku"
										disabled={!canEdit}
										className="text-xs h-7"
									/>
									<div className="grid grid-cols-2 gap-2">
										<Select
											value={m.asal || "Domestik"}
											onValueChange={(val) => {
												const next = [...rawMaterials];
												next[idx].asal = val as Asal;
												setRawMaterials(next);
											}}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-7">
												<SelectValue />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="Domestik">Domestik</SelectItem>
												<SelectItem value="Impor">Impor</SelectItem>
											</SelectContent>
										</Select>

										<Input
											type="number"
											value={m.volume ?? ""}
											onChange={(e) => {
												const next = [...rawMaterials];
												next[idx].volume = e.target.value
													? Number(e.target.value)
													: undefined;
												setRawMaterials(next);
											}}
											placeholder="Volume"
											disabled={!canEdit}
											className="text-xs h-7"
										/>
									</div>
								</div>
							))
						)}
					</CardContent>
				</Card>

				{/* Pasar Produk */}
				<Card className="border-border/60 shadow-xs">
					<CardHeader className="pb-3 flex flex-row items-center justify-between">
						<div>
							<CardTitle className="text-xs font-semibold">
								6. Orientasi Pasar
							</CardTitle>
							<CardDescription className="text-[11px]">
								Distribusi penjualan
							</CardDescription>
						</div>
						{canEdit && (
							<Button
								type="button"
								variant="outline"
								size="sm"
								onClick={addMarketRow}
								className="h-7 text-xs px-2"
							>
								<Plus className="size-3" />
							</Button>
						)}
					</CardHeader>
					<CardContent className="space-y-3">
						{markets.length === 0 ? (
							<p className="text-xs text-muted-foreground text-center py-4">
								Belum ada pasar produk.
							</p>
						) : (
							markets.map((m, idx) => (
								<div
									// biome-ignore lint/suspicious/noArrayIndexKey: dynamic form rows
									key={idx}
									className="p-2.5 border rounded-md space-y-2 bg-muted/20"
								>
									<div className="flex items-center justify-between">
										<span className="text-[11px] font-semibold text-muted-foreground">
											Pasar #{idx + 1}
										</span>
										{canEdit && (
											<Button
												type="button"
												variant="ghost"
												size="icon"
												onClick={() => removeMarketRow(idx)}
												className="size-6 text-destructive"
											>
												<Trash2 className="size-3" />
											</Button>
										)}
									</div>
									<Input
										value={m.bahan ?? ""}
										onChange={(e) => {
											const next = [...markets];
											next[idx].bahan = e.target.value || undefined;
											setMarkets(next);
										}}
										placeholder="Target Segmen / Komoditas"
										disabled={!canEdit}
										className="text-xs h-7"
									/>
									<div className="grid grid-cols-2 gap-2">
										<Select
											value={m.asal || "Domestik"}
											onValueChange={(val) => {
												const next = [...markets];
												next[idx].asal = val as Asal;
												setMarkets(next);
											}}
											disabled={!canEdit}
										>
											<SelectTrigger className="text-xs h-7">
												<SelectValue />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="Domestik">Domestik</SelectItem>
												<SelectItem value="Impor">Ekspor</SelectItem>
											</SelectContent>
										</Select>

										<Input
											type="number"
											value={m.volume ?? ""}
											onChange={(e) => {
												const next = [...markets];
												next[idx].volume = e.target.value
													? Number(e.target.value)
													: undefined;
												setMarkets(next);
											}}
											placeholder="Volume"
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

			{/* Keterangan Lain */}
			<div className="space-y-1.5">
				<Label className="text-xs font-medium">Catatan / Keterangan Lain</Label>
				<Textarea
					placeholder="Catatan tambahan hasil observasi lapangan atau surveyor..."
					value={keteranganLain}
					onChange={(e) => setKeteranganLain(e.target.value)}
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
						Simpan Data Survei KK0
					</Button>
				</div>
			)}
		</form>
	);
}
